namespace TestHosts.DataTransferObjects.AgencyBanking;

public class TransactionTypeConfigurationRequest
{
    public string TransactionType { get; set; } = "";

    public bool Enabled { get; set; }

    public bool RequiresFloat { get; set; }

    public bool RequiresSettlement { get; set; }

    public bool RequiresApproval { get; set; }
}