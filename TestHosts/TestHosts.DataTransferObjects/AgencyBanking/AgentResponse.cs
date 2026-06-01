namespace TestHosts.DataTransferObjects.AgencyBanking;

public class AgentResponse
{
    public long Id { get; set; }
    public string AgentId { get; set; } = "";
    public string TerminalId { get; set; } = "";
    public string SuperAgentId { get; set; } = "";
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string Location { get; set; } = "";
    public decimal ReservedFloat { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MinimumFloat { get; set; }
    public decimal FloatBalance { get; set; }
    public bool Active { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime? ActivatedAt { get; set; }
    public string ActivatedBy { get; set; } = "";
}
