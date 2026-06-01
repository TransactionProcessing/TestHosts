namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CashOutRequest
{
    public string TransactionId { get; set; } = "";

    public string AgentId { get; set; } = "";

    public string CustomerAccount { get; set; } = "";

    public decimal Amount { get; set; }
}