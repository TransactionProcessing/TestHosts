namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CreateSuperAgentRequest
{
    public string AgentId { get; set; } = "";

    public string Name { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public string Email { get; set; } = "";

    public string Region { get; set; } = "";

    public decimal DailyLimit { get; set; }

    public decimal MinimumFloat { get; set; }
}