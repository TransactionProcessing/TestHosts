using Microsoft.AspNetCore.Mvc;
using System;
using TestHosts.Database.TestBank;

namespace TestHosts.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Database.PataPawa;
    using Newtonsoft.Json;
    using Shared.General;

    [Route("api/developer")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly Func<String, PataPawaContext> ContextResolver;

        public DeveloperController(Func<String, PataPawaContext> contextResolver) {
            this.ContextResolver = contextResolver;
        }

        [HttpPost]
        [Route("patapawapostpay/createbill")]
        public async Task<IActionResult> CreateHostConfiguration([FromBody] CreatePataPawaPostPayBill request,
                                                                 CancellationToken cancellationToken)
        {
            String connectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
            PataPawaContext context = this.ContextResolver(connectionString);

            Guid billIdentifier = Guid.NewGuid();

            // TODO: check for a duplicate bill??

            await context.PostPaidBills.AddAsync(new PostPaidBill {
                                                                Amount = request.Amount,
                                                                AccountNumber = request.AccountNumber,
                                                                DueDate = request.DueDate,
                                                                AccountName = request.AccountName,
                                                                IsFullyPaid = false,
                                                                PostPaidBillId = billIdentifier
                                                            },
                                           cancellationToken);
            
            await context.SaveChangesAsync(cancellationToken);

            return this.Ok(new
                           {
                               billIdentifier
                           });
        }
    }

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
}
