using System.Collections.Generic;

namespace TestHosts.DataTransferObjects.AgencyBanking;

public class ApiClientRequest
{
    public string ClientId { get; set; } = "";

    public string ClientName { get; set; } = "";

    public List<string> AllowedIps { get; set; } = new();

    public List<string> Scopes { get; set; } = new();
}