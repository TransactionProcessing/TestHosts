using System;
using System.Threading.Tasks;
using TestHosts.AgencyBanking.Database;
using TestHosts.AgencyBanking.Database.Entities;

namespace TestHosts.AgencyBanking.Services;

public interface ILedgerService {
    Task Post(string transactionId,
              string debitAccount,
              string creditAccount,
              decimal amount);
}

public class LedgerService : ILedgerService {
    private readonly AgencyBankingDbContext _db;

    public LedgerService(AgencyBankingDbContext db)
    {
        this._db = db;
    }

    public async Task Post(string transactionId,
                           string debitAccount,
                           string creditAccount,
                           decimal amount)
    {
        AgencyBankingServiceLogging.Started(
            "PostLedgerEntry",
            $"transactionId={transactionId} debitAccount={debitAccount} creditAccount={creditAccount} amount={amount}");
        this._db.LedgerEntries.Add(new LedgerEntry
        {
            TransactionId = transactionId,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        });

        await this._db.SaveChangesAsync();

        AgencyBankingServiceLogging.Completed(
            "PostLedgerEntry",
            $"transactionId={transactionId} debitAccount={debitAccount} creditAccount={creditAccount} amount={amount}");
    }
}
