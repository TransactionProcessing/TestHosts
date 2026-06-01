using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using SimpleResults;
using TestHosts.AgencyBanking.Models;
using TestHosts.AgencyBanking.Services;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.AgencyBanking.Endpoints;

public static class TransactionEndpoints {
    public static WebApplication MapAgencyBankingTransactionEndpoints(this WebApplication app) {
        app.MapPost("/api/agencybanking/transactions/deposit", TransactionHandlers.Deposit);

        app.MapPost("/api/agencybanking/transactions/withdrawal", TransactionHandlers.Withdrawal);

        app.MapPost("/api/agencybanking/transactions/balance-enquiry", TransactionHandlers.BalanceEnquiry);

        app.MapPost("/api/agencybanking/transactions/mini-statement", TransactionHandlers.MiniStatement);

        app.MapPost("/api/agencybanking/transactions/reversal", TransactionHandlers.Reversal);

        return app;
    }

    public static class TransactionHandlers {
        public static async Task<IResult> Deposit(DepositRequest request,
                                                       ITransactionService transactionService) {
            Result<TransactionResult> response = await transactionService.ProcessDeposit(request);

            if (response.IsFailed) {
                // TODO: Translate the response code to a user-friendly message
                TransactionResponse errorResponse = new()
                {
                    ResponseCode = response.Message
                };
                return Results.BadRequest(errorResponse);
            }

            return Results.Ok(response.Data);
        }

        public static async Task<IResult> Withdrawal(WithdrawalRequest request,
                                                          ITransactionService transactionService)
        {
            var response = await transactionService.ProcessWithdrawal(request);

            if (response.IsFailed)
            {
                // TODO: Translate the response code to a user-friendly message
                TransactionResponse errorResponse = new();
                return Results.BadRequest(errorResponse);
            }

            return Results.Ok(response.Data);
        }

        public static async Task<IResult> BalanceEnquiry(BalanceEnquiryRequest request,
                                                              ITransactionService transactionService)
        {
            var response = await transactionService.ProcessBalanceEnquiry(request);

            if (response.IsFailed)
            {
                // TODO: Translate the response code to a user-friendly message
                TransactionResponse errorResponse = new();
                return Results.BadRequest(errorResponse);
            }

            return Results.Ok(response.Data);
        }

        public static async Task<IResult> MiniStatement(MiniStatementRequest request,
                                                         ITransactionService transactionService) {
            var response = await transactionService.ProcessMiniStatement(request);

            if (response.IsFailed)
            {
                // TODO: Translate the response code to a user-friendly message
                TransactionResponse errorResponse = new();
                return Results.BadRequest(errorResponse);
            }

            return Results.Ok(response.Data);
        }

        public static async Task<IResult> Reversal(ReversalRequest request,
                                                    ITransactionService transactionService) {
            var response = await transactionService.ProcessReversal(request);

            if (response.IsFailed)
            {
                // TODO: Translate the response code to a user-friendly message
                TransactionResponse errorResponse = new();
                return Results.BadRequest(errorResponse);
            }

            return Results.Ok(response.Data);
        }
    }
}