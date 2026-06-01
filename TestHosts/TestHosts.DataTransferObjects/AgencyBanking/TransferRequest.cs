namespace TestHosts.DataTransferObjects.AgencyBanking;

public class TransferRequest
{
    public string TransactionId { get; set; } = "";

    public string SourceAccount { get; set; } = "";

    public string DestinationAccount { get; set; } = "";

    public decimal Amount { get; set; }
}