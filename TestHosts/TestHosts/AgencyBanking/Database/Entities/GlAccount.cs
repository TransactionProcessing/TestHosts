using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class GlAccount
{
    [Key]
    public long Id { get; set; }

    public string GlCode { get; set; } = "";

    public string GlName { get; set; } = "";

    public string GlType { get; set; } = "";

    public string Currency { get; set; } = "";

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
}