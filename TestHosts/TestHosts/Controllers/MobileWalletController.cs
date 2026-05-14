namespace TestHosts.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TestHosts.Database.MobileWallet;
    using TestHosts.Services.MobileWallet;

    [Route("mobilemoney/v1_0")]
    [ApiController]
    public sealed class MobileWalletController : ControllerBase
    {
        private readonly MobileWalletContext Context;
        private readonly MobileWalletService Service;

        public MobileWalletController(MobileWalletContext context, MobileWalletService service)
        {
            this.Context = context;
            this.Service = service;
        }

        [HttpPost("~/oauth/token")]
        public async Task<IActionResult> IssueToken([FromForm] MobileWalletTokenRequest request, CancellationToken cancellationToken)
        {
            if (request.grant_type != "client_credentials")
            {
                return this.BadRequest(new { error = "unsupported_grant_type" });
            }

            MobileWalletAccessToken? token = await this.Service.IssueTokenAsync(request.client_id, request.client_secret, cancellationToken);
            if (token == null)
            {
                return this.Unauthorized(new { error = "invalid_client" });
            }

            var response = new
            {
                access_token = token.Token,
                token_type = "Bearer",
                expires_in = Math.Max(0, Convert.ToInt32((token.ExpiresAt - DateTime.UtcNow).TotalSeconds))
            };

            await this.Service.RecordAuditAsync("oauth_token",
                                                token.TokenId.ToString("N"),
                                                "issue",
                                                request.client_id,
                                                this.HttpContext,
                                                null,
                                                JsonConvert.SerializeObject(request),
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateMobileWalletAccountRequest request, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            IActionResult? validationResult = this.ValidateAccountRequest(request);
            if (validationResult != null)
            {
                return validationResult;
            }

            if (await this.Context.Accounts.AnyAsync(a => a.AccountReference == request.accountReference, cancellationToken))
            {
                return this.Conflict(new { message = "An account with the same reference already exists." });
            }

            MobileWalletAccount account = new()
            {
                AccountReference = request.accountReference,
                AccountType = request.accountType ?? "wallet",
                Status = request.accountStatus ?? MobileWalletStatus.Active,
                Currency = request.currency,
                AvailableBalance = request.initialBalance ?? 0,
                GivenName = request.kycInformation?.givenName ?? String.Empty,
                FamilyName = request.kycInformation?.familyName ?? String.Empty,
                Msisdn = request.msisdn,
                EmailAddress = request.emailAddress,
                KycStatus = request.kycInformation?.kycStatus ?? "not_provided",
                IdentityType = request.kycInformation?.identity?.idDocument?.idType,
                IdentityNumber = request.kycInformation?.identity?.idDocument?.idNumber,
                DateOfBirth = request.kycInformation?.dateOfBirth,
                Nationality = request.kycInformation?.nationality,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await this.Context.Accounts.AddAsync(account, cancellationToken);
            await this.Context.SaveChangesAsync(cancellationToken);

            var response = this.MapAccount(account);
            await this.Service.RecordAuditAsync("account",
                                                account.AccountReference,
                                                "create",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                JsonConvert.SerializeObject(request),
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.CreatedAtAction(nameof(GetAccount), new { accountReference = account.AccountReference }, response);
        }

        [HttpGet("accounts/{accountReference}")]
        public async Task<IActionResult> GetAccount(String accountReference, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            MobileWalletAccount? account = await this.Context.Accounts.SingleOrDefaultAsync(a => a.AccountReference == accountReference, cancellationToken);
            if (account == null)
            {
                return this.NotFound();
            }

            var response = this.MapAccount(account);
            await this.Service.RecordAuditAsync("account",
                                                account.AccountReference,
                                                "view",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                null,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpGet("accounts/{accountReference}/balance")]
        public async Task<IActionResult> GetAccountBalance(String accountReference, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            MobileWalletAccount? account = await this.Context.Accounts.SingleOrDefaultAsync(a => a.AccountReference == accountReference, cancellationToken);
            if (account == null)
            {
                return this.NotFound();
            }

            var response = new
            {
                accountReference = account.AccountReference,
                availableBalance = account.AvailableBalance,
                currency = account.Currency
            };

            await this.Service.RecordAuditAsync("account_balance",
                                                account.AccountReference,
                                                "view",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                null,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpGet("accounts/{accountReference}/status")]
        public async Task<IActionResult> GetAccountStatus(String accountReference, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            MobileWalletAccount? account = await this.Context.Accounts.SingleOrDefaultAsync(a => a.AccountReference == accountReference, cancellationToken);
            if (account == null)
            {
                return this.NotFound();
            }

            var response = new
            {
                accountReference = account.AccountReference,
                accountStatus = account.Status
            };

            await this.Service.RecordAuditAsync("account_status",
                                                account.AccountReference,
                                                "view",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                null,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpGet("accounts/{accountReference}/transactions")]
        public async Task<IActionResult> GetAccountTransactions(String accountReference, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            if (await this.Context.Accounts.AnyAsync(a => a.AccountReference == accountReference, cancellationToken) == false)
            {
                return this.NotFound();
            }

            var transactions = await this.Context.Transactions.Where(t => t.DebitAccountReference == accountReference || t.CreditAccountReference == accountReference)
                                                              .OrderByDescending(t => t.CreatedAt)
                                                              .Select(t => new
                                                              {
                                                                  transactionReference = t.TransactionReference,
                                                                  transactionType = t.TransactionType,
                                                                  transactionStatus = t.Status,
                                                                  amount = t.Amount,
                                                                  currency = t.Currency,
                                                                  createdAt = t.CreatedAt,
                                                                  debitAccountReference = t.DebitAccountReference,
                                                                  creditAccountReference = t.CreditAccountReference
                                                              })
                                                              .ToListAsync(cancellationToken);

            List<Object> response = transactions.Cast<Object>().ToList();

            await this.Service.RecordAuditAsync("account_transactions",
                                                accountReference,
                                                "list",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                null,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpPost("transactions/type/{transactionType}")]
        public async Task<IActionResult> CreateTransaction(String transactionType, [FromBody] CreateMobileWalletTransactionRequest request, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            IActionResult? validationResult = this.ValidateTransactionRequest(request, transactionType);
            if (validationResult != null)
            {
                return validationResult;
            }

            String idempotencyKey = this.GetRequiredIdempotencyKey();
            if (String.IsNullOrWhiteSpace(idempotencyKey))
            {
                return this.BadRequest(new { message = "X-Idempotency-Key header is required." });
            }

            String requestPayload = JsonConvert.SerializeObject(request);
            MobileWalletTransaction? existingTransaction = await this.Context.Transactions.SingleOrDefaultAsync(t => t.ClientId == authResult.client!.ClientId &&
                                                                                                                       t.IdempotencyKey == idempotencyKey,
                                                                                                                 cancellationToken);

            if (existingTransaction != null)
            {
                if (existingTransaction.RequestPayload != requestPayload)
                {
                    return this.Conflict(new { message = "Idempotency key has already been used with a different request payload." });
                }

                return this.Accepted(this.MapTransaction(existingTransaction));
            }

            MobileWalletTransaction transaction = new()
            {
                TransactionReference = Guid.NewGuid().ToString("N"),
                ClientId = authResult.client!.ClientId,
                TransactionType = transactionType,
                Amount = request.amount,
                Currency = request.currency,
                DebitAccountReference = request.debitParty.accountReference,
                CreditAccountReference = request.creditParty.accountReference,
                ExternalReference = request.requestingOrganisationTransactionReference,
                IdempotencyKey = idempotencyKey,
                RequestPayload = requestPayload,
                CallbackUrl = request.callbackUrl,
                MetadataJson = request.metadata == null ? null : JsonConvert.SerializeObject(request.metadata),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await this.Context.Transactions.AddAsync(transaction, cancellationToken);
            await this.Context.SaveChangesAsync(cancellationToken);

            var response = this.MapTransaction(transaction);
            await this.Service.RecordAuditAsync("transaction",
                                                transaction.TransactionReference,
                                                "create",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                idempotencyKey,
                                                requestPayload,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Accepted(response);
        }

        [HttpGet("transactions/{transactionReference}")]
        public async Task<IActionResult> GetTransaction(String transactionReference, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            MobileWalletTransaction? transaction = await this.Context.Transactions.SingleOrDefaultAsync(t => t.TransactionReference == transactionReference, cancellationToken);
            if (transaction == null)
            {
                return this.NotFound();
            }

            var response = this.MapTransaction(transaction);
            await this.Service.RecordAuditAsync("transaction",
                                                transaction.TransactionReference,
                                                "view",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                null,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpPatch("transactions/{transactionReference}")]
        public async Task<IActionResult> UpdateTransaction(String transactionReference, [FromBody] UpdateMobileWalletTransactionRequest request, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            MobileWalletTransaction? transaction = await this.Context.Transactions.SingleOrDefaultAsync(t => t.TransactionReference == transactionReference, cancellationToken);
            if (transaction == null)
            {
                return this.NotFound();
            }

            if (this.IsSupportedTransactionStatus(request.transactionStatus) == false)
            {
                return this.BadRequest(new { message = $"transactionStatus must be one of {String.Join(", ", SupportedTransactionStatuses)}." });
            }

            if (transaction.Status != MobileWalletStatus.Pending && transaction.Status != request.transactionStatus)
            {
                return this.Conflict(new { message = "Only pending transactions can be updated." });
            }

            if (request.transactionStatus == MobileWalletStatus.Pending)
            {
                transaction.StatusMessage = request.statusMessage ?? transaction.StatusMessage;
                transaction.UpdatedAt = DateTime.UtcNow;
                await this.Context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await this.Service.FinalizeTransactionAsync(transaction, request.transactionStatus, request.statusMessage, cancellationToken);
            }

            var response = this.MapTransaction(transaction);
            await this.Service.RecordAuditAsync("transaction",
                                                transaction.TransactionReference,
                                                "update",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                JsonConvert.SerializeObject(request),
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpPost("reversals")]
        public async Task<IActionResult> CreateReversal([FromBody] CreateMobileWalletReversalRequest request, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            if (String.IsNullOrWhiteSpace(request.originalTransactionReference) || String.IsNullOrWhiteSpace(request.reversalReason))
            {
                return this.BadRequest(new { message = "originalTransactionReference and reversalReason are required." });
            }

            String idempotencyKey = this.GetRequiredIdempotencyKey();
            if (String.IsNullOrWhiteSpace(idempotencyKey))
            {
                return this.BadRequest(new { message = "X-Idempotency-Key header is required." });
            }

            String requestPayload = JsonConvert.SerializeObject(request);
            MobileWalletReversal? existingReversal = await this.Context.Reversals.SingleOrDefaultAsync(r => r.ClientId == authResult.client!.ClientId &&
                                                                                                               r.IdempotencyKey == idempotencyKey,
                                                                                                         cancellationToken);
            if (existingReversal != null)
            {
                if (existingReversal.RequestPayload != requestPayload)
                {
                    return this.Conflict(new { message = "Idempotency key has already been used with a different reversal request." });
                }

                return this.Accepted(this.MapReversal(existingReversal));
            }

            if (await this.Context.Transactions.AnyAsync(t => t.TransactionReference == request.originalTransactionReference, cancellationToken) == false)
            {
                return this.NotFound(new { message = "Original transaction not found." });
            }

            MobileWalletReversal reversal = new()
            {
                ReversalReference = Guid.NewGuid().ToString("N"),
                ClientId = authResult.client!.ClientId,
                OriginalTransactionReference = request.originalTransactionReference,
                Reason = request.reversalReason,
                IdempotencyKey = idempotencyKey,
                RequestPayload = requestPayload,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await this.Context.Reversals.AddAsync(reversal, cancellationToken);
            await this.Context.SaveChangesAsync(cancellationToken);

            var response = this.MapReversal(reversal);
            await this.Service.RecordAuditAsync("reversal",
                                                reversal.ReversalReference,
                                                "create",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                idempotencyKey,
                                                requestPayload,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Accepted(response);
        }

        [HttpGet("reversals/{reversalReference}")]
        public async Task<IActionResult> GetReversal(String reversalReference, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            MobileWalletReversal? reversal = await this.Context.Reversals.SingleOrDefaultAsync(r => r.ReversalReference == reversalReference, cancellationToken);
            if (reversal == null)
            {
                return this.NotFound();
            }

            var response = this.MapReversal(reversal);
            await this.Service.RecordAuditAsync("reversal",
                                                reversal.ReversalReference,
                                                "view",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                null,
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        [HttpPost("webhooks/subscriptions")]
        public async Task<IActionResult> CreateWebhookSubscription([FromBody] CreateMobileWalletWebhookSubscriptionRequest request, CancellationToken cancellationToken)
        {
            var authResult = await this.RequireClientAsync(cancellationToken);
            if (authResult.errorResult != null)
            {
                return authResult.errorResult;
            }

            if (Uri.TryCreate(request.callbackUrl, UriKind.Absolute, out _) == false)
            {
                return this.BadRequest(new { message = "callbackUrl must be an absolute URL." });
            }

            MobileWalletWebhookSubscription subscription = new()
            {
                SubscriptionId = Guid.NewGuid(),
                ClientId = authResult.client!.ClientId,
                EventType = String.IsNullOrWhiteSpace(request.eventType) ? "*" : request.eventType,
                CallbackUrl = request.callbackUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await this.Context.WebhookSubscriptions.AddAsync(subscription, cancellationToken);
            await this.Context.SaveChangesAsync(cancellationToken);

            var response = new
            {
                subscriptionId = subscription.SubscriptionId,
                eventType = subscription.EventType,
                callbackUrl = subscription.CallbackUrl,
                isActive = subscription.IsActive
            };

            await this.Service.RecordAuditAsync("webhook_subscription",
                                                subscription.SubscriptionId.ToString("N"),
                                                "create",
                                                authResult.client!.ClientId,
                                                this.HttpContext,
                                                null,
                                                JsonConvert.SerializeObject(request),
                                                JsonConvert.SerializeObject(response),
                                                cancellationToken);

            return this.Ok(response);
        }

        private async Task<(MobileWalletClient? client, IActionResult? errorResult)> RequireClientAsync(CancellationToken cancellationToken)
        {
            String authorizationHeader = this.Request.Headers.Authorization.ToString();
            String? bearerToken = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? authorizationHeader["Bearer ".Length..].Trim() : null;
            MobileWalletClient? client = await this.Service.AuthenticateAsync(bearerToken, cancellationToken);
            if (client == null)
            {
                this.Response.Headers["WWW-Authenticate"] = "Bearer";
                return (null, this.Unauthorized(new { message = "A valid OAuth2 bearer token is required." }));
            }

            return (client, null);
        }

        private IActionResult? ValidateAccountRequest(CreateMobileWalletAccountRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.accountReference))
            {
                return this.BadRequest(new { message = "accountReference is required." });
            }

            if (this.IsValidCurrency(request.currency) == false)
            {
                return this.BadRequest(new { message = "currency must be a three-letter ISO currency code." });
            }

            if (request.initialBalance < 0)
            {
                return this.BadRequest(new { message = "initialBalance cannot be negative." });
            }

            if (request.kycInformation?.kycStatus == "verified" &&
                (String.IsNullOrWhiteSpace(request.kycInformation.givenName) ||
                 String.IsNullOrWhiteSpace(request.kycInformation.familyName) ||
                 String.IsNullOrWhiteSpace(request.kycInformation.identity?.idDocument?.idType) ||
                 String.IsNullOrWhiteSpace(request.kycInformation.identity?.idDocument?.idNumber)))
            {
                return this.BadRequest(new { message = "Verified KYC accounts must include customer names and identity document details." });
            }

            return null;
        }

        private IActionResult? ValidateTransactionRequest(CreateMobileWalletTransactionRequest request, String transactionType)
        {
            if (String.IsNullOrWhiteSpace(transactionType))
            {
                return this.BadRequest(new { message = "transactionType is required." });
            }

            if (request.amount <= 0)
            {
                return this.BadRequest(new { message = "amount must be greater than zero." });
            }

            if (request.debitParty == null || String.IsNullOrWhiteSpace(request.debitParty.accountReference) ||
                request.creditParty == null || String.IsNullOrWhiteSpace(request.creditParty.accountReference))
            {
                return this.BadRequest(new { message = "debitParty.accountReference and creditParty.accountReference are required." });
            }

            if (request.debitParty.accountReference == request.creditParty.accountReference)
            {
                return this.BadRequest(new { message = "debit and credit accounts must be different." });
            }

            if (this.IsValidCurrency(request.currency) == false)
            {
                return this.BadRequest(new { message = "currency must be a three-letter ISO currency code." });
            }

            if (String.IsNullOrWhiteSpace(request.callbackUrl) == false &&
                Uri.TryCreate(request.callbackUrl, UriKind.Absolute, out _) == false)
            {
                return this.BadRequest(new { message = "callbackUrl must be an absolute URL." });
            }

            return null;
        }

        private String GetRequiredIdempotencyKey()
        {
            return this.Request.Headers.TryGetValue("X-Idempotency-Key", out var values) ? values.ToString().Trim() : String.Empty;
        }

        private Boolean IsValidCurrency(String? currency)
        {
            return String.IsNullOrWhiteSpace(currency) == false &&
                   currency.Length == 3 &&
                   currency.All(Char.IsLetter);
        }

        private static readonly String[] SupportedTransactionStatuses = new[] {
            MobileWalletStatus.Pending,
            MobileWalletStatus.Completed,
            MobileWalletStatus.Failed,
            MobileWalletStatus.Rejected
        };

        private Boolean IsSupportedTransactionStatus(String transactionStatus)
        {
            return SupportedTransactionStatuses.Contains(transactionStatus, StringComparer.Ordinal);
        }

        private Object MapAccount(MobileWalletAccount account)
        {
            return new
            {
                accountReference = account.AccountReference,
                accountType = account.AccountType,
                accountStatus = account.Status,
                currency = account.Currency,
                availableBalance = account.AvailableBalance,
                msisdn = account.Msisdn,
                emailAddress = account.EmailAddress,
                kycInformation = new
                {
                    givenName = account.GivenName,
                    familyName = account.FamilyName,
                    kycStatus = account.KycStatus,
                    nationality = account.Nationality,
                    dateOfBirth = account.DateOfBirth,
                    identity = String.IsNullOrWhiteSpace(account.IdentityType) || String.IsNullOrWhiteSpace(account.IdentityNumber)
                        ? null
                        : new
                        {
                            idDocument = new
                            {
                                idType = account.IdentityType,
                                idNumber = account.IdentityNumber
                            }
                        }
                }
            };
        }

        private Object MapTransaction(MobileWalletTransaction transaction)
        {
            return new
            {
                transactionReference = transaction.TransactionReference,
                transactionType = transaction.TransactionType,
                transactionStatus = transaction.Status,
                statusMessage = transaction.StatusMessage,
                amount = transaction.Amount,
                currency = transaction.Currency,
                debitParty = new { accountReference = transaction.DebitAccountReference },
                creditParty = new { accountReference = transaction.CreditAccountReference },
                requestingOrganisationTransactionReference = transaction.ExternalReference,
                callbackUrl = transaction.CallbackUrl,
                createdAt = transaction.CreatedAt,
                completedAt = transaction.CompletedAt
            };
        }

        private Object MapReversal(MobileWalletReversal reversal)
        {
            return new
            {
                reversalReference = reversal.ReversalReference,
                originalTransactionReference = reversal.OriginalTransactionReference,
                reversalStatus = reversal.Status,
                statusMessage = reversal.StatusMessage,
                reversalReason = reversal.Reason,
                createdAt = reversal.CreatedAt,
                completedAt = reversal.CompletedAt
            };
        }
    }

    public sealed class MobileWalletTokenRequest
    {
        [Required]
        public String grant_type { get; set; } = String.Empty;

        [Required]
        public String client_id { get; set; } = String.Empty;

        [Required]
        public String client_secret { get; set; } = String.Empty;
    }

    public sealed class CreateMobileWalletAccountRequest
    {
        [Required]
        [JsonProperty("accountReference")]
        public String accountReference { get; set; } = String.Empty;

        [Required]
        [JsonProperty("currency")]
        public String currency { get; set; } = String.Empty;

        [JsonProperty("accountType")]
        public String? accountType { get; set; }

        [JsonProperty("accountStatus")]
        public String? accountStatus { get; set; }

        [JsonProperty("initialBalance")]
        public Decimal? initialBalance { get; set; }

        [JsonProperty("msisdn")]
        public String? msisdn { get; set; }

        [JsonProperty("emailAddress")]
        public String? emailAddress { get; set; }

        [JsonProperty("kycInformation")]
        public MobileWalletKycInformation? kycInformation { get; set; }
    }

    public sealed class CreateMobileWalletTransactionRequest
    {
        [JsonProperty("amount")]
        public Decimal amount { get; set; }

        [Required]
        [JsonProperty("currency")]
        public String currency { get; set; } = String.Empty;

        [Required]
        [JsonProperty("debitParty")]
        public MobileWalletParty debitParty { get; set; } = new();

        [Required]
        [JsonProperty("creditParty")]
        public MobileWalletParty creditParty { get; set; } = new();

        [JsonProperty("requestingOrganisationTransactionReference")]
        public String? requestingOrganisationTransactionReference { get; set; }

        [JsonProperty("callbackUrl")]
        public String? callbackUrl { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<String, String>? metadata { get; set; }
    }

    public sealed class UpdateMobileWalletTransactionRequest
    {
        [Required]
        [JsonProperty("transactionStatus")]
        public String transactionStatus { get; set; } = String.Empty;

        [JsonProperty("statusMessage")]
        public String? statusMessage { get; set; }
    }

    public sealed class CreateMobileWalletReversalRequest
    {
        [Required]
        [JsonProperty("originalTransactionReference")]
        public String originalTransactionReference { get; set; } = String.Empty;

        [Required]
        [JsonProperty("reversalReason")]
        public String reversalReason { get; set; } = String.Empty;
    }

    public sealed class CreateMobileWalletWebhookSubscriptionRequest
    {
        [Required]
        [JsonProperty("callbackUrl")]
        public String callbackUrl { get; set; } = String.Empty;

        [JsonProperty("eventType")]
        public String? eventType { get; set; }
    }

    public sealed class MobileWalletParty
    {
        [Required]
        [JsonProperty("accountReference")]
        public String accountReference { get; set; } = String.Empty;
    }

    public sealed class MobileWalletKycInformation
    {
        [JsonProperty("givenName")]
        public String? givenName { get; set; }

        [JsonProperty("familyName")]
        public String? familyName { get; set; }

        [JsonProperty("kycStatus")]
        public String? kycStatus { get; set; }

        [JsonProperty("dateOfBirth")]
        public String? dateOfBirth { get; set; }

        [JsonProperty("nationality")]
        public String? nationality { get; set; }

        [JsonProperty("identity")]
        public MobileWalletIdentity? identity { get; set; }
    }

    public sealed class MobileWalletIdentity
    {
        [JsonProperty("idDocument")]
        public MobileWalletIdDocument? idDocument { get; set; }
    }

    public sealed class MobileWalletIdDocument
    {
        [JsonProperty("idType")]
        public String? idType { get; set; }

        [JsonProperty("idNumber")]
        public String? idNumber { get; set; }
    }
}
