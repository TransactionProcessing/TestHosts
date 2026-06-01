namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CreateGlAccountRequest
{
    public string GlCode { get; set; } = "";

    public string GlName { get; set; } = "";

    public string GlType { get; set; } = "";

    public string Currency { get; set; } = "";
}