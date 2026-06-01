namespace TestHosts.DataTransferObjects.AgencyBanking;

public class FloatConfigurationResponse
{
    public long Id { get; set; }
    public string AgentId { get; set; } = "";
    public decimal MinimumFloat { get; set; }
    public decimal MaximumFloat { get; set; }
    public decimal DailyFloatLimit { get; set; }
    public System.DateTime UpdatedAt { get; set; }
}
