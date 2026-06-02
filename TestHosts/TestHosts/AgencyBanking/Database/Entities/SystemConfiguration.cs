using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class SystemConfiguration
{
    [Key]
    public long Id { get; set; }

    public string InstitutionCode { get; set; } = "";

    public string InstitutionName { get; set; } = "";

    public string DefaultCurrency { get; set; } = "";

    public string Timezone { get; set; } = "";

    public string SettlementMode { get; set; } = "";

    public DateTime InitializedAt { get; set; }
}