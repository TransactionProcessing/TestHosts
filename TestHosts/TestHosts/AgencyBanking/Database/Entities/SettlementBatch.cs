using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class SettlementBatch
{
    [Key]
    public long Id { get; set; }

    public string BatchId { get; set; } = "";

    public DateTime SettlementDate { get; set; }

    public int TotalTransactions { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}