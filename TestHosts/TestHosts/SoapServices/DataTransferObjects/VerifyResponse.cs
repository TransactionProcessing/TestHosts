namespace TestHosts.SoapServices.DataTransferObjects;

using System;
using System.Runtime.Serialization;

[DataContract(Name = "verify")]
public class VerifyResponse
{
    [DataMember(Name= "account_no")]
    public String AccountNumber { get; set; }

    [DataMember(Name= "account_name")]
    public String AccountName { get; set; }

    [DataMember(Name = "account_balance")]
    public Decimal AccountBalance { get; set; }

    [DataMember(Name = "due_date")]
    public DateTime DueDate { get; set; }
}