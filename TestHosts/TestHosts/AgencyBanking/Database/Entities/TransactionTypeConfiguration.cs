using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class TransactionTypeConfiguration
{
    [Key]
    public long Id { get; set; }

    public string TransactionType { get; set; } = "";

    public bool Enabled { get; set; }

    public bool RequiresFloat { get; set; }

    public bool RequiresSettlement { get; set; }

    public bool RequiresApproval { get; set; }

    public DateTime CreatedAt { get; set; }
}