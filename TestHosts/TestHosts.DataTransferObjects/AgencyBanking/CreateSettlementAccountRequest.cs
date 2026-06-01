namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CreateSettlementAccountRequest
{
    public string AccountNumber { get; set; } = "";

    public string BankCode { get; set; } = "";

    public string Currency { get; set; } = "";

    public string AccountName { get; set; } = "";
}