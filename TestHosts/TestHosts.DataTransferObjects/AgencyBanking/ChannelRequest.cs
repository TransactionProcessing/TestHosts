namespace TestHosts.DataTransferObjects.AgencyBanking;

public class ChannelRequest
{
    public string ChannelCode { get; set; } = "";

    public string ChannelName { get; set; } = "";

    public bool Enabled { get; set; }
}