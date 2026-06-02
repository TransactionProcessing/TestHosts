using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class TransactionEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public string TransactionId { get; set; } = "";

    public string TransactionType { get; set; } = "";

    public string AgentId { get; set; } = "";

    public string CustomerAccount { get; set; } = "";

    public decimal Amount { get; set; }

    public string Status { get; set; } = "";

    public string ResponseCode { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public bool IsDuplicate { get; set; }

    public Guid DuplicateOfId { get; set; }
}