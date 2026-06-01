namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CustomerResponse
{
    public long Id { get; set; }
    public string CustomerId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public System.DateTime CreatedAt { get; set; }
}
