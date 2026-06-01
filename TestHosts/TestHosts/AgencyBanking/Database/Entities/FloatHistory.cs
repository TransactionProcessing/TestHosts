using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class FloatHistory
{
    [Key]
    public long Id { get; set; }

    public string AgentId { get; set; } = "";

    public string TransactionId { get; set; } = "";

    public Int32 OperationType { get; set; }

    public decimal Amount { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal ClosingBalance { get; set; }

    public string Narrative { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}