using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class Account
{
    [Key]
    public string AccountNumber { get; set; } = "";

    public string AccountName { get; set; } = "";

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "USD";

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CustomerId { get; set; } = "";
}