namespace TestHosts.SoapServices.DataTransferObjects;

using System;
using System.Runtime.Serialization;

[DataContract(Name = "login")]
public class LoginResponse
{
    [DataMember(Name = "status")]
    public Int32 Status { get; set; }
    [DataMember(Name = "message")]
    public String Message { get; set; }
    [DataMember(Name = "balance")]
    public Decimal Balance { get; set; }
    [DataMember(Name = "api_key")]
    public String APIKey { get; set; }
}