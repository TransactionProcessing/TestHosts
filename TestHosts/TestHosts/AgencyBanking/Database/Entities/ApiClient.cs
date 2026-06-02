using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class ApiClient
{
    [Key]
    public long Id { get; set; }

    public string ClientId { get; set; } = "";

    public string ClientName { get; set; } = "";

    public string AllowedIps { get; set; } = "";

    public string Scopes { get; set; } = "";

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
}