namespace TestHosts.DataTransferObjects.AgencyBanking;

public class CreateCustomerRequest
{
    public string CustomerId { get; set; } = "";

    public string FullName { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public string NationalId { get; set; } = "";

    public string AccountNumber { get; set; } = "";
}