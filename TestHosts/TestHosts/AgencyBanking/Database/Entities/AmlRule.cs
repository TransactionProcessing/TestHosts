using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class AmlRule
{
    [Key]
    public long Id { get; set; }

    public string RuleCode { get; set; } = "";

    public string TransactionType { get; set; } = "";

    public decimal ThresholdAmount { get; set; }

    public string Action { get; set; } = "";

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
}