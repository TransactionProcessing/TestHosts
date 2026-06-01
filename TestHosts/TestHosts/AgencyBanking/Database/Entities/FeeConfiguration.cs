using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class FeeConfiguration
{
    [Key]
    public long Id { get; set; }

    public string TransactionType { get; set; } = "";

    public decimal MinimumAmount { get; set; }

    public decimal MaximumAmount { get; set; }

    public decimal FeeAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}