using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class LimitConfiguration
{
    [Key]
    public long Id { get; set; }

    public string TransactionType { get; set; } = "";

    public decimal PerTransactionLimit { get; set; }

    public decimal DailyLimit { get; set; }

    public DateTime CreatedAt { get; set; }
}