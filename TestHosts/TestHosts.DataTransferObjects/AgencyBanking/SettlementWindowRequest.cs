namespace TestHosts.DataTransferObjects.AgencyBanking;

public class SettlementWindowRequest
{
    public string WindowName { get; set; } = "";

    public string StartTime { get; set; } = "";

    public string EndTime { get; set; } = "";

    public string SettlementMode { get; set; } = "";
}