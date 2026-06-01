using System.ComponentModel.DataAnnotations;

namespace TestHosts.DataTransferObjects.AgencyBanking;

public class SystemConfigurationDto
{
    public long Id { get; set; }
    public string InstitutionCode { get; set; } = "";
    public string InstitutionName { get; set; } = "";
    public string DefaultCurrency { get; set; } = "";
    public string Timezone { get; set; } = "";
    public string SettlementMode { get; set; } = "";
    public System.DateTime InitializedAt { get; set; }
}


public class TransactionDto
{
    public string TransactionId { get; set; } = "";

    public string TransactionType { get; set; } = "";

    public string AgentId { get; set; } = "";

    public string CustomerAccount { get; set; } = "";

    public decimal Amount { get; set; }

    public string Status { get; set; } = "";

    public string ResponseCode { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}