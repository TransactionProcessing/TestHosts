using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestHosts.Controllers
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using Shared.Logger;

    [Route("api/safaricom")]
    [ApiController]
    public class SafaricomController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> PerformTopup([FromQuery] String vendor, [FromQuery] String reqType, [FromQuery] String data)
        {
            // Deserialise the request message
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);
            XmlSerializer xs = new XmlSerializer(typeof(SafaricomRequest));
            SafaricomRequest cl = (SafaricomRequest)xs.Deserialize(new StringReader(doc.OuterXml));
            
            // Build the response object

            OkObjectResult result = Ok(new SafaricomResponse
                                       {
                                           MESSAGE = this.GenerateTxnStatusMessage(cl.Amount),
                                           DATE = cl.Date,
                                           EXTREFNUM = cl.ExternalReferenceNumber,
                                           TXNID = this.GenerateTransactionId(),
                                           TXNSTATUS = this.GenerateTxnStatus(cl.Amount),
                                           TYPE = "EXRCTRFRESP"
            });

            // currently result.Formatters is empty but we'd like to ensure it will be so in the future
            result.Formatters.Clear();

            // force response as xml
            result.Formatters.Add(new Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerOutputFormatter());

            return result;
        }

        private String GenerateTransactionId()
        {
            return DateTime.Now.ToString($"yyyyMMddHHmmssfff");
        }

        private Int32 GenerateTxnStatus(Int32 transactionAmount)
        {
            if (transactionAmount == 25000)
            {
                return 400;
            }

            if (transactionAmount > 25000)
            {
                return 401;
            }

            if (transactionAmount < 1000)
            {
                return 402;
            }

            return 200;
        }

        private String GenerateTxnStatusMessage(Int32 transactionAmount)
        {
            if (transactionAmount == 25000)
            {
                return "Bad Request";
            }

            if (transactionAmount > 25000)
            {
                return "Amount Greater than 25000";
            }

            if (transactionAmount < 1000)
            {
                return "Amount Less than 1000";
            }

            return "Topup Successful";
        }
    }


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://safaricom.co.ke/Pinless/keyaccounts/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://safaricom.co.ke/Pinless/keyaccounts/", IsNullable = false, ElementName = "COMMAND")]
    public class SafaricomRequest
    { 
        [XmlElement(ElementName = "TYPE")]
        public String Type { get; set; }

        [XmlElement(ElementName = "DATE")]
        public String Date { get; set; }

        [XmlElement(ElementName = "EXTNWCODE")]
        public String NetworkCode { get; set; }

        [XmlElement(ElementName = "MSISDN")]
        public String BankMSISDN { get; set; }

        [XmlElement(ElementName = "PIN")]
        public String Pin { get; set; }

        [XmlElement(ElementName = "LOGINID")]
        public String LOGINID { get; set; }

        [XmlElement(ElementName = "PASSWORD")]
        public String PASSWORD { get; set; }

        [XmlElement(ElementName = "EXTCODE")]
        public String ExternalBankCode { get; set; }

        [XmlElement(ElementName = "EXTREFNUM")]
        public String ExternalReferenceNumber { get; set; }

        [XmlElement(ElementName = "MSISDN2")]
        public String CustomerMSISDN { get; set; }

        [XmlElement(ElementName = "AMOUNT")]
        public Int32 Amount { get; set; }

        [XmlElement(ElementName = "LANGUAGE1")]
        public String BankLanguage { get; set; }

        [XmlElement(ElementName = "LANGUAGE2")]
        public String CustomerLanguage { get; set; }

        [XmlElement(ElementName = "SELECTOR")]
        public String Selector { get; set; }
    }


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "COMMAND")]
    public partial class SafaricomResponse
    {
        [XmlElement(ElementName = "TYPE")]
        public String TYPE { get; set; }

        [XmlElement(ElementName = "TXNSTATUS")]
        public Int32 TXNSTATUS { get; set; }

        [XmlElement(ElementName = "DATE")]
        public String DATE { get; set; }

        [XmlElement(ElementName = "EXTREFNUM")]
        public String EXTREFNUM { get; set; }

        [XmlElement(ElementName = "TXNID")]
        public string TXNID { get; set; }

        [XmlElement(ElementName = "MESSAGE")]
        public String MESSAGE { get; set; }
    }


}