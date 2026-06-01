namespace TestHosts.DataTransferObjects.AgencyBanking;

public class FeeConfigurationRequest
{
    public string TransactionType { get; set; } = "";

    public decimal MinimumAmount { get; set; }

    public decimal MaximumAmount { get; set; }

    public decimal FeeAmount { get; set; }
}