namespace TestHosts.DataTransferObjects.AgencyBanking;

public class GlAccountResponse
{
    public long Id { get; set; }
    public string GlCode { get; set; } = "";
    public string GlName { get; set; } = "";
    public string GlType { get; set; } = "";
    public string Currency { get; set; } = "";
    public bool Active { get; set; }
    public System.DateTime CreatedAt { get; set; }
}
