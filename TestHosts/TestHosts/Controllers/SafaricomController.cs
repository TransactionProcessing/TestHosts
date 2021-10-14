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
    using DataTransferObjects.Safaricom;
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
}