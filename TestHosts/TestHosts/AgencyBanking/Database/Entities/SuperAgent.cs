using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class SuperAgent
{
    [Key]
    public long Id { get; set; }

    public string AgentId { get; set; } = "";

    public string Name { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public string Email { get; set; } = "";

    public string Region { get; set; } = "";

    public decimal DailyLimit { get; set; }

    public decimal MinimumFloat { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
}