namespace TestHosts.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Database.TestBank;
    using DataTransferObjects.TestBank;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Shared.General;
    using Shared.Logger;
    using Deposit = Database.TestBank.Deposit;

    [Route("api/testbank")]
    [ApiController]
    public class TestBankController : ControllerBase
    {
        #region Fields

        private readonly Func<String, TestBankContext> ContextFactory;

        #endregion

        #region Constructors

        public TestBankController(Func<String, TestBankContext> contextFactory)
        {
            this.ContextFactory = contextFactory;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("configuration")]
        public async Task<IActionResult> CreateHostConfiguration([FromBody] CreateHostConfigurationRequest createHostConfigurationRequest,
                                                                 CancellationToken cancellationToken)
        {
            var connectionString = ConfigurationReader.GetConnectionString("TestBankReadModel");
            var context = this.ContextFactory(connectionString);

            Guid hostIdentifier = Guid.NewGuid();

            var host = context.HostConfigurations.SingleOrDefault(h => h.AccountNumber == createHostConfigurationRequest.AccountNumber &&
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
            await context.HostConfigurations.AddAsync(hostConfiguration, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

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

            String connectionString = ConfigurationReader.GetConnectionString("TestBankReadModel");
            TestBankContext context = this.ContextFactory(connectionString);
            HostConfiguration host = context.HostConfigurations.SingleOrDefault(h => h.AccountNumber == makeDepositRequest.ToAccountNumber &&
                                                                                     h.SortCode == makeDepositRequest.ToSortCode);
            Guid depositId = Guid.Empty;
            if (host == null)
            {
                return this.NotFound($"No host found");
            }

            depositId = Guid.NewGuid();
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
                await context.Deposits.AddAsync(deposit, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

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
                        await context.SaveChangesAsync(cancellationToken);
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