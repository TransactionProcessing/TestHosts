using System;
using Newtonsoft.Json;

namespace TestHosts.PataPawa.DataTransferObjects.PrePay;

public class AddPatapawaPrePayUserDebt
{
    [JsonProperty("user_name")]
    public String UserName { get; set; }

    [JsonProperty("debt_amount")]
    public Decimal DebtAmount { get; set; }
}