using System;

namespace TestHosts.AgencyBanking.Database.Entities;

public class SettlementSummary
{
    public DateTime SettlementDate { get; set; }

    public int TotalTransactions { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal TotalCashIn { get; set; }

    public decimal TotalCashOut { get; set; }

    public decimal TotalReversals { get; set; }
}