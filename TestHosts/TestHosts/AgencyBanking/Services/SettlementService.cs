using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestHosts.AgencyBanking.Database;
using TestHosts.AgencyBanking.Database.Entities;

namespace TestHosts.AgencyBanking.Services;

// ============================================================
// ENTERPRISE SETTLEMENT SERVICE
// AGENCY BANKING SETTLEMENT ENGINE
// ============================================================

using Microsoft.EntityFrameworkCore;

// ============================================================
// INTERFACE
// ============================================================

public interface ISettlementService
{
    Task RecordSettlement(SettlementRecord record);

    Task<SettlementBatchResult> RunEndOfDaySettlement(DateTime settlementDate);

    Task<SettlementBatchResult> RunAgentSettlement(string agentId, DateTime settlementDate);

    Task<List<SettlementRecord>> GetPendingSettlements();

    Task<SettlementSummary> GetSettlementSummary(DateTime settlementDate);

    Task ReverseSettlement(string transactionId);

    Task ReconcileSettlement(DateTime settlementDate);
}

public class SettlementService : ISettlementService
{
    private readonly AgencyBankingDbContext _db;

    private readonly IAuditService _audit;

    private readonly ILedgerService _ledger;

    public SettlementService(
        AgencyBankingDbContext db,
        IAuditService audit,
        ILedgerService ledger)
    {
        _db = db;
        _audit = audit;
        _ledger = ledger;
    }

    // ========================================================
    // RECORD SETTLEMENT ENTRY
    // ========================================================

    public async Task RecordSettlement(
        SettlementRecord record)
    {
        AgencyBankingServiceLogging.Started(
            "RecordSettlement",
            $"transactionId={record.TransactionId} agentId={record.AgentId} amount={record.Amount}");
        _db.SettlementRecords.Add(record);

        await _audit.Log(
            record.TransactionId,
            "SETTLEMENT_RECORDED",
            "SUCCESS");

        await _db.SaveChangesAsync();

        AgencyBankingServiceLogging.Completed(
            "RecordSettlement",
            $"transactionId={record.TransactionId} agentId={record.AgentId} amount={record.Amount}");
    }

    // ========================================================
    // END OF DAY SETTLEMENT
    // ========================================================

    public async Task<SettlementBatchResult>
        RunEndOfDaySettlement(
            DateTime settlementDate)
    {
        AgencyBankingServiceLogging.Started(
            "RunEndOfDaySettlement",
            $"settlementDate={settlementDate:o}");
        var batchId =
            Guid.NewGuid().ToString();

        var pending =
            await _db.SettlementRecords
                .Where(x =>
                    x.SettlementStatus == "PENDING" &&
                    x.CreatedAt.Date <=
                    settlementDate.Date)
                .ToListAsync();

        if (!pending.Any())
        {
            AgencyBankingServiceLogging.Warn(
                "RunEndOfDaySettlement",
                "no pending settlements",
                $"settlementDate={settlementDate:o}");
            return FailedBatch(
                batchId,
                "No pending settlements");
        }

        try
        {
            decimal totalAmount = 0;

            int successCount = 0;

            foreach (var record in pending)
            {
                // --------------------------------------------
                // PROCESS SETTLEMENT
                // --------------------------------------------

                record.SettlementStatus =
                    "SETTLED";

                record.SettledAt =
                    DateTime.UtcNow;

                record.BatchId =
                    batchId;

                totalAmount +=
                    record.Amount;

                successCount++;

                // --------------------------------------------
                // SETTLEMENT LEDGER
                // --------------------------------------------

                await _ledger.Post(
                    transactionId:
                        record.TransactionId,

                    debitAccount:
                        "SETTLEMENT_SUSPENSE_GL",

                    creditAccount:
                        "BANK_SETTLEMENT_GL",

                    amount:
                        record.Amount);
            }

            // --------------------------------------------
            // CREATE BATCH
            // --------------------------------------------

            var batch =
                new SettlementBatch
                {
                    BatchId = batchId,

                    SettlementDate =
                        settlementDate,

                    TotalTransactions =
                        successCount,

                    TotalAmount =
                        totalAmount,

                    Status =
                        "COMPLETED",

                    CreatedAt =
                        DateTime.UtcNow
                };

            _db.SettlementBatches.Add(batch);

            // --------------------------------------------
            // AUDIT
            // --------------------------------------------

            await _audit.Log(
                batchId,
                "SETTLEMENT_BATCH",
                "SUCCESS");

            await _db.SaveChangesAsync();

            AgencyBankingServiceLogging.Completed(
                "RunEndOfDaySettlement",
                $"settlementDate={settlementDate:o} batchId={batchId} totalTransactions={successCount} totalAmount={totalAmount}");
            return new SettlementBatchResult
            {
                Success = true,
                BatchId = batchId,
                TotalTransactions = successCount,
                TotalAmount = totalAmount,
                ResponseMessage =
                    "Settlement Successful"
            };
        }
        catch (Exception ex)
        {
            AgencyBankingServiceLogging.Failed(
                "RunEndOfDaySettlement",
                ex,
                $"settlementDate={settlementDate:o} batchId={batchId}");
            return FailedBatch(
                batchId,
                ex.Message);
        }
    }

    // ========================================================
    // AGENT SETTLEMENT
    // ========================================================

    public async Task<SettlementBatchResult>
        RunAgentSettlement(
            string agentId,
            DateTime settlementDate)
    {
        AgencyBankingServiceLogging.Started(
            "RunAgentSettlement",
            $"agentId={agentId} settlementDate={settlementDate:o}");
        var records =
            await _db.SettlementRecords
                .Where(x =>
                    x.AgentId == agentId &&
                    x.SettlementStatus == "PENDING")
                .ToListAsync();

        if (!records.Any())
        {
            AgencyBankingServiceLogging.Warn(
                "RunAgentSettlement",
                "no pending agent settlements",
                $"agentId={agentId} settlementDate={settlementDate:o}");
            return FailedBatch(
                "",
                "No pending agent settlements");
        }

        try
        {
            decimal total =
                records.Sum(x =>
                    x.Amount);

            foreach (var record in records)
            {
                record.SettlementStatus =
                    "SETTLED";

                record.SettledAt =
                    DateTime.UtcNow;
            }

            // --------------------------------------------
            // AGENT FLOAT RECONCILIATION
            // --------------------------------------------

            await _ledger.Post(
                transactionId:
                    Guid.NewGuid().ToString(),

                debitAccount:
                    "AGENT_SETTLEMENT_GL",

                creditAccount:
                    "BANK_SETTLEMENT_GL",

                amount:
                    total);

            await _db.SaveChangesAsync();

            AgencyBankingServiceLogging.Completed(
                "RunAgentSettlement",
                $"agentId={agentId} settlementDate={settlementDate:o} totalTransactions={records.Count} totalAmount={total}");
            return new SettlementBatchResult
            {
                Success = true,
                TotalTransactions =
                    records.Count,

                TotalAmount =
                    total,

                ResponseMessage =
                    "Agent Settlement Successful"
            };
        }
        catch (Exception ex)
        {
            AgencyBankingServiceLogging.Failed(
                "RunAgentSettlement",
                ex,
                $"agentId={agentId} settlementDate={settlementDate:o}");
            return FailedBatch(
                "",
                ex.Message);
        }
    }

    // ========================================================
    // GET PENDING
    // ========================================================

    public async Task<List<SettlementRecord>>
        GetPendingSettlements()
    {
        AgencyBankingServiceLogging.Started(
            "GetPendingSettlements");
        return await _db.SettlementRecords
            .Where(x =>
                x.SettlementStatus == "PENDING")
            .OrderBy(x =>
                x.CreatedAt)
            .ToListAsync();
    }

    // ========================================================
    // SETTLEMENT SUMMARY
    // ========================================================

    public async Task<SettlementSummary>
        GetSettlementSummary(
            DateTime settlementDate)
    {
        AgencyBankingServiceLogging.Started(
            "GetSettlementSummary",
            $"settlementDate={settlementDate:o}");
        var records =
            await _db.SettlementRecords
                .Where(x =>
                    x.CreatedAt.Date ==
                    settlementDate.Date)
                .ToListAsync();

        SettlementSummary summary = new SettlementSummary
        {
            SettlementDate =
                settlementDate,

            TotalTransactions =
                records.Count,

            TotalAmount =
                records.Sum(x =>
                    x.Amount),

            TotalCashIn =
                records
                    .Where(x =>
                        x.TransactionType ==
                        "CASH_IN")
                    .Sum(x =>
                        x.Amount),

            TotalCashOut =
                records
                    .Where(x =>
                        x.TransactionType ==
                        "CASH_OUT")
                    .Sum(x =>
                        x.Amount),

                TotalReversals =
                records
                    .Where(x =>
                        x.TransactionType ==
                        "REVERSAL")
                    .Sum(x =>
                        x.Amount)
        };

        AgencyBankingServiceLogging.Completed(
            "GetSettlementSummary",
            $"settlementDate={settlementDate:o} totalTransactions={records.Count} totalAmount={records.Sum(x => x.Amount)}");
        return summary;
    }

    // ========================================================
    // REVERSE SETTLEMENT
    // ========================================================

    public async Task ReverseSettlement(
        string transactionId)
    {
        AgencyBankingServiceLogging.Started(
            "ReverseSettlement",
            $"transactionId={transactionId}");
        var settlement =
            await _db.SettlementRecords
                .FirstOrDefaultAsync(x =>
                    x.TransactionId ==
                    transactionId);

        if (settlement == null)
        {
            AgencyBankingServiceLogging.Warn(
                "ReverseSettlement",
                "settlement not found",
                $"transactionId={transactionId}");
            throw new Exception(
                "Settlement Not Found");
        }

        settlement.SettlementStatus =
            "REVERSED";

        // --------------------------------------------
        // REVERSE LEDGER
        // --------------------------------------------

        await _ledger.Post(
            transactionId:
                Guid.NewGuid().ToString(),

            debitAccount:
                "BANK_SETTLEMENT_GL",

            creditAccount:
                "SETTLEMENT_SUSPENSE_GL",

            amount:
                settlement.Amount);

        await _audit.Log(
            transactionId,
            "SETTLEMENT_REVERSED",
            "SUCCESS");

        await _db.SaveChangesAsync();

        AgencyBankingServiceLogging.Completed(
            "ReverseSettlement",
            $"transactionId={transactionId}");
    }

    // ========================================================
    // RECONCILIATION
    // ========================================================

    public async Task ReconcileSettlement(
        DateTime settlementDate)
    {
        AgencyBankingServiceLogging.Started(
            "ReconcileSettlement",
            $"settlementDate={settlementDate:o}");
        var settlements =
            await _db.SettlementRecords
                .Where(x =>
                    x.CreatedAt.Date ==
                    settlementDate.Date)
                .ToListAsync();

        var ledgerTotal =
            await _db.LedgerEntries
                .Where(x =>
                    x.CreatedAt.Date ==
                    settlementDate.Date)
                .SumAsync(x =>
                    x.Amount);

        var settlementTotal =
            settlements.Sum(x =>
                x.Amount);

        if (ledgerTotal != settlementTotal)
        {
            AgencyBankingServiceLogging.Warn(
                "ReconcileSettlement",
                "reconciliation mismatch",
                $"settlementDate={settlementDate:o} ledgerTotal={ledgerTotal} settlementTotal={settlementTotal}");
            await _audit.Log(
                Guid.NewGuid().ToString(),
                "SETTLEMENT_RECON_FAILED",
                "FAILED");

            throw new Exception(
                "Settlement Reconciliation Failed");
        }

        await _audit.Log(
            Guid.NewGuid().ToString(),
            "SETTLEMENT_RECON_SUCCESS",
            "SUCCESS");

        AgencyBankingServiceLogging.Completed(
            "ReconcileSettlement",
            $"settlementDate={settlementDate:o} ledgerTotal={ledgerTotal} settlementTotal={settlementTotal}");
    }

    // ========================================================
    // HELPERS
    // ========================================================

    private SettlementBatchResult FailedBatch(
        string batchId,
        string message)
    {
        return new SettlementBatchResult
        {
            Success = false,
            BatchId = batchId,
            ResponseMessage = message
        };
    }
}

// ============================================================
// END OF DAY WORKER
// ============================================================

/*

public class SettlementWorker : BackgroundService
{
    private readonly IServiceProvider _services;

    public SettlementWorker(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            // RUN AT MIDNIGHT UTC
            if(now.Hour == 0 && now.Minute == 0)
            {
                using var scope =
                    _services.CreateScope();

                var settlement =
                    scope.ServiceProvider
                        .GetRequiredService<ISettlementService>();

                await settlement
                    .RunEndOfDaySettlement(
                        DateTime.UtcNow.Date);
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}

*/
