namespace TestHosts.DataTransferObjects.AgencyBanking;

public class AmlRuleRequest
{
    public string RuleCode { get; set; } = "";

    public string TransactionType { get; set; } = "";

    public decimal ThresholdAmount { get; set; }

    public string Action { get; set; } = "";
}