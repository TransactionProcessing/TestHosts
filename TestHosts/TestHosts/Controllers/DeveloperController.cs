using Microsoft.AspNetCore.Mvc;
using System;
using TestHosts.Database.TestBank;

namespace TestHosts.Controllers
{
    using Database.PataPawa;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Shared.EntityFramework;
    using Shared.General;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [Route("api/developer")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly IDbContextResolver<PataPawaContext> ContextResolver;
        private const String PataPawaReadModelKey = "PataPawaReadModel";
        public DeveloperController(IDbContextResolver<PataPawaContext> contextResolver) {
            this.ContextResolver = contextResolver;
        }

        [HttpPost]
        [Route("patapawaprepay/createuser")]
        public async Task<IActionResult> CreatePrepayUser([FromBody] CreatePatapawaPrePayUser request, CancellationToken cancellationToken){
            
            using ResolvedDbContext<PataPawaContext>? resolvedContext = this.ContextResolver.Resolve(PataPawaReadModelKey);

            Guid userId = Guid.NewGuid();

            PrePayUser user = await resolvedContext.Context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

            if (user == null){

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());
                string base64String = Convert.ToBase64String(bytes);

                // Create the user
                await resolvedContext.Context.PrePayUsers.AddAsync(new PrePayUser
                                                   {
                                                                      Balance = 0,
                                                                      Key = base64String,
                                                                      Password = request.Password,
                                                                      UserId = userId,
                                                                      UserName = request.UserName,
                                                                  }, cancellationToken);
                await resolvedContext.Context.SaveChangesAsync(cancellationToken);
            }

            return this.Ok();
        }

        [HttpPut]
        [Route("patapawaprepay/adduserdebt")]
        public async Task<IActionResult> AddUserDebt([FromBody] AddPatapawaPrePayUserDebt request, CancellationToken cancellationToken)
        {
            using ResolvedDbContext<PataPawaContext>? resolvedContext = this.ContextResolver.Resolve(PataPawaReadModelKey);
            
            PrePayUser user = await resolvedContext.Context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

            if (user == null){
                return this.NotFound();
            }

            user.Balance += request.DebtAmount;
           
            await resolvedContext.Context.SaveChangesAsync(cancellationToken);
            
            return this.Ok();
        }

        [HttpPost]
        [Route("patapawaprepay/createmeter")]
        public async Task<IActionResult> CreatePrepayMeter([FromBody] CreatePatapawaPrePayMeter request, CancellationToken cancellationToken){
            using ResolvedDbContext<PataPawaContext>? resolvedContext = this.ContextResolver.Resolve(PataPawaReadModelKey);
            
            Guid meterId = Guid.NewGuid();

            PrePayMeter meter = await resolvedContext.Context.PrePayMeters.SingleOrDefaultAsync(m => m.MeterNumber == request.MeterNumber, cancellationToken);

            if (meter == null)
            {
                // Create the meter
                await resolvedContext.Context.PrePayMeters.AddAsync(new PrePayMeter
                                                   {
                                                       MeterNumber = request.MeterNumber,
                                                       CustomerName = request.CustomerName,
                                                       MeterId = meterId
                                                   }, cancellationToken);
                await resolvedContext.Context.SaveChangesAsync(cancellationToken);
            }

            return this.Ok();
        }

        [HttpPost]
        [Route("patapawapostpay/createbill")]
        public async Task<IActionResult> CreateHostConfiguration([FromBody] CreatePataPawaPostPayBill request,
                                                                 CancellationToken cancellationToken)
        {
            using ResolvedDbContext<PataPawaContext>? resolvedContext = this.ContextResolver.Resolve(PataPawaReadModelKey);

            Guid billIdentifier = Guid.NewGuid();
            
            await resolvedContext.Context.PostPaidBills.AddAsync(new PostPaidBill {
                                                                Amount = request.Amount,
                                                                AccountNumber = request.AccountNumber,
                                                                DueDate = request.DueDate,
                                                                AccountName = request.AccountName,
                                                                IsFullyPaid = false,
                                                                PostPaidBillId = billIdentifier
                                                            },
                                           cancellationToken);
            
            await resolvedContext.Context.SaveChangesAsync(cancellationToken);

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
