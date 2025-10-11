namespace TestHosts.Controllers
{
    using Database.TestBank;
    using DataTransferObjects.TestBank;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Shared.EntityFramework;
    using Shared.General;
    using Shared.Logger;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using TestHosts.Database.PataPawa;
    using Deposit = Database.TestBank.Deposit;

    [Route("api/testbank")]
    [ApiController]
    public class TestBankController : ControllerBase
    {
        private readonly IDbContextResolver<TestBankContext> ContextResolver;

        #region Fields


        #endregion

        #region Constructors

        public TestBankController(IDbContextResolver<TestBankContext> contextResolver) {
            this.ContextResolver = contextResolver;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("configuration")]
        public async Task<IActionResult> CreateHostConfiguration([FromBody] CreateHostConfigurationRequest createHostConfigurationRequest,
                                                                 CancellationToken cancellationToken)
        {
            using ResolvedDbContext<TestBankContext>? resolvedContext = this.ContextResolver.Resolve("TestBankReadModel");

            Guid hostIdentifier = Guid.NewGuid();

            var host = resolvedContext.Context.HostConfigurations.SingleOrDefault(h => h.AccountNumber == createHostConfigurationRequest.AccountNumber &&
                                                                       h.SortCode == createHostConfigurationRequest.SortCode);

            if (host != null)
            {
                return this.BadRequest("Invalid host identifier");
            }

            HostConfiguration hostConfiguration = new HostConfiguration
                                                  {
                                                      CallbackUri = createHostConfigurationRequest.CallbackUrl,
                                                      AccountNumber = createHostConfigurationRequest.AccountNumber,
                                                      HostIdentifier = hostIdentifier,
                                                      SortCode = createHostConfigurationRequest.SortCode
                                                  };
            await resolvedContext.Context.HostConfigurations.AddAsync(hostConfiguration, cancellationToken);
            await resolvedContext.Context.SaveChangesAsync(cancellationToken);

            return this.Ok(new
                           {
                               hostConfiguration.HostIdentifier
                           });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> MakeDeposit([FromBody] MakeDepositRequest makeDepositRequest,
                                                     CancellationToken cancellationToken)
        {
            Logger.LogInformation(JsonConvert.SerializeObject(makeDepositRequest));

            using ResolvedDbContext<TestBankContext>? resolvedContext = this.ContextResolver.Resolve("TestBankReadModel");
            HostConfiguration host = resolvedContext.Context.HostConfigurations.SingleOrDefault(h => h.AccountNumber == makeDepositRequest.ToAccountNumber &&
                                                                                     h.SortCode == makeDepositRequest.ToSortCode);
            if (host == null)
            {
                return this.NotFound($"No host found");
            }

            Guid depositId = Guid.NewGuid();
                Deposit deposit = new Deposit
                                  {
                                      Amount = makeDepositRequest.Amount,
                                      AccountNumber = makeDepositRequest.ToAccountNumber,
                                      SortCode = makeDepositRequest.ToSortCode,
                                      DateTime = makeDepositRequest.DateTime,
                                      Reference = makeDepositRequest.DepositReference,
                                      DepositId = depositId,
                                      HostIdentifier = host.HostIdentifier,
                                      SentToHost = false
                                  };
                await resolvedContext.Context.Deposits.AddAsync(deposit, cancellationToken);
                await resolvedContext.Context.SaveChangesAsync(cancellationToken);

                // Send to the call back Url (if specificed)
                if (host.CallbackUri != null)
                {
                    // Convert the entity to a DTO
                    DataTransferObjects.TestBank.Deposit depositDto = new DataTransferObjects.TestBank.Deposit()
                                                                      {
                                                                          Amount = makeDepositRequest.Amount,
                                                                          AccountNumber = makeDepositRequest.ToAccountNumber,
                                                                          SortCode = makeDepositRequest.ToSortCode,
                                                                          DateTime = makeDepositRequest.DateTime,
                                                                          Reference = makeDepositRequest.DepositReference,
                                                                          DepositId = depositId,
                                                                          HostIdentifier = host.HostIdentifier
                                                                      };

                    HttpClient client = new HttpClient();
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, host.CallbackUri);
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(depositDto), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        deposit.SentToHost = true;
                        await resolvedContext.Context.SaveChangesAsync(cancellationToken);
                    }
                }
            
            return this.Ok(new
                           {
                               host.HostIdentifier,
                               DepositId = depositId
                           });
        }

        #endregion
    }
}