using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class SettlementWindow
{
    [Key]
    public long Id { get; set; }

    public string WindowName { get; set; } = "";

    public string StartTime { get; set; } = "";

    public string EndTime { get; set; } = "";

    public string SettlementMode { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}