using System.ComponentModel.DataAnnotations;

namespace TestHosts.DataTransferObjects.AgencyBanking;

public class SystemInitializationRequest
{
    public string InstitutionCode { get; set; } = "";

    public string InstitutionName { get; set; } = "";

    public string DefaultCurrency { get; set; } = "";

    public string Timezone { get; set; } = "";

    public string SettlementMode { get; set; } = "";
}

public class SystemConfigurationResponse
{
    public string InstitutionCode { get; set; } 

    public string InstitutionName { get; set; } 

    public string DefaultCurrency { get; set; } 

    public string Timezone { get; set; } 

    public string SettlementMode { get; set; } 

    public DateTime InitializedAt { get; set; }
}