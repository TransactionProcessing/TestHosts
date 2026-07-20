using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleResults;
using TestHosts.AgencyBanking.Database;

namespace TestHosts.AgencyBanking.Services;

// ============================================================
// FLOAT SERVICE
// ENTERPRISE AGENCY BANKING FLOAT MANAGEMENT ENGINE
// ============================================================

// ============================================================
// INTERFACE
// ============================================================

public interface IFloatService
{
    Task<Result> DebitFloat(string agentId, decimal amount, string transactionId, string narrative);

    Task<Result> CreditFloat(string agentId, decimal amount, string transactionId, string narrative);

    Task<decimal> GetAvailableFloat(string agentId);

    Task<Result> ReserveFloat(string agentId, decimal amount, string transactionId);

    Task<Result> ReleaseReservedFloat(string agentId, decimal amount, string transactionId);

    Task<Result> HasSufficientFloat(string agentId, decimal amount);

    Task<FloatSummary> GetFloatSummary(string agentId);

    Task<List<FloatHistory>> GetFloatHistory(
        string agentId,
        DateTime fromDate,
        DateTime toDate);
}

// ============================================================
// IMPLEMENTATION
// ============================================================

public class FloatService : IFloatService
{
    private readonly AgencyBankingDbContext _db;
    private readonly IAuditService _audit;

    public FloatService(AgencyBankingDbContext db, IAuditService audit)
    {
        this._db = db;
        this._audit = audit;
    }

    // ========================================================
    // DEBIT FLOAT
    // ========================================================

    public async Task<Result> DebitFloat(string agentId, decimal amount,string transactionId, string narrative)
    {
        AgencyBankingServiceLogging.Started(
            "DebitFloat",
            $"agentId={agentId} transactionId={transactionId} amount={amount}");
        var agent = await this._db.Agents
            .FirstOrDefaultAsync(x =>
                x.AgentId == agentId);

        if (agent == null) {
            AgencyBankingServiceLogging.Warn(
                "DebitFloat",
                "invalid agent",
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.InvalidAgent.ToString());
        }

        if (!agent.Active) {
            AgencyBankingServiceLogging.Warn(
                "DebitFloat",
                "agent disabled",
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.AgentDisabled.ToString());
        }

        Decimal availableFloat = agent.FloatBalance - agent.ReservedFloat;

        if (availableFloat < amount) {
            AgencyBankingServiceLogging.Warn(
                "DebitFloat",
                "insufficient float",
                $"agentId={agentId} transactionId={transactionId} availableFloat={availableFloat} amount={amount}");
            return Result.Failure(ResponseCodes.InsufficientFloat.ToString());
        }

        try
        {
            var openingBalance = agent.FloatBalance;

            agent.FloatBalance -= amount;

            var closingBalance = agent.FloatBalance;

            this._db.FloatHistories.Add(new Database.Entities.FloatHistory
            {
                AgentId = agentId,
                TransactionId = transactionId,
                OperationType = (Int32)FloatOperationType.DEBIT,
                Amount = amount,
                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                Narrative = narrative,
                CreatedAt = DateTime.UtcNow
            });

            await this._audit.Log(transactionId, "FLOAT_DEBIT", "SUCCESS");

            await this._db.SaveChangesAsync();

            AgencyBankingServiceLogging.Completed(
                "DebitFloat",
                $"agentId={agentId} transactionId={transactionId} balance={closingBalance}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            AgencyBankingServiceLogging.Failed(
                "DebitFloat",
                ex,
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.SystemError.ToString());
        }
    }

    // ========================================================
    // CREDIT FLOAT
    // ========================================================

    public async Task<Result> CreditFloat(string agentId,
                                          decimal amount,
                                          string transactionId,
                                          string narrative)
    {
        AgencyBankingServiceLogging.Started(
            "CreditFloat",
            $"agentId={agentId} transactionId={transactionId} amount={amount}");
        var agent = await this._db.Agents
            .FirstOrDefaultAsync(x =>
                x.AgentId == agentId);

        if (agent == null) {
            AgencyBankingServiceLogging.Warn(
                "CreditFloat",
                "invalid agent",
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.InvalidAgent.ToString());
        }

        try
        {
            var openingBalance = agent.FloatBalance;

            agent.FloatBalance += amount;

            var closingBalance = agent.FloatBalance;

            this._db.FloatHistories.Add(new Database.Entities.FloatHistory
            {
                AgentId = agentId,
                TransactionId = transactionId,
                OperationType = (Int32)FloatOperationType.CREDIT,
                Amount = amount,
                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                Narrative = narrative,
                CreatedAt = DateTime.UtcNow
            });

            await this._audit.Log(
                transactionId,
                "FLOAT_CREDIT",
                "SUCCESS");

            await this._db.SaveChangesAsync();
                        
            AgencyBankingServiceLogging.Completed(
                "CreditFloat",
                $"agentId={agentId} transactionId={transactionId} balance={closingBalance}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            AgencyBankingServiceLogging.Failed(
                "CreditFloat",
                ex,
                $"agentId={agentId} transactionId={transactionId}");

            return Result.Failure(ResponseCodes.SystemError.ToString());
        }
    }

    // ========================================================
    // RESERVE FLOAT
    // ========================================================

    public async Task<Result> ReserveFloat(string agentId,
                                           decimal amount,
                                           string transactionId)
    {
        AgencyBankingServiceLogging.Started(
            "ReserveFloat",
            $"agentId={agentId} transactionId={transactionId} amount={amount}");
        var agent = await this._db.Agents
            .FirstOrDefaultAsync(x =>
                x.AgentId == agentId);

        if (agent == null)
        {
            AgencyBankingServiceLogging.Warn(
                "ReserveFloat",
                "invalid agent",
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.InvalidAgent.ToString());
        }

        var available =
            agent.FloatBalance - agent.ReservedFloat;

        if (available < amount)
        {
            AgencyBankingServiceLogging.Warn(
                "ReserveFloat",
                "insufficient float",
                $"agentId={agentId} transactionId={transactionId} availableFloat={available} amount={amount}");
            return Result.Failure(ResponseCodes.InsufficientFloat.ToString());
        }

        try
        {
            agent.ReservedFloat += amount;

            this._db.FloatReservations.Add(new Database.Entities.FloatReservation
            {
                AgentId = agentId,
                TransactionId = transactionId,
                Amount = amount,
                Status = "RESERVED",
                CreatedAt = DateTime.UtcNow
            });

            await this._audit.Log(
                transactionId,
                "FLOAT_RESERVED",
                "SUCCESS");

            await this._db.SaveChangesAsync();

            AgencyBankingServiceLogging.Completed(
                "ReserveFloat",
                $"agentId={agentId} transactionId={transactionId} reservedFloat={agent.ReservedFloat}");
            return Result.Success();
        }
        catch (Exception ex)
        {            
            AgencyBankingServiceLogging.Failed(
                "ReserveFloat",
                ex,
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.SystemError.ToString());
        }
    }

    // ========================================================
    // RELEASE RESERVED FLOAT
    // ========================================================

    public async Task<Result> ReleaseReservedFloat(
        string agentId,
        decimal amount,
        string transactionId)
    {
        AgencyBankingServiceLogging.Started(
            "ReleaseReservedFloat",
            $"agentId={agentId} transactionId={transactionId} amount={amount}");
        var agent = await this._db.Agents
            .FirstOrDefaultAsync(x =>
                x.AgentId == agentId);

        if (agent == null)
        {
            AgencyBankingServiceLogging.Warn(
                "ReleaseReservedFloat",
                "invalid agent",
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.InvalidAgent.ToString());
        }

        if (agent.ReservedFloat < amount)
        {
            AgencyBankingServiceLogging.Warn(
                "ReleaseReservedFloat",
                "insufficient reserved float",
                $"agentId={agentId} transactionId={transactionId} reservedFloat={agent.ReservedFloat} amount={amount}");
            return Result.Failure(ResponseCodes.InsufficientReservedFloat.ToString());
        }

        try
        {
            agent.ReservedFloat -= amount;

            var reservation = await this._db.FloatReservations
                    .SingleOrDefaultAsync(x =>
                        x.TransactionId == transactionId);

            if (reservation != null)
            {
                reservation.Status = "RELEASED";
            }

            await this._audit.Log(
                transactionId,
                "FLOAT_RELEASED",
                "SUCCESS");

            await this._db.SaveChangesAsync();

            AgencyBankingServiceLogging.Completed(
                "ReleaseReservedFloat",
                $"agentId={agentId} transactionId={transactionId} reservedFloat={agent.ReservedFloat}");
            return Result.Success();
        }
        catch (Exception ex)
        {
            AgencyBankingServiceLogging.Failed(
                "ReleaseReservedFloat",
                ex,
                $"agentId={agentId} transactionId={transactionId}");
            return Result.Failure(ResponseCodes.SystemError.ToString());
        }
    }

    // ========================================================
    // CHECK FLOAT
    // ========================================================

    public async Task<Result> HasSufficientFloat(string agentId, decimal amount)
    {
        AgencyBankingServiceLogging.Started(
            "HasSufficientFloat",
            $"agentId={agentId} amount={amount}");
        var agent = await this._db.Agents
            .FirstOrDefaultAsync(x =>
                x.AgentId == agentId);

        if (agent == null)
        {
            AgencyBankingServiceLogging.Warn(
                "HasSufficientFloat",
                "invalid agent",
                $"agentId={agentId}");
            return Result.Failure(ResponseCodes.InvalidAgent.ToString());
        }

        var available = agent.FloatBalance - agent.ReservedFloat;

        Result result = available >= amount
            ? Result.Success()
            : Result.Failure(ResponseCodes.InsufficientFloat.ToString());

        AgencyBankingServiceLogging.Completed(
            "HasSufficientFloat",
            $"agentId={agentId} amount={amount} availableFloat={available} result={(result.IsFailed ? "failed" : "success")}");
        return result;
    }

    // ========================================================
    // GET AVAILABLE FLOAT
    // ========================================================

    public async Task<decimal> GetAvailableFloat(
        string agentId)
    {
        AgencyBankingServiceLogging.Started(
            "GetAvailableFloat",
            $"agentId={agentId}");
        var agent = await this._db.Agents
            .FirstOrDefaultAsync(x =>
                x.AgentId == agentId);

        if (agent == null)
        {
            AgencyBankingServiceLogging.Warn(
                "GetAvailableFloat",
                "invalid agent",
                $"agentId={agentId}");
            return 0;
        }

        decimal result = agent.FloatBalance -
               agent.ReservedFloat;
        AgencyBankingServiceLogging.Completed(
            "GetAvailableFloat",
            $"agentId={agentId} availableFloat={result}");
        return result;
    }

    // ========================================================
    // FLOAT SUMMARY
    // ========================================================

    public async Task<FloatSummary> GetFloatSummary(
        string agentId)
    {
        AgencyBankingServiceLogging.Started(
            "GetFloatSummary",
            $"agentId={agentId}");
        var agent = await this._db.Agents
            .FirstOrDefaultAsync(x =>
                x.AgentId == agentId);

        if (agent == null)
        {
            AgencyBankingServiceLogging.Warn(
                "GetFloatSummary",
                "invalid agent",
                $"agentId={agentId}");
            return new FloatSummary();
        }

        FloatSummary summary = new FloatSummary
        {
            AgentId = agent.AgentId,
            TotalFloat = agent.FloatBalance,
            ReservedFloat = agent.ReservedFloat,
            AvailableFloat =
                agent.FloatBalance -
                agent.ReservedFloat
        };

        AgencyBankingServiceLogging.Completed(
            "GetFloatSummary",
            $"agentId={agentId} availableFloat={summary.AvailableFloat}");
        return summary;
    }

    // ========================================================
    // FLOAT HISTORY
    // ========================================================

    public async Task<List<FloatHistory>> GetFloatHistory(
        string agentId,
        DateTime fromDate,
        DateTime toDate)
    {
        AgencyBankingServiceLogging.Started(
            "GetFloatHistory",
            $"agentId={agentId} fromDate={fromDate:o} toDate={toDate:o}");
        
        List<FloatHistory> history = await this._db.FloatHistories
            .Where(x =>
                x.AgentId == agentId &&
                x.CreatedAt >= fromDate &&
                x.CreatedAt <= toDate)
            .OrderByDescending(x =>
                x.CreatedAt)
            .Select(f => new FloatHistory {
                AgentId = f.AgentId,
                Amount = f.Amount,
                ClosingBalance = f.ClosingBalance,
                CreatedAt = f.CreatedAt,
                Id = f.Id,
                Narrative = f.Narrative,
                OpeningBalance = f.OpeningBalance,
                OperationType = (FloatOperationType)f.OperationType,
                TransactionId = f.TransactionId
            }).ToListAsync();

        AgencyBankingServiceLogging.Completed(
            "GetFloatHistory",
            $"agentId={agentId} count={history.Count}");
        return history;
    }

    // ========================================================
    // HELPERS
    // ========================================================

    private FloatOperationResult Success(
        decimal balance,
        string message)
    {
        return new FloatOperationResult
        {
            Success = true,
            ResponseCode = "00",
            ResponseMessage = message,
            Balance = balance
        };
    }

    private FloatOperationResult Failed(
        string code,
        string message)
    {
        return new FloatOperationResult
        {
            Success = false,
            ResponseCode = code,
            ResponseMessage = message
        };
    }
}

// ============================================================
// MODELS
// ============================================================

public class FloatOperationResult
{
    public bool Success { get; set; }

    public string ResponseCode { get; set; } = "";

    public string ResponseMessage { get; set; } = "";

    public decimal Balance { get; set; }
}

public class FloatSummary
{
    public string AgentId { get; set; } = "";

    public decimal TotalFloat { get; set; }

    public decimal ReservedFloat { get; set; }

    public decimal AvailableFloat { get; set; }
}

// ============================================================
// FLOAT HISTORY ENTITY
// ============================================================

public class FloatHistory
{
    [Key]
    public long Id { get; set; }

    public string AgentId { get; set; } = "";

    public string TransactionId { get; set; } = "";

    public FloatOperationType OperationType { get; set; }

    public decimal Amount { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal ClosingBalance { get; set; }

    public string Narrative { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}

// ============================================================
// FLOAT RESERVATION ENTITY
// ============================================================

public class FloatReservation
{
    [Key]
    public long Id { get; set; }

    public string AgentId { get; set; } = "";

    public string TransactionId { get; set; } = "";

    public decimal Amount { get; set; }

    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}

// ============================================================
// ENUMS
// ============================================================

public enum FloatOperationType
{
    CREDIT = 1,
    DEBIT = 2,
    RESERVE = 3,
    RELEASE = 4
}

// ============================================================
// SQL TABLES
// ============================================================

/*

CREATE TABLE FloatHistories (
    Id BIGINT IDENTITY PRIMARY KEY,
    AgentId NVARCHAR(50),
    TransactionId NVARCHAR(100),
    OperationType INT,
    Amount DECIMAL(18,2),
    OpeningBalance DECIMAL(18,2),
    ClosingBalance DECIMAL(18,2),
    Narrative NVARCHAR(500),
    CreatedAt DATETIME2
);

CREATE TABLE FloatReservations (
    Id BIGINT IDENTITY PRIMARY KEY,
    AgentId NVARCHAR(50),
    TransactionId NVARCHAR(100),
    Amount DECIMAL(18,2),
    Status NVARCHAR(50),
    CreatedAt DATETIME2
);

ALTER TABLE Agents
ADD ReservedFloat DECIMAL(18,2) DEFAULT 0;

ALTER TABLE Agents
ADD DailyFloatLimit DECIMAL(18,2) DEFAULT 0;

ALTER TABLE Agents
ADD MinimumFloatThreshold DECIMAL(18,2) DEFAULT 0;

*/

// ============================================================
// SAMPLE USAGE
// ============================================================

/*

var result = await _floatService.DebitFloat(
    "AGT001",
    100,
    "TXN123");

if(result.Success)
{
    Console.WriteLine(result.Balance);
}

*/

// ============================================================
// FUTURE ENHANCEMENTS
// ============================================================

/*

- Float expiry
- Multi-currency float
- Float reconciliation
- Auto top-up
- Float limits engine
- Float settlement integration
- Branch-level float pools
- Agent hierarchy
- Real-time float streaming
- Float fraud detection
- Liquidity forecasting

*/
