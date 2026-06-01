using System;
using Newtonsoft.Json;

namespace TestHosts.PataPawa.DataTransferObjects.PrePay;

public class CreatePataPawaPostPayBill
{
    [JsonProperty("due_date")]
    public DateTime DueDate { get; set; }
    [JsonProperty("amount")]
    public Decimal Amount { get; set; }

    [JsonProperty("account_number")]
    public String AccountNumber { get; set; }
    [JsonProperty("account_name")]
    public String AccountName { get; set; }

}