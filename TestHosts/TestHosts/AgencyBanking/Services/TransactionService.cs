// ============================================================
// ENTERPRISE TRANSACTION SERVICE
// WITH FULL FLOAT SERVICE INTEGRATION
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SimpleResults;
using System;
using System.Linq;
using System.Threading.Tasks;
using TestHosts.AgencyBanking.Database;
using TestHosts.AgencyBanking.Database.Entities;
using TestHosts.AgencyBanking.Models;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.AgencyBanking.Services {


    public enum ResponseCodes
    {
        InvalidAgent = 1,
        AgentDisabled = 2,
        InvalidCustomerAccount = 3,
        DuplicateTransaction = 4,
        InsufficientFloat = 5,
        ReserveFloatFailed = 6,
        DebitFloatFailed = 7,
        InsufficientReservedFloat = 8,
        InvalidAccount = 9,
        InsufficientFunds = 10,
        OriginalTransactionNotFound = 11,
        InvalidAmount = 12,
        SystemError = 96
    }

    public interface ITransactionService {
        Task<Result<TransactionResult>> ProcessDeposit(DepositRequest request);

        Task<Result<TransactionResult>> ProcessWithdrawal(WithdrawalRequest request);

        Task<Result<BalanceEnquiryResult>> ProcessBalanceEnquiry(BalanceEnquiryRequest request);

        Task<Result<MiniStatementResult>> ProcessMiniStatement(MiniStatementRequest request);

        Task<Result<TransactionResult>> ProcessReversal(ReversalRequest request);
    }

// ============================================================
// IMPLEMENTATION
// ============================================================

    public class TransactionService : ITransactionService {
        private readonly AgencyBankingDbContext _db;

        private readonly IFloatService _floatService;

        private readonly ILedgerService _ledger;

        private readonly INotificationService _notifications;

        private readonly IAuditService _audit;

        private readonly ISettlementService _settlement;

        public TransactionService(AgencyBankingDbContext db,
                                  IFloatService floatService,
                                  ILedgerService ledger,
                                  INotificationService notifications,
                                  IAuditService audit,
                                  ISettlementService settlement) {
            this._db = db;
            this._floatService = floatService;
            this._ledger = ledger;
            this._notifications = notifications;
            this._audit = audit;
            this._settlement = settlement;
        }
        
        public async Task<Result<TransactionResult>> ProcessDeposit(DepositRequest request) {
            // ----------------------------------------------------
            // STEP 1: VALIDATE AGENT
            // ----------------------------------------------------

            Agent agent = await this._db.Agents.SingleOrDefaultAsync(x => x.AgentId == request.AgentId);

            if (agent == null) {
                await RecordFailedTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.InvalidAgent.ToString());
                return Result.Failure(ResponseCodes.InvalidAgent.ToString());
            }

            if (!agent.Active) {
                await RecordFailedTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.AgentDisabled.ToString());
                return Result.Failure(ResponseCodes.AgentDisabled.ToString());
            }

            // ----------------------------------------------------
            // STEP 2: VALIDATE CUSTOMER
            // ----------------------------------------------------

            Account customer = await this._db.Accounts.SingleOrDefaultAsync(x => x.AccountNumber == request.AccountNumber);

            if (customer == null) {
                await RecordFailedTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.InvalidCustomerAccount.ToString());
                return Result.Failure(ResponseCodes.InvalidCustomerAccount.ToString());
            }

            // ----------------------------------------------------
            // STEP 3: IDEMPOTENCY CHECK
            // ----------------------------------------------------

            TransactionEntity existing = await this._db.Transactions.SingleOrDefaultAsync(x => x.TransactionId == request.TransactionId);

            if (existing != null) {
                await RecordDuplicateTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.DuplicateTransaction.ToString(), existing.Id);
                return Result.Failure(ResponseCodes.DuplicateTransaction.ToString());
            }

            // ----------------------------------------------------
            // STEP 4: CHECK FLOAT
            // ----------------------------------------------------

            Result hasFloat = await this._floatService.HasSufficientFloat(request.AgentId, request.Amount);

            if (hasFloat.IsFailed) {
                await RecordFailedTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, hasFloat.Message ?? ResponseCodes.InsufficientFloat.ToString());
                return hasFloat;
            }

            // ----------------------------------------------------
            // STEP 5: RESERVE FLOAT
            // ----------------------------------------------------

            Result reserveResult = await this._floatService.ReserveFloat(request.AgentId, request.Amount, request.TransactionId);
            if (reserveResult.IsFailed) {
                // TODO: Log Error.....
                await RecordFailedTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, reserveResult.Message ?? ResponseCodes.ReserveFloatFailed.ToString());
                return reserveResult;
            }

            try {
                // ------------------------------------------------
                // STEP 6: DEBIT FLOAT
                // ------------------------------------------------

                Result debitResult = await this._floatService.DebitFloat(request.AgentId, request.Amount, request.TransactionId, $"Transaction {request.TransactionId}");

                // TODO: Log Error
                if (debitResult.IsFailed) {
                    await RecordFailedTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, debitResult.Message ?? ResponseCodes.DebitFloatFailed.ToString());

                    return debitResult;
                }

                // ------------------------------------------------
                // STEP 7: CREDIT CUSTOMER
                // ------------------------------------------------

                customer.Balance += request.Amount;

                // ------------------------------------------------
                // STEP 8: SAVE TRANSACTION
                // ------------------------------------------------

                TransactionEntity transaction = new TransactionEntity {
                    Id= Guid.NewGuid(),
                    TransactionId = request.TransactionId,
                    TransactionType = "CASH_IN",
                    AgentId = request.AgentId,
                    CustomerAccount = request.AccountNumber,
                    Amount = request.Amount,
                    Status = "COMPLETED",
                    ResponseCode = "00",
                    CreatedAt = DateTime.UtcNow
                };

                await this._db.Transactions.AddAsync(transaction);

                // ------------------------------------------------
                // STEP 9: LEDGER POSTING
                // ------------------------------------------------

                await this._ledger.Post(transactionId: request.TransactionId, debitAccount: "AGENT_FLOAT_GL", creditAccount: customer.AccountNumber, amount: request.Amount);

                // ------------------------------------------------
                // STEP 10: SETTLEMENT RECORD
                // ------------------------------------------------

                await this._settlement.RecordSettlement(new SettlementRecord {
                    TransactionId = request.TransactionId,
                    AgentId = request.AgentId,
                    Amount = request.Amount,
                    TransactionType = "CASH_IN",
                    SettlementStatus = "PENDING"
                });

                // ------------------------------------------------
                // STEP 11: AUDIT
                // ------------------------------------------------

                await this._audit.Log(request.TransactionId, "CASH_IN", "SUCCESS");

                // ------------------------------------------------
                // STEP 12: SAVE DATABASE
                // ------------------------------------------------

                await this._db.SaveChangesAsync();

                // ------------------------------------------------
                // STEP 14: RELEASE RESERVATION
                // ------------------------------------------------

                await this._floatService.ReleaseReservedFloat(request.AgentId, request.Amount, request.TransactionId);

                // ------------------------------------------------
                // STEP 15: SEND NOTIFICATION
                // ------------------------------------------------

                await this._notifications.Send(customer.AccountNumber, $"Cash-in successful. Amount: {request.Amount}");

                return Result.Success(new TransactionResult { TransactionId = request.TransactionId, ResponseCode = "0" });
            }
            catch (Exception ex) {
                await this._audit.Log(request.TransactionId, "DEPOSIT", $"FAILED: {ex.Message}");
                await RecordFailedTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.SystemError.ToString());

                return Result.Failure(ResponseCodes.SystemError.ToString());
            }
        }

        public async Task<Result<TransactionResult>> ProcessWithdrawal(WithdrawalRequest request) {
            Account customer = await this._db.Accounts.FirstOrDefaultAsync(x => x.AccountNumber == request.AccountNumber);

            if (customer == null) {
                await RecordFailedTransaction(request.TransactionId, "CASH_OUT", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.InvalidAccount.ToString());
                return Result.Failure(ResponseCodes.InvalidAccount.ToString());
            }

            if (customer.Balance < request.Amount) {
                await RecordFailedTransaction(request.TransactionId, "CASH_OUT", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.InsufficientFunds.ToString());
                return Result.Failure(ResponseCodes.InsufficientFunds.ToString());
            }

            try {
                // ------------------------------------------------
                // STEP 1: DEBIT CUSTOMER
                // ------------------------------------------------

                customer.Balance -= request.Amount;

                Result hasFloat = await this._floatService.HasSufficientFloat(request.AgentId, request.Amount);

                if (hasFloat.IsFailed)
                {
                    await RecordFailedTransaction(request.TransactionId, "CASH_OUT", request.AgentId, request.AccountNumber, request.Amount, hasFloat.Message ?? ResponseCodes.InsufficientFloat.ToString());
                    return hasFloat;
                }

                TransactionEntity existing = await this._db.Transactions.SingleOrDefaultAsync(x => x.TransactionId == request.TransactionId);

                if (existing != null)
                {
                    await RecordDuplicateTransaction(request.TransactionId, "CASH_IN", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.DuplicateTransaction.ToString(), existing.Id);
                    return Result.Failure(ResponseCodes.DuplicateTransaction.ToString());
                }

                // ----------------------------------------------------
                // STEP 5: RESERVE FLOAT
                // ----------------------------------------------------

                Result reserveResult = await this._floatService.ReserveFloat(request.AgentId, request.Amount, request.TransactionId);
                if (reserveResult.IsFailed)
                {
                    // TODO: Log Error.....
                    await RecordFailedTransaction(request.TransactionId, "CASH_OUT", request.AgentId, request.AccountNumber, request.Amount, reserveResult.Message ?? ResponseCodes.ReserveFloatFailed.ToString());
                    return reserveResult;
                }

                // ------------------------------------------------
                // STEP 2: CREDIT AGENT FLOAT
                // ------------------------------------------------

                var floatResult = await this._floatService.CreditFloat(request.AgentId, request.Amount, request.TransactionId, $"Transaction {request.TransactionId}");

                if (floatResult.IsFailed) {
                    await RecordFailedTransaction(request.TransactionId, "CASH_OUT", request.AgentId, request.AccountNumber, request.Amount, floatResult.Message ?? ResponseCodes.DebitFloatFailed.ToString());
                    return floatResult;
                }

                // ------------------------------------------------
                // STEP 3: SAVE TRANSACTION
                // ------------------------------------------------

                this._db.Transactions.Add(new TransactionEntity {
                    TransactionId = request.TransactionId,
                    TransactionType = "CASH_OUT",
                    AgentId = request.AgentId,
                    CustomerAccount = request.AccountNumber,
                    Amount = request.Amount,
                    Status = "COMPLETED",
                    ResponseCode = "00",
                    CreatedAt = DateTime.UtcNow
                });

                // ------------------------------------------------
                // STEP 4: LEDGER
                // ------------------------------------------------

                await this._ledger.Post(request.TransactionId, customer.AccountNumber, "AGENT_FLOAT_GL", request.Amount);

                // ------------------------------------------------
                // STEP 5: AUDIT
                // ------------------------------------------------

                await this._audit.Log(request.TransactionId, "CASH_OUT", "SUCCESS");

                await this._db.SaveChangesAsync();

                return Result.Success(new TransactionResult { TransactionId = request.TransactionId, ResponseCode = "00"});
            }
            catch (Exception ex) {
                await RecordFailedTransaction(request.TransactionId, "CASH_OUT", request.AgentId, request.AccountNumber, request.Amount, ResponseCodes.SystemError.ToString());
                return Result.Failure(ResponseCodes.SystemError.ToString());
            }
        }
        
        public async Task<Result<BalanceEnquiryResult>> ProcessBalanceEnquiry(BalanceEnquiryRequest request) {
            var account = await _db.Accounts.SingleOrDefaultAsync(x => x.AccountNumber == request.AccountNumber);

            if (account == null) {
                return Result.Failure(ResponseCodes.InvalidAccount.ToString());
            }

            return Result.Success(new BalanceEnquiryResult { ResponseCode = "00", ResponseMessage = "Success", AvailableBalance = account.Balance });
        }
        
        public async Task<Result<MiniStatementResult>> ProcessMiniStatement(MiniStatementRequest request) {
            var transactions = await _db.Transactions.Where(x => x.CustomerAccount == request.AccountNumber)
                .OrderByDescending(x => x.CreatedAt)
                .Take(request.Count)
                .Select(x => new Models.MiniStatementItem { TransactionDate = x.CreatedAt, TransactionType = x.TransactionType, Amount = x.Amount }).ToListAsync();

            return Result.Success(new MiniStatementResult { ResponseCode = "00", ResponseMessage = "Success", Transactions = transactions });
        }
        
        public async Task<Result<TransactionResult>> ProcessReversal(ReversalRequest request) {
            var original = await this._db.Transactions.FirstOrDefaultAsync(x => x.TransactionId == request.OriginalTransactionId);

            if (original == null) {
                await RecordFailedTransaction(request.TransactionId, "REVERSAL", request.TransactionId /* use provided id as agent? */, "", 0, ResponseCodes.OriginalTransactionNotFound.ToString());
                return Result.Failure(ResponseCodes.OriginalTransactionNotFound.ToString());
            }

            try {
                // ------------------------------------------------
                // REVERSE FLOAT
                // ------------------------------------------------

                if (original.TransactionType == "CASH_IN") {
                    await this._floatService.CreditFloat(original.AgentId, original.Amount, request.TransactionId, $"Reversal of Transaction {original.TransactionId}");
                }

                if (original.TransactionType == "CASH_OUT") {
                    await this._floatService.DebitFloat(original.AgentId, original.Amount, request.TransactionId, $"Reversal of Transaction {original.TransactionId}");
                }

                var customerAccount = await _db.Accounts.SingleOrDefaultAsync(x => x.AccountNumber == original.CustomerAccount);

                if (customerAccount == null)
                {
                    await RecordFailedTransaction(request.TransactionId, "REVERSAL", original.AgentId, original.CustomerAccount, original.Amount, ResponseCodes.InvalidAccount.ToString());
                    return Result.Failure(ResponseCodes.InvalidAccount.ToString());
                }

                if (original.Amount <= 0)
                {
                    await RecordFailedTransaction(request.TransactionId, "REVERSAL", original.AgentId, original.CustomerAccount, original.Amount, ResponseCodes.InvalidAmount.ToString());
                    return Result.Failure(ResponseCodes.InvalidAmount.ToString());
                }

                // CUSTOMER BALANCE UPDATE
                customerAccount.Balance += original.Amount;

                // ------------------------------------------------
                // REVERSE TRANSACTION
                // ------------------------------------------------

                original.Status = "REVERSED";

                // ------------------------------------------------
                // REVERSE LEDGER
                // ------------------------------------------------

                await this._ledger.Post(request.TransactionId, "REVERSAL_GL", "ORIGINAL_GL", original.Amount);

                // ------------------------------------------------
                // SAVE REVERSAL
                // ------------------------------------------------

                this._db.Transactions.Add(new TransactionEntity {
                    TransactionId = request.TransactionId,
                    TransactionType = "REVERSAL",
                    AgentId = original.AgentId,
                    CustomerAccount = original.CustomerAccount,
                    Amount = original.Amount,
                    Status = "COMPLETED",
                    ResponseCode = "00",
                    CreatedAt = DateTime.UtcNow
                });

                await this._db.SaveChangesAsync();

                return Result.Success(new TransactionResult { TransactionId = request.TransactionId, ResponseCode = "00", Success = true });
            }
            catch (Exception ex) {
                await RecordFailedTransaction(request.TransactionId, "REVERSAL", original?.AgentId ?? "", original?.CustomerAccount ?? "", original?.Amount ?? 0, ResponseCodes.SystemError.ToString());
                return Result.Failure(ResponseCodes.SystemError.ToString());
            }
        }

        private async Task RecordFailedTransaction(string transactionId, string transactionType, string agentId, string customerAccount, decimal amount, string responseCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                    return;

                var existing = await this._db.Transactions.SingleOrDefaultAsync(x => x.TransactionId == transactionId);
                if (existing != null)
                    return;

                var failed = new TransactionEntity
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionId,
                    TransactionType = transactionType,
                    AgentId = agentId ?? string.Empty,
                    CustomerAccount = customerAccount ?? string.Empty,
                    Amount = amount,
                    Status = "FAILED",
                    ResponseCode = responseCode ?? "96",
                    CreatedAt = DateTime.UtcNow
                };

                await this._db.Transactions.AddAsync(failed);
                await this._db.SaveChangesAsync();

                try { await this._audit.Log(transactionId, transactionType, "FAILED"); } catch { }
            }
            catch
            {
                // swallow; do not raise further errors when attempting to record failures
            }
        }

        private async Task RecordDuplicateTransaction(string transactionId, string transactionType, string agentId, string customerAccount, decimal amount, string responseCode, Guid isDuplicateOf)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionId))
                    return;

                var failed = new TransactionEntity
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionId,
                    TransactionType = transactionType,
                    AgentId = agentId ?? string.Empty,
                    CustomerAccount = customerAccount ?? string.Empty,
                    Amount = amount,
                    Status = "FAILED",
                    ResponseCode = responseCode ?? "96",
                    CreatedAt = DateTime.UtcNow,
                    IsDuplicate = true,
                    DuplicateOfId = isDuplicateOf
                };

                await this._db.Transactions.AddAsync(failed);
                await this._db.SaveChangesAsync();

                try { await this._audit.Log(transactionId, transactionType, "FAILED"); } catch { }
            }
            catch(Exception ex)
            {
                // swallow; do not raise further errors when attempting to record failures
                throw;
            }
        }
    }
}
