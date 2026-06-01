namespace TestHosts.DataTransferObjects.AgencyBanking;

public class LoginRequest
{
    public string AgentId { get; set; } = "";

    public string Pin { get; set; } = "";
}