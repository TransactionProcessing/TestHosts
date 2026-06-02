using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class LedgerEntry
{
    [Key]
    public long Id { get; set; }

    public string TransactionId { get; set; } = "";

    public string DebitAccount { get; set; } = "";

    public string CreditAccount { get; set; } = "";

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }
}