using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TestHosts.AgencyBanking.Database;
using TestHosts.AgencyBanking.Database.Entities;

namespace TestHosts.Endpoints
{
    public static class StaticQueryEndpoints
    {
        public static WebApplication MapStaticQueryEndpoints(this WebApplication app)
        {
            app.MapGet("/api/agencybanking/system/configuration", GetSystemConfiguration);
            app.MapGet("/api/agencybanking/glaccounts/{glCode}", GetGlAccountByCode);
            app.MapGet("/api/agencybanking/settlement/accounts/{accountNumber}", GetSettlementAccountByNumber);
            app.MapGet("/api/agencybanking/agents/super-agent/{agentId}", GetSuperAgentById);
            app.MapGet("/api/agencybanking/agents/{agentId}", GetAgentById);
            app.MapGet("/api/agencybanking/float/configuration/{agentId}", GetFloatConfigurationByAgentId);
            app.MapGet("/api/agencybanking/float-history/{agentId}/{count}", GetLastFloatEntries);
            app.MapGet("/api/agencybanking/customers/{accountNumber}", GetCustomerByAccountNumber);
            app.MapGet("/api/agencybanking/customers/{accountNumber}/balance", GetCustomerBalance);
            app.MapGet("/api/agencybanking/system/go-live/records", GetGoLiveRecord);
            app.MapGet("/api/agencybanking/transactions/{transactionId}", GetTransactionById);
            return app;
        }

        private static async Task<IResult> GetCustomerBalance(string accountNumber, AgencyBankingDbContext db) {
            var item = await db.Accounts.SingleOrDefaultAsync(c => c.AccountNumber == accountNumber);
            return item is null ? Results.NotFound() : Results.Ok(item.Balance);
        }

        // Handlers wired to AgencyBankingDbContext
        public static async Task<IResult> GetSystemConfiguration(AgencyBankingDbContext db)
        {
            var item = await db.SystemConfigurations.SingleOrDefaultAsync();
            return Results.Ok(item);
        }

        public static async Task<IResult> GetGlAccountByCode(string glCode, AgencyBankingDbContext db)
        {
            var item = await db.GlAccounts.SingleOrDefaultAsync(g => g.GlCode == glCode);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }

        public static async Task<IResult> GetSettlementAccountByNumber(string accountNumber, AgencyBankingDbContext db)
        {
            var item = await db.SettlementAccounts.SingleOrDefaultAsync(s => s.AccountNumber == accountNumber);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }

        public static async Task<IResult> GetSuperAgentById(string agentId, AgencyBankingDbContext db)
        {
            var item = await db.SuperAgents.SingleOrDefaultAsync(s => s.AgentId == agentId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }

        public static async Task<IResult> GetAgentById(string agentId, AgencyBankingDbContext db)
        {
            Agent item = await db.Agents.SingleOrDefaultAsync(a => a.AgentId == agentId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }

        public static async Task<IResult> GetFloatConfigurationByAgentId(string agentId, AgencyBankingDbContext db)
        {
            var item = await db.FloatConfigurations.SingleOrDefaultAsync(f => f.AgentId == agentId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }

        public static async Task<IResult> GetLastFloatEntries(string agentId, int count, AgencyBankingDbContext db)
        {
            if (count <= 0)
                return Results.BadRequest("count must be greater than zero.");

            var items = await db.FloatHistories
                                .Where(f => f.AgentId == agentId)
                                .OrderByDescending(f => f.CreatedAt)
                                .Take(count)
                                .ToListAsync();

            return Results.Ok(items);
        }

        public static async Task<IResult> GetCustomerByAccountNumber(string accountNumber, AgencyBankingDbContext db)
        {
            var item = await db.Customers.SingleOrDefaultAsync(c => c.AccountNumber == accountNumber);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }

        public static async Task<IResult> GetGoLiveRecord(AgencyBankingDbContext db)
        {
            var item = await db.GoLiveRecords.SingleOrDefaultAsync();
            return Results.Ok(item);
        }

        public static async Task<IResult> GetTransactionById(string transactionId, AgencyBankingDbContext db)
        {
            var item = await db.Transactions.Where(t => t.TransactionId == transactionId).OrderByDescending(t => t.CreatedAt).FirstAsync();
            return item is null ? Results.NotFound() : Results.Ok(item);
        }
    }
}
