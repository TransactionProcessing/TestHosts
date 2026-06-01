namespace TestHosts.DataTransferObjects.AgencyBanking;

public class LimitConfigurationRequest
{
    public string TransactionType { get; set; } = "";

    public decimal PerTransactionLimit { get; set; }

    public decimal DailyLimit { get; set; }
}