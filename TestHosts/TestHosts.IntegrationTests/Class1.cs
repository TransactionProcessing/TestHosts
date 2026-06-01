using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using SimpleResults;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.IntegrationTests
{
    using Reqnroll;
    using System.Collections.Generic;
    using System.Linq;

    public static class StepHelpers
    {
        public static Dictionary<string, string> ToDictionary(Table table)
        {
            return table.Rows.ToDictionary(r => r[0], r => r[1]);
        }
    }

    [Binding]
    [Scope(Tag = "system")]
    public class SystemSteps
    {
        private readonly TestingContext TestingContext;

        [Given("the Agency Banking API is available")]
        public void GivenTheAgencyBankingAPIIsAvailable()
        {
            // TODO: Health Check here
        }


        public SystemSteps(TestingContext testingContext) {
            this.TestingContext = testingContext;
        }

        [When(@"I initialize the system with:")]
        public async Task WhenInitializeSystem(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new SystemInitializationRequest
            {
                InstitutionCode = data["institutionCode"],
                InstitutionName = data["institutionName"],
                DefaultCurrency = data["defaultCurrency"],
                Timezone = data["timezone"],
                SettlementMode = data["settlementMode"]
            };
            Result result = await this.TestingContext.DockerHelper.AgencyBankingClient.InitializeSystem(request, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            
            // TODO: add assertions
            var systemConfiguration = await this.TestingContext.DockerHelper.AgencyBankingClient.GetSystemConfiguration(CancellationToken.None);
            systemConfiguration.IsSuccess.ShouldBeTrue();
            systemConfiguration.Data.ShouldNotBeNull();
            systemConfiguration.Data.InstitutionCode.ShouldBe(request.InstitutionCode);
            systemConfiguration.Data.InstitutionName.ShouldBe(request.InstitutionName);
            systemConfiguration.Data.DefaultCurrency.ShouldBe(request.DefaultCurrency);
            systemConfiguration.Data.Timezone.ShouldBe(request.Timezone);
            systemConfiguration.Data.SettlementMode.ShouldBe(request.SettlementMode);
        }

        [When(@"I approve go live with:")]
        public async Task WhenGoLive(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new GoLiveRequest { ApprovedBy = data["approvedBy"], Environment = data["environment"] };
            
            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.GoLive(request, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            var golive = await this.TestingContext.DockerHelper.AgencyBankingClient.GetGoLiveRecord(CancellationToken.None);
            golive.IsSuccess.ShouldBeTrue();
            golive.Data.ShouldNotBeNull();
            golive.Data.ApprovedBy.ShouldBe(request.ApprovedBy);
            golive.Data.Environment.ShouldBe(request.Environment);
            
        }
    }

    [Binding]
    [Scope(Tag = "glaccount")]
    public class GlAccountSteps
    {
        private readonly TestingContext TestingContext;

        public GlAccountSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [When(@"I create a GL account with:")]
        public async Task WhenCreateGlAccount(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new CreateGlAccountRequest() { GlCode = data["code"], GlName = data["name"], GlType = data["type"], Currency = data["currency"] };


            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.CreateGlAccount(request, CancellationToken.None);

            // TODO: add assertions
            result.IsSuccess.ShouldBeTrue();

            var glaccount = await this.TestingContext.DockerHelper.AgencyBankingClient.GetGlAccountByCode(request.GlCode, CancellationToken.None);
            glaccount.IsSuccess.ShouldBeTrue();
            glaccount.Data.ShouldNotBeNull();
            glaccount.Data.GlCode.ShouldBe(request.GlCode);
            glaccount.Data.GlName.ShouldBe(request.GlName);
            glaccount.Data.GlType.ShouldBe(request.GlType);
            glaccount.Data.Currency.ShouldBe(request.Currency);
        }
    }

    [Binding]
    [Scope(Tag = "settlement")]
    public class SettlementSteps
    {
        private readonly TestingContext TestingContext;

        public SettlementSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [When(@"I create a settlement account with:")]
        public async Task WhenCreateSettlement(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new CreateSettlementAccountRequest() { AccountNumber = data["accountNumber"], BankCode = data["bankCode"], Currency = data["currency"], AccountName = data["accountName"] };


            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.CreateSettlementAccount(request, CancellationToken.None);

            // TODO: add assertions;
            result.IsSuccess.ShouldBeTrue();

            var settlementAccount = await this.TestingContext.DockerHelper.AgencyBankingClient.GetSettlementAccountByNumber(request.AccountNumber, CancellationToken.None);
            settlementAccount.IsSuccess.ShouldBeTrue();
            settlementAccount.Data.ShouldNotBeNull();
            settlementAccount.Data.AccountNumber.ShouldBe(request.AccountNumber);
            settlementAccount.Data.BankCode.ShouldBe(request.BankCode);
            settlementAccount.Data.Currency.ShouldBe(request.Currency);
            settlementAccount.Data.AccountName.ShouldBe(request.AccountName);
        }
    }

    [Binding]
    [Scope(Tag = "agent")]
    public class AgentSteps
    {
        private readonly TestingContext TestingContext;

        public AgentSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [When(@"I create a super agent with:")]
        public async Task WhenCreateSuperAgent(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new CreateSuperAgentRequest() {
                AgentId = data["agentId"],
                Name = data["name"],
                PhoneNumber = data["phoneNumber"],
                Email = data["email"],
                Region = data["region"],
                DailyLimit = int.Parse(data["dailyLimit"]),
                MinimumFloat = int.Parse(data["minimumFloat"])
            };


            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.CreateSuperAgent(request, CancellationToken.None);

            // TODO: add assertions
            result.IsSuccess.ShouldBeTrue();
            
            var agent = await this.TestingContext.DockerHelper.AgencyBankingClient.GetSuperAgentById(request.AgentId, CancellationToken.None);
            agent.IsSuccess.ShouldBeTrue();
            agent.Data.ShouldNotBeNull();
            agent.Data.AgentId.ShouldBe(request.AgentId);
            agent.Data.Name.ShouldBe(request.Name);
            agent.Data.PhoneNumber.ShouldBe(request.PhoneNumber);
            agent.Data.Email.ShouldBe(request.Email);
            agent.Data.Region.ShouldBe(request.Region);
            agent.Data.DailyLimit.ShouldBe(request.DailyLimit);
            agent.Data.MinimumFloat.ShouldBe(request.MinimumFloat);
        }

        [When(@"I create a retail agent with:")]
        public async Task WhenCreateRetailAgent(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new CreateAgentRequest()
            {
                AgentId = data["agentId"],
                SuperAgentId = data["superAgentId"],
                Name = data["name"],
                PhoneNumber = data["phoneNumber"],
                Email = data["email"],
                Location = data["location"],
                DailyLimit = int.Parse(data["dailyLimit"]),
                MinimumFloat = int.Parse(data["minimumFloat"])
            };

            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.CreateRetailAgent(request, CancellationToken.None);

            // TODO: add assertions
            result.IsSuccess.ShouldBeTrue();
            
            var agent = await this.TestingContext.DockerHelper.AgencyBankingClient.GetAgentById(request.AgentId, CancellationToken.None);
            agent.IsSuccess.ShouldBeTrue();
            agent.Data.ShouldNotBeNull();
            agent.Data.AgentId.ShouldBe(request.AgentId);
            agent.Data.SuperAgentId.ShouldBe(request.SuperAgentId);
            agent.Data.Name.ShouldBe(request.Name);
            agent.Data.PhoneNumber.ShouldBe(request.PhoneNumber);
            agent.Data.Email.ShouldBe(request.Email);
            agent.Data.Location.ShouldBe(request.Location);
            agent.Data.DailyLimit.ShouldBe(request.DailyLimit);
            agent.Data.MinimumFloat.ShouldBe(request.MinimumFloat);
            
        }

        [When(@"I activate agent ""(.*)"" with:")]
        public async Task WhenActivateAgent(string agentId, Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.ActivateAgent(agentId, new ActivateAgentRequest()
            {
                ActivatedBy = data["activatedBy"]
            }, CancellationToken.None);

            // TODO: add assertions
            result.IsSuccess.ShouldBeTrue();
            
            Result<AgentResponse> agent = await this.TestingContext.DockerHelper.AgencyBankingClient.GetAgentById(agentId, CancellationToken.None);
            agent.IsSuccess.ShouldBeTrue();
            agent.Data.ShouldNotBeNull();
            agent.Data.AgentId.ShouldBe(agentId);
            agent.Data.Active.ShouldBeTrue();
        }
    }

    [Binding]
    [Scope(Tag = "float")]
    public class FloatSteps
    {
        private readonly TestingContext TestingContext;

        public FloatSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [When(@"I configure float for agent ""(.*)"" with:")]
        public async Task WhenConfigureFloat(string agentId, Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new ConfigureFloatRequest()
            {
                AgentId = agentId,
                MinimumFloat = int.Parse(data["minimumFloat"]),
                MaximumFloat = int.Parse(data["maximumFloat"]),
                DailyFloatLimit = int.Parse(data["dailyFloatLimit"])
            };

            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.ConfigureFloat(request, CancellationToken.None);
            
            // TODO: add assertions
            result.IsSuccess.ShouldBeTrue();
            
            var floatConfiguration = await this.TestingContext.DockerHelper.AgencyBankingClient.GetFloatConfigurationByAgentId(agentId, CancellationToken.None);
            floatConfiguration.IsSuccess.ShouldBeTrue();
            floatConfiguration.Data.ShouldNotBeNull();
            floatConfiguration.Data.AgentId.ShouldBe(agentId);
            floatConfiguration.Data.MinimumFloat.ShouldBe(request.MinimumFloat);
            floatConfiguration.Data.MaximumFloat.ShouldBe(request.MaximumFloat);
            floatConfiguration.Data.DailyFloatLimit.ShouldBe(request.DailyFloatLimit);
        }

        [When(@"I credit float to agent ""(.*)"" with:")]
        public async Task WhenCreditFloat(string agentId, Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new FloatCreditRequest()
            {
                TransactionId = data["transactionId"],
                AgentId = agentId,
                Amount = int.Parse(data["amount"]),
                SourceAccount = data["sourceAccount"],
                Narration = data["narration"]
            };

            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.CreditFloat(request, CancellationToken.None);

            // TODO: add assertions
            result.IsSuccess.ShouldBeTrue();
            
            var floatHistory = await this.TestingContext.DockerHelper.AgencyBankingClient.GetLastFloatEntries(agentId, 1, CancellationToken.None);
            floatHistory.IsSuccess.ShouldBeTrue();
            floatHistory.Data.ShouldNotBeNull();
            floatHistory.Data.Count().ShouldBe(1);
            floatHistory.Data[0].TransactionId.ShouldBe(request.TransactionId);
            floatHistory.Data[0].AgentId.ShouldBe(agentId);
            floatHistory.Data[0].Amount.ShouldBe(request.Amount);
            floatHistory.Data[0].Narrative.ShouldBe(request.Narration);
        }
    }

    [Binding]
    [Scope(Tag = "customer")]
    public class CustomerSteps {
        private readonly TestingContext TestingContext;

        public CustomerSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [When(@"I create a customer with:")]
        public async Task WhenCreateCustomer(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new CreateCustomerRequest() {
                CustomerId = data["customerId"],
                FullName = data["fullName"],
                PhoneNumber = data["phoneNumber"],
                NationalId = data["nationalId"],
                AccountNumber = data["accountNumber"]
            };
            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.CreateCustomer(request, CancellationToken.None);

            // TODO: add assertions
            result.IsSuccess.ShouldBeTrue();
            
            var customer = await this.TestingContext.DockerHelper.AgencyBankingClient.GetCustomerByAccountNumber(request.AccountNumber, CancellationToken.None);
            customer.IsSuccess.ShouldBeTrue();
            customer.Data.ShouldNotBeNull();
            customer.Data.CustomerId.ShouldBe(request.CustomerId);
            customer.Data.FullName.ShouldBe(request.FullName);
            customer.Data.PhoneNumber.ShouldBe(request.PhoneNumber);
            customer.Data.NationalId.ShouldBe(request.NationalId);
            customer.Data.AccountNumber.ShouldBe(request.AccountNumber);
        }
    }
}
