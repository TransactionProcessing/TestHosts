using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestHosts.AgencyBanking.Database;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.AgencyBanking.Endpoints;

public static class AccountEndpoints {

    public static WebApplication MapAgencyBankingAccountEndpoints(this WebApplication app) {
        app.MapGet("/api/agencybanking/accounts/{accountNumber}/balance", async (string accountNumber,
                                                                                 AgencyBankingDbContext db) => {
            var account = await db.Accounts.FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);

            if (account == null) {
                return Results.BadRequest(new ApiResponse { ResponseCode = "14", ResponseMessage = "Invalid Account" });
            }

            return Results.Ok(new { responseCode = "00", balance = account.Balance, currency = account.Currency });
        });
        return app;
    }
}