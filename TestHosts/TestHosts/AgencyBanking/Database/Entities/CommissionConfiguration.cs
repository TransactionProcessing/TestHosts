using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class CommissionConfiguration
{
    [Key]
    public long Id { get; set; }

    public string TransactionType { get; set; } = "";

    public string CommissionType { get; set; } = "";

    public decimal CommissionValue { get; set; }

    public DateTime CreatedAt { get; set; }
}