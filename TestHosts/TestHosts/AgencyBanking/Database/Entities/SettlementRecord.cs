using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class SettlementRecord
{
    [Key]
    public long Id { get; set; }

    public string TransactionId { get; set; } = "";

    public string AgentId { get; set; } = "";

    public decimal Amount { get; set; }

    public string TransactionType { get; set; } = "";

    public string SettlementStatus { get; set; } = "";

    public string BatchId { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime? SettledAt { get; set; }
}