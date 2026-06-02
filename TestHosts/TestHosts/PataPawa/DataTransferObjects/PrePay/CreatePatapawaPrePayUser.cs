using System;
using Newtonsoft.Json;

namespace TestHosts.PataPawa.DataTransferObjects.PrePay;

public class CreatePatapawaPrePayUser
{
    [JsonProperty("user_name")]
    public String UserName{ get; set; }
    [JsonProperty("password")]
    public String Password { get; set; }
}