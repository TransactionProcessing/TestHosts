using System.Collections.Generic;

namespace TestHosts.PataPawa.DataTransferObjects.PrePay;

public class Transaction
{
    [Newtonsoft.Json.JsonProperty("fixed")]
    public List<Fixed> Charges { get; set; }
    public string customerName { get; set; }
    public string date { get; set; }
    public string meterNo { get; set; }
    public string msg { get; set; }
    [Newtonsoft.Json.JsonProperty("ref")]
    public string reference { get; set; }
    public string rescode { get; set; }
    public int status { get; set; }
    public decimal stdTokenAmt { get; set; }
    public string stdTokenRctNum { get; set; }
    public string stdTokenTax { get; set; }
    public string token { get; set; }
    public string totalAmount { get; set; }
    public int transactionId { get; set; }
    public string units { get; set; }
    public string vendor { get; set; }
}