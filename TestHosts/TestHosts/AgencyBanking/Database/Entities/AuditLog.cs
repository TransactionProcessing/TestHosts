using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class AuditLog
{
    [Key]
    public long Id { get; set; }

    public string TransactionId { get; set; } = "";

    public string Action { get; set; } = "";

    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}