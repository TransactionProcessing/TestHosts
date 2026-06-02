namespace TestHosts.DataTransferObjects.AgencyBanking;

public class SettlementAccountResponse
{
    public long Id { get; set; }
    public string AccountNumber { get; set; } = "";
    public string BankCode { get; set; } = "";
    public string Currency { get; set; } = "";
    public string AccountName { get; set; } = "";
    public bool Active { get; set; }
    public System.DateTime CreatedAt { get; set; }
}
