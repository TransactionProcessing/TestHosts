using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class SettlementAccount
{
    [Key]
    public long Id { get; set; }

    public string AccountNumber { get; set; } = "";

    public string BankCode { get; set; } = "";

    public string Currency { get; set; } = "";

    public string AccountName { get; set; } = "";

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
}