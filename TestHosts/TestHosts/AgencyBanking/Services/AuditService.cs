using System;
using System.Threading.Tasks;
using TestHosts.AgencyBanking.Database;
using TestHosts.AgencyBanking.Database.Entities;

namespace TestHosts.AgencyBanking.Services {
    public interface IAuditService {
        Task Log(string transactionId,
                 string action,
                 string status);
    }

    public class AuditService : IAuditService {
        private readonly AgencyBankingDbContext _db;

        public AuditService(AgencyBankingDbContext db)
        {
            this._db = db;
        }

        public async Task Log(string transactionId,
                              string action,
                              string status)
        {
            AgencyBankingServiceLogging.Started(
                "AuditLog",
                $"transactionId={transactionId} action={action} status={status}");
            this._db.AuditLogs.Add(new AuditLog
            {
                TransactionId = transactionId,
                Action = action,
                Status = status,
                CreatedAt = DateTime.UtcNow
            });

            await this._db.SaveChangesAsync();

            AgencyBankingServiceLogging.Completed(
                "AuditLog",
                $"transactionId={transactionId} action={action} status={status}");
        }
    }
}
