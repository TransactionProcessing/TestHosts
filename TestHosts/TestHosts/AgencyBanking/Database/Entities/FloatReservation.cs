using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class FloatReservation
{
    [Key]
    public long Id { get; set; }

    public string AgentId { get; set; } = "";

    public string TransactionId { get; set; } = "";

    public decimal Amount { get; set; }

    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}