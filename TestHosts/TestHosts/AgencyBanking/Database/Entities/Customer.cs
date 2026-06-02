using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class Customer
{
    [Key]
    public long Id { get; set; }

    public string CustomerId { get; set; } = "";

    public string FullName { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public string NationalId { get; set; } = "";

    public string AccountNumber { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}