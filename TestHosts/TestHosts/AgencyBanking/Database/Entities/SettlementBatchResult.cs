namespace TestHosts.AgencyBanking.Database.Entities;

public class SettlementBatchResult
{
    public bool Success { get; set; }

    public string BatchId { get; set; } = "";

    public int TotalTransactions { get; set; }

    public decimal TotalAmount { get; set; }

    public string ResponseMessage { get; set; } = "";
}