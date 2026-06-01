namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CommissionConfigurationRequest
{
    public string TransactionType { get; set; } = "";

    public string CommissionType { get; set; } = "";

    public decimal CommissionValue { get; set; }
}