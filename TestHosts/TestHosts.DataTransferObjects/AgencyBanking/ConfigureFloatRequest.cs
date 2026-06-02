namespace TestHosts.DataTransferObjects.AgencyBanking;

public class ConfigureFloatRequest
{
    public string AgentId { get; set; } = "";

    public decimal MinimumFloat { get; set; }

    public decimal MaximumFloat { get; set; }

    public decimal DailyFloatLimit { get; set; }
}

public class FloatCreditRequest
{
    public string TransactionId { get; set; }

    public string AgentId { get; set; }

    public decimal Amount { get; set; }

    public string SourceAccount { get; set; }

    public string Narration { get; set; }
}

public class FloatCreditResponse
{
    public string ResponseCode { get; set; }

    public string ResponseMessage { get; set; }

    public string AgentId { get; set; }

    public decimal NewFloatBalance { get; set; }

    public string TransactionId { get; set; }
}