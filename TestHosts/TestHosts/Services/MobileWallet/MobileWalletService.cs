namespace TestHosts.Services.MobileWallet
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using TestHosts.Database.MobileWallet;

    public sealed class MobileWalletService
    {
        private const Int32 PendingBatchSize = 25;
        private const Int32 MaxWebhookAttempts = 5;
        private const Int32 WebhookRetryDelaySeconds = 30;
        private const Decimal TestAsyncAmountLimit = 50000m;

        private readonly MobileWalletContext Context;
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory HttpClientFactory;

        public MobileWalletService(MobileWalletContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.Context = context;
            this.Configuration = configuration;
            this.HttpClientFactory = httpClientFactory;
        }

        public async Task EnsureSeedDataAsync(CancellationToken cancellationToken)
        {
            var defaultClientId = this.Configuration["MobileWallet:OAuth:DefaultClientId"] ?? "mobile-wallet-test-client";
            var defaultClientSecret = this.Configuration["MobileWallet:OAuth:DefaultClientSecret"] ?? "mobile-wallet-test-secret";

            MobileWalletClient? client = await this.Context.Clients.SingleOrDefaultAsync(c => c.ClientId == defaultClientId, cancellationToken);
            if (client == null)
            {
                await this.Context.Clients.AddAsync(new MobileWalletClient
                {
                    ClientId = defaultClientId,
                    ClientSecret = defaultClientSecret,
                    Name = "Default Mobile Wallet Test Client",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

                await this.Context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<MobileWalletAccessToken?> IssueTokenAsync(String clientId, String clientSecret, CancellationToken cancellationToken)
        {
            MobileWalletClient? client = await this.Context.Clients.SingleOrDefaultAsync(c => c.ClientId == clientId && c.IsActive, cancellationToken);
            if (client == null || client.ClientSecret != clientSecret)
            {
                return null;
            }

            Int32 tokenLifetimeMinutes = this.Configuration.GetValue<Int32?>("MobileWallet:OAuth:TokenLifetimeMinutes") ?? 60;

            MobileWalletAccessToken token = new()
            {
                TokenId = Guid.NewGuid(),
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-").TrimEnd('='),
                ClientId = client.ClientId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(tokenLifetimeMinutes)
            };

            await this.Context.AccessTokens.AddAsync(token, cancellationToken);
            await this.Context.SaveChangesAsync(cancellationToken);
            return token;
        }

        public async Task<MobileWalletClient?> AuthenticateAsync(String? bearerToken, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(bearerToken))
            {
                return null;
            }

            MobileWalletAccessToken? accessToken = await this.Context.AccessTokens.SingleOrDefaultAsync(t => t.Token == bearerToken, cancellationToken);
            if (accessToken == null || accessToken.ExpiresAt <= DateTime.UtcNow)
            {
                return null;
            }

            return await this.Context.Clients.SingleOrDefaultAsync(c => c.ClientId == accessToken.ClientId && c.IsActive, cancellationToken);
        }

        public async Task RecordAuditAsync(String resourceType,
                                           String resourceReference,
                                           String action,
                                           String actor,
                                           HttpContext? httpContext,
                                           String? idempotencyKey,
                                           String? requestPayload,
                                           String? responsePayload,
                                           CancellationToken cancellationToken)
        {
            await this.Context.AuditEntries.AddAsync(new MobileWalletAuditEntry
            {
                AuditEntryId = Guid.NewGuid(),
                ResourceType = resourceType,
                ResourceReference = resourceReference,
                Action = action,
                Actor = actor,
                HttpMethod = httpContext?.Request.Method ?? "SYSTEM",
                Path = httpContext?.Request.Path.Value ?? "background",
                IdempotencyKey = idempotencyKey,
                RequestPayload = requestPayload,
                ResponsePayload = responsePayload,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await this.Context.SaveChangesAsync(cancellationToken);
        }

        public async Task FinalizeTransactionAsync(MobileWalletTransaction transaction, String targetStatus, String? statusMessage, CancellationToken cancellationToken)
        {
            if (transaction.Status != MobileWalletStatus.Pending && transaction.Status != targetStatus)
            {
                return;
            }

            switch (targetStatus)
            {
                case MobileWalletStatus.Completed:
                    await this.ApplyTransactionAsync(transaction, cancellationToken);
                    await this.RecordAuditAsync("transaction",
                                                transaction.TransactionReference,
                                                "finalize",
                                                transaction.ClientId,
                                                null,
                                                transaction.IdempotencyKey,
                                                null,
                                                JsonConvert.SerializeObject(new
                                                {
                                                    transactionStatus = transaction.Status,
                                                    statusMessage = transaction.StatusMessage
                                                }),
                                                cancellationToken);
                    break;
                case MobileWalletStatus.Failed:
                case MobileWalletStatus.Rejected:
                    transaction.Status = targetStatus;
                    transaction.StatusMessage = statusMessage ?? "Transaction rejected by the test mobile wallet.";
                    transaction.CompletedAt = DateTime.UtcNow;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    await this.Context.SaveChangesAsync(cancellationToken);
                    await this.QueueWebhookDeliveriesAsync(transaction.ClientId,
                                                           $"transaction.{targetStatus}",
                                                           transaction.TransactionReference,
                                                           new
                                                           {
                                                               transactionReference = transaction.TransactionReference,
                                                               transactionStatus = transaction.Status,
                                                               statusMessage = transaction.StatusMessage
                                                            },
                                                            transaction.CallbackUrl,
                                                            cancellationToken);
                    await this.RecordAuditAsync("transaction",
                                                transaction.TransactionReference,
                                                "finalize",
                                                transaction.ClientId,
                                                null,
                                                transaction.IdempotencyKey,
                                                null,
                                                JsonConvert.SerializeObject(new
                                                {
                                                    transactionStatus = transaction.Status,
                                                    statusMessage = transaction.StatusMessage
                                                }),
                                                cancellationToken);
                    break;
            }
        }

        public async Task ProcessPendingTransactionsAsync(CancellationToken cancellationToken)
        {
            List<MobileWalletTransaction> pendingTransactions = await this.Context.Transactions.Where(t => t.Status == MobileWalletStatus.Pending)
                                                                                               .OrderBy(t => t.CreatedAt)
                                                                                               .Take(PendingBatchSize)
                                                                                               .ToListAsync(cancellationToken);

            foreach (MobileWalletTransaction transaction in pendingTransactions)
            {
                await this.FinalizeTransactionAsync(transaction, MobileWalletStatus.Completed, null, cancellationToken);
            }
        }

        public async Task FinalizeReversalAsync(MobileWalletReversal reversal, String targetStatus, String? statusMessage, CancellationToken cancellationToken)
        {
            if (reversal.Status != MobileWalletStatus.Pending && reversal.Status != targetStatus)
            {
                return;
            }

            if (targetStatus == MobileWalletStatus.Completed)
            {
                MobileWalletTransaction? originalTransaction = await this.Context.Transactions.SingleOrDefaultAsync(t => t.TransactionReference == reversal.OriginalTransactionReference, cancellationToken);
                if (originalTransaction == null || originalTransaction.BalanceApplied == false || originalTransaction.Status != MobileWalletStatus.Completed)
                {
                    reversal.Status = MobileWalletStatus.Failed;
                    reversal.StatusMessage = "Original transaction cannot be reversed.";
                }
                else
                {
                    MobileWalletAccount? debitAccount = await this.Context.Accounts.SingleOrDefaultAsync(a => a.AccountReference == originalTransaction.DebitAccountReference, cancellationToken);
                    MobileWalletAccount? creditAccount = await this.Context.Accounts.SingleOrDefaultAsync(a => a.AccountReference == originalTransaction.CreditAccountReference, cancellationToken);

                    if (debitAccount == null || creditAccount == null || creditAccount.AvailableBalance < originalTransaction.Amount)
                    {
                        reversal.Status = MobileWalletStatus.Failed;
                        reversal.StatusMessage = "Insufficient funds to process the reversal.";
                    }
                    else
                    {
                        creditAccount.AvailableBalance -= originalTransaction.Amount;
                        debitAccount.AvailableBalance += originalTransaction.Amount;
                        creditAccount.UpdatedAt = DateTime.UtcNow;
                        debitAccount.UpdatedAt = DateTime.UtcNow;
                        originalTransaction.Status = MobileWalletStatus.Reversed;
                        originalTransaction.StatusMessage = "Transaction reversed successfully.";
                        originalTransaction.UpdatedAt = DateTime.UtcNow;
                        reversal.Status = MobileWalletStatus.Completed;
                        reversal.StatusMessage = statusMessage ?? "Reversal completed successfully.";
                    }
                }
            }
            else
            {
                reversal.Status = targetStatus;
                reversal.StatusMessage = statusMessage ?? "Reversal rejected by the test mobile wallet.";
            }

            reversal.CompletedAt = DateTime.UtcNow;
            reversal.UpdatedAt = DateTime.UtcNow;
            await this.Context.SaveChangesAsync(cancellationToken);

            await this.QueueWebhookDeliveriesAsync(reversal.ClientId,
                                                   $"reversal.{reversal.Status}",
                                                   reversal.ReversalReference,
                                                   new
                                                   {
                                                       reversalReference = reversal.ReversalReference,
                                                       transactionReference = reversal.OriginalTransactionReference,
                                                       reversalStatus = reversal.Status,
                                                       statusMessage = reversal.StatusMessage
                                                   },
                                                   null,
                                                   cancellationToken);
            await this.RecordAuditAsync("reversal",
                                        reversal.ReversalReference,
                                        "finalize",
                                        reversal.ClientId,
                                        null,
                                        reversal.IdempotencyKey,
                                        null,
                                        JsonConvert.SerializeObject(new
                                        {
                                            reversalStatus = reversal.Status,
                                            statusMessage = reversal.StatusMessage
                                        }),
                                        cancellationToken);
        }

        public async Task ProcessPendingReversalsAsync(CancellationToken cancellationToken)
        {
            List<MobileWalletReversal> pendingReversals = await this.Context.Reversals.Where(r => r.Status == MobileWalletStatus.Pending)
                                                                                      .OrderBy(r => r.CreatedAt)
                                                                                      .Take(PendingBatchSize)
                                                                                      .ToListAsync(cancellationToken);

            foreach (MobileWalletReversal reversal in pendingReversals)
            {
                await this.FinalizeReversalAsync(reversal, MobileWalletStatus.Completed, null, cancellationToken);
            }
        }

        public async Task DeliverPendingWebhooksAsync(CancellationToken cancellationToken)
        {
            List<MobileWalletWebhookDelivery> deliveries = await this.Context.WebhookDeliveries.Where(d => d.Status == MobileWalletStatus.Pending &&
                                                                                                           (d.LastAttemptAt == null || d.LastAttemptAt <= DateTime.UtcNow.AddSeconds(-WebhookRetryDelaySeconds)) &&
                                                                                                           d.AttemptCount < MaxWebhookAttempts)
                                                                                               .OrderBy(d => d.CreatedAt)
                                                                                               .Take(PendingBatchSize)
                                                                                               .ToListAsync(cancellationToken);

            foreach (MobileWalletWebhookDelivery delivery in deliveries)
            {
                delivery.AttemptCount += 1;
                delivery.LastAttemptAt = DateTime.UtcNow;

                try
                {
                    HttpClient client = this.HttpClientFactory.CreateClient();
                    HttpRequestMessage request = new(HttpMethod.Post, delivery.CallbackUrl)
                    {
                        Content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json")
                    };
                    request.Headers.Add("X-Webhook-Event", delivery.EventType);
                    request.Headers.Add("X-Webhook-Delivery-Id", delivery.DeliveryId.ToString("N"));

                    HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        delivery.Status = "delivered";
                        delivery.DeliveredAt = DateTime.UtcNow;
                        delivery.LastError = null;
                    }
                    else
                    {
                        delivery.LastError = $"Webhook endpoint returned HTTP {(Int32)response.StatusCode}.";
                    }
                }
                catch (Exception ex)
                {
                    delivery.LastError = ex.Message;
                }

                await this.Context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task ApplyTransactionAsync(MobileWalletTransaction transaction, CancellationToken cancellationToken)
        {
            MobileWalletAccount? debitAccount = await this.Context.Accounts.SingleOrDefaultAsync(a => a.AccountReference == transaction.DebitAccountReference, cancellationToken);
            MobileWalletAccount? creditAccount = await this.Context.Accounts.SingleOrDefaultAsync(a => a.AccountReference == transaction.CreditAccountReference, cancellationToken);

            if (debitAccount == null || creditAccount == null)
            {
                transaction.Status = MobileWalletStatus.Failed;
                transaction.StatusMessage = "One or more transaction accounts were not found.";
            }
            else if (debitAccount.Status != MobileWalletStatus.Active || creditAccount.Status != MobileWalletStatus.Active)
            {
                transaction.Status = MobileWalletStatus.Failed;
                transaction.StatusMessage = "Both accounts must be active before a transaction can complete.";
            }
            else if (debitAccount.Currency != transaction.Currency || creditAccount.Currency != transaction.Currency)
            {
                transaction.Status = MobileWalletStatus.Failed;
                transaction.StatusMessage = "Transaction currency must match both wallet account currencies.";
            }
            else if (debitAccount.AvailableBalance < transaction.Amount)
            {
                transaction.Status = MobileWalletStatus.Failed;
                transaction.StatusMessage = "Insufficient balance on the debit account.";
            }
            else if (transaction.Amount >= TestAsyncAmountLimit)
            {
                transaction.Status = MobileWalletStatus.Failed;
                transaction.StatusMessage = "Amounts above the asynchronous test limit are rejected.";
            }
            else
            {
                if (transaction.BalanceApplied == false)
                {
                    debitAccount.AvailableBalance -= transaction.Amount;
                    creditAccount.AvailableBalance += transaction.Amount;
                    debitAccount.UpdatedAt = DateTime.UtcNow;
                    creditAccount.UpdatedAt = DateTime.UtcNow;
                    transaction.BalanceApplied = true;
                }

                transaction.Status = MobileWalletStatus.Completed;
                transaction.StatusMessage = "Transaction completed successfully.";
            }

            transaction.CompletedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            await this.Context.SaveChangesAsync(cancellationToken);

            await this.QueueWebhookDeliveriesAsync(transaction.ClientId,
                                                   $"transaction.{transaction.Status}",
                                                   transaction.TransactionReference,
                                                   new
                                                   {
                                                       transactionReference = transaction.TransactionReference,
                                                       transactionStatus = transaction.Status,
                                                       amount = transaction.Amount,
                                                       currency = transaction.Currency,
                                                       debitAccountReference = transaction.DebitAccountReference,
                                                       creditAccountReference = transaction.CreditAccountReference,
                                                       statusMessage = transaction.StatusMessage
                                                   },
                                                   transaction.CallbackUrl,
                                                   cancellationToken);
        }

        private async Task QueueWebhookDeliveriesAsync(String clientId,
                                                       String eventType,
                                                       String resourceReference,
                                                       Object payload,
                                                       String? callbackUrl,
                                                       CancellationToken cancellationToken)
        {
            String payloadJson = JsonConvert.SerializeObject(payload);
            HashSet<String> callbackUrls = new(StringComparer.OrdinalIgnoreCase);

            if (String.IsNullOrWhiteSpace(callbackUrl) == false)
            {
                callbackUrls.Add(callbackUrl);
            }

            List<MobileWalletWebhookSubscription> subscriptions = await this.Context.WebhookSubscriptions.Where(s => s.ClientId == clientId &&
                                                                                                                       s.IsActive &&
                                                                                                                       (s.EventType == "*" || s.EventType == eventType))
                                                                                                           .ToListAsync(cancellationToken);

            foreach (MobileWalletWebhookSubscription subscription in subscriptions)
            {
                callbackUrls.Add(subscription.CallbackUrl);
            }

            foreach (String targetUrl in callbackUrls)
            {
                await this.Context.WebhookDeliveries.AddAsync(new MobileWalletWebhookDelivery
                {
                    DeliveryId = Guid.NewGuid(),
                    ClientId = clientId,
                    EventType = eventType,
                    ResourceReference = resourceReference,
                    CallbackUrl = targetUrl,
                    Payload = payloadJson,
                    Status = MobileWalletStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            if (callbackUrls.Count > 0)
            {
                await this.Context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
