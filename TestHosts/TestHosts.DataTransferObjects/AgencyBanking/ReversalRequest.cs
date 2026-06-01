namespace TestHosts.DataTransferObjects.AgencyBanking;

public class ReversalRequest
{
    // Transaction being reversed
    public string OriginalTransactionId { get; set; } = "";

    // New reversal transaction
    public string TransactionId { get; set; } = "";

    public string ReasonCode { get; set; } = "";

    public string ReversedBy { get; set; } = "";

    public String Channel { get; set; }
    public string AgentId { get; set; }
}