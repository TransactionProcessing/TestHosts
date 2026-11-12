using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults.AspNetCore;
using TestHosts.AgencyBanking.Models;
using TestHosts.AgencyBanking.Services;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.AgencyBanking.Endpoints;

public static class TransactionEndpoints {
    public static WebApplication MapAgencyBankingTransactionEndpoints(this WebApplication app) {
        var transactions = app.MapGroup("/api/agencybanking/transactions").AddUnionResultFilter();

        transactions.MapPost("/deposit", TransactionHandlers.Deposit)
            .ProducesUnionResult<TransactionResult>();
        transactions.MapPost("/withdrawal", TransactionHandlers.Withdrawal)
            .ProducesUnionResult<TransactionResult>();
        transactions.MapPost("/balance-enquiry", TransactionHandlers.BalanceEnquiry)
            .ProducesUnionResult<BalanceEnquiryResult>();
        transactions.MapPost("/mini-statement", TransactionHandlers.MiniStatement)
            .ProducesUnionResult<MiniStatementResult>();
        transactions.MapPost("/reversal", TransactionHandlers.Reversal)
            .ProducesUnionResult<TransactionResult>();

        return app;
    }

    public static class TransactionHandlers {
        public static Task<Result<TransactionResult>> Deposit(DepositRequest request,
                                                               ITransactionService transactionService) =>
            transactionService.ProcessDeposit(request);

        public static Task<Result<TransactionResult>> Withdrawal(WithdrawalRequest request,
                                                                  ITransactionService transactionService) =>
            transactionService.ProcessWithdrawal(request);

        public static Task<Result<BalanceEnquiryResult>> BalanceEnquiry(BalanceEnquiryRequest request,
                                                                        ITransactionService transactionService) =>
            transactionService.ProcessBalanceEnquiry(request);

        public static Task<Result<MiniStatementResult>> MiniStatement(MiniStatementRequest request,
                                                                      ITransactionService transactionService) =>
            transactionService.ProcessMiniStatement(request);

        public static Task<Result<TransactionResult>> Reversal(ReversalRequest request,
                                                                ITransactionService transactionService) =>
            transactionService.ProcessReversal(request);
    }
}

