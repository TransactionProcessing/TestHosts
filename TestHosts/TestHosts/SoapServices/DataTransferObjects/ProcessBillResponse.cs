namespace TestHosts.SoapServices.DataTransferObjects;

using System;
using System.Runtime.Serialization;
using System.Xml.Linq;

[DataContract(Name = "paybill")]
public class ProcessBillResponse
{
    [DataMember(Name = "receipt_no")]
    public String ReceiptNumber { get; set; }

    [DataMember(Name = "status")]
    public Int32 Status { get; set; }

    [DataMember(Name = "sms_id")]
    public String SmsId { get; set; }

    [DataMember(Name = "rescode")]
    public String ResultCode { get; set; }

    [DataMember(Name = "agent_id")]
    public String AgentId { get; set; }

    [DataMember(Name = "msg")]
    public String Message { get; set; }

}