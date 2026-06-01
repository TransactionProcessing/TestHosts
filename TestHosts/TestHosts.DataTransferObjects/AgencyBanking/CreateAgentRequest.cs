namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CreateAgentRequest
{
    public string AgentId { get; set; } = "";

    public string SuperAgentId { get; set; } = "";

    public string Name { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public string Email { get; set; } = "";

    public string Location { get; set; } = "";

    public decimal DailyLimit { get; set; }

    public decimal MinimumFloat { get; set; }
}