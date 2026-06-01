using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class FloatConfiguration
{
    [Key]
    public long Id { get; set; }

    public string AgentId { get; set; } = "";

    public decimal MinimumFloat { get; set; }

    public decimal MaximumFloat { get; set; }

    public decimal DailyFloatLimit { get; set; }

    public DateTime UpdatedAt { get; set; }
}