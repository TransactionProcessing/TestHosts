namespace TestHosts.DataTransferObjects.AgencyBanking;

public class GoLiveRecordResponse
{
    public long Id { get; set; }
    public string ApprovedBy { get; set; } = "";
    public string Environment { get; set; } = "";
    public System.DateTime GoLiveDate { get; set; }
}
