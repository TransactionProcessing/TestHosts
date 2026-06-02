using System;
using Newtonsoft.Json;

namespace TestHosts.PataPawa.DataTransferObjects.PrePay;

public class CreatePatapawaPrePayMeter
{
    [JsonProperty("meter_number")]
    public String MeterNumber{ get; set; }
    [JsonProperty("customer_name")]
    public String CustomerName { get; set; }
}