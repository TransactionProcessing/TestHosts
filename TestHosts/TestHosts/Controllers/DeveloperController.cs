using Microsoft.AspNetCore.Mvc;
using System;
using TestHosts.Database.TestBank;

namespace TestHosts.Controllers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Database.PataPawa;
    using Microsoft.EntityFrameworkCore;
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
        [Route("patapawaprepay/createuser")]
        public async Task<IActionResult> CreatePrepayUser([FromBody] CreatePatapawaPrePayUser request, CancellationToken cancellationToken){
            String connectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
            PataPawaContext context = this.ContextResolver(connectionString);


            Guid userId = Guid.NewGuid();

            PrePayUser user = await context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

            if (user == null){

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());
                string base64String = Convert.ToBase64String(bytes);

                // Create the user
                await context.PrePayUsers.AddAsync(new PrePayUser
                                                   {
                                                                      Balance = 0,
                                                                      Key = base64String,
                                                                      Password = request.Password,
                                                                      UserId = userId,
                                                                      UserName = request.UserName,
                                                                  }, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            return this.Ok();
        }

        [HttpPut]
        [Route("patapawaprepay/adduserdebt")]
        public async Task<IActionResult> AddUserDebt([FromBody] AddPatapawaPrePayUserDebt request, CancellationToken cancellationToken)
        {
            String connectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
            PataPawaContext context = this.ContextResolver(connectionString);


            Guid userId = Guid.NewGuid();

            PrePayUser user = await context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

            if (user == null){
                return this.NotFound();
            }

            user.Balance += request.DebtAmount;
           
            await context.SaveChangesAsync(cancellationToken);
            
            return this.Ok();
        }

        [HttpPost]
        [Route("patapawaprepay/createmeter")]
        public async Task<IActionResult> CreatePrepayMeter([FromBody] CreatePatapawaPrePayMeter request, CancellationToken cancellationToken){
            String connectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
            PataPawaContext context = this.ContextResolver(connectionString);


            Guid meterId = Guid.NewGuid();

            PrePayMeter meter = await context.PrePayMeters.SingleOrDefaultAsync(m => m.MeterNumber == request.MeterNumber, cancellationToken);

            if (meter == null)
            {
                // Create the meter
                await context.PrePayMeters.AddAsync(new PrePayMeter
                                                   {
                                                       MeterNumber = request.MeterNumber,
                                                       CustomerName = request.CustomerName,
                                                       MeterId = meterId
                                                   }, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            return this.Ok();
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

    public class CreatePatapawaPrePayUser{
        [JsonProperty("user_name")]
        public String UserName{ get; set; }
        [JsonProperty("password")]
        public String Password { get; set; }
    }

    public class CreatePatapawaPrePayMeter{
        [JsonProperty("meter_number")]
        public String MeterNumber{ get; set; }
        [JsonProperty("customer_name")]
        public String CustomerName { get; set; }
    }

    public class AddPatapawaPrePayUserDebt
    {
        [JsonProperty("user_name")]
        public String UserName { get; set; }

        [JsonProperty("debt_amount")]
        public Decimal DebtAmount { get; set; }
    }

}
