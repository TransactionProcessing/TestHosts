using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using TransactionProcessing.UnionResults;
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
            result.IsSuccess().ShouldBeTrue();
            
            // TODO: add assertions
            var systemConfigurationResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetSystemConfiguration(CancellationToken.None);
            var systemConfiguration = systemConfigurationResult.Value.ShouldBeOfType<Success<SystemConfigurationDto>>().Value;
            systemConfiguration.ShouldNotBeNull();
            systemConfiguration.InstitutionCode.ShouldBe(request.InstitutionCode);
            systemConfiguration.InstitutionName.ShouldBe(request.InstitutionName);
            systemConfiguration.DefaultCurrency.ShouldBe(request.DefaultCurrency);
            systemConfiguration.Timezone.ShouldBe(request.Timezone);
            systemConfiguration.SettlementMode.ShouldBe(request.SettlementMode);
        }

        [When(@"I approve go live with:")]
        public async Task WhenGoLive(Table table)
        {
            var data = StepHelpers.ToDictionary(table);

            var request = new GoLiveRequest { ApprovedBy = data["approvedBy"], Environment = data["environment"] };
            
            var result = await this.TestingContext.DockerHelper.AgencyBankingClient.GoLive(request, CancellationToken.None);

            result.IsSuccess().ShouldBeTrue();

            var goliveResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetGoLiveRecord(CancellationToken.None);
            var golive = goliveResult.Value.ShouldBeOfType<Success<GoLiveRecordResponse>>().Value;
            golive.ShouldNotBeNull();
            golive.ApprovedBy.ShouldBe(request.ApprovedBy);
            golive.Environment.ShouldBe(request.Environment);
            
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
            result.IsSuccess().ShouldBeTrue();

            var glaccountResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetGlAccountByCode(request.GlCode, CancellationToken.None);
            var glaccount = glaccountResult.Value.ShouldBeOfType<Success<GlAccountResponse>>().Value;
            glaccount.ShouldNotBeNull();
            glaccount.GlCode.ShouldBe(request.GlCode);
            glaccount.GlName.ShouldBe(request.GlName);
            glaccount.GlType.ShouldBe(request.GlType);
            glaccount.Currency.ShouldBe(request.Currency);
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
            result.IsSuccess().ShouldBeTrue();

            var settlementAccountResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetSettlementAccountByNumber(request.AccountNumber, CancellationToken.None);
            var settlementAccount = settlementAccountResult.Value.ShouldBeOfType<Success<SettlementAccountResponse>>().Value;
            settlementAccount.ShouldNotBeNull();
            settlementAccount.AccountNumber.ShouldBe(request.AccountNumber);
            settlementAccount.BankCode.ShouldBe(request.BankCode);
            settlementAccount.Currency.ShouldBe(request.Currency);
            settlementAccount.AccountName.ShouldBe(request.AccountName);
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
            result.IsSuccess().ShouldBeTrue();
            
            var agentResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetSuperAgentById(request.AgentId, CancellationToken.None);
            var agent = agentResult.Value.ShouldBeOfType<Success<SuperAgentResponse>>().Value;
            agent.ShouldNotBeNull();
            agent.AgentId.ShouldBe(request.AgentId);
            agent.Name.ShouldBe(request.Name);
            agent.PhoneNumber.ShouldBe(request.PhoneNumber);
            agent.Email.ShouldBe(request.Email);
            agent.Region.ShouldBe(request.Region);
            agent.DailyLimit.ShouldBe(request.DailyLimit);
            agent.MinimumFloat.ShouldBe(request.MinimumFloat);
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
            result.IsSuccess().ShouldBeTrue();
            
            var agentResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetAgentById(request.AgentId, CancellationToken.None);
            var agent = agentResult.Value.ShouldBeOfType<Success<AgentResponse>>().Value;
            agent.ShouldNotBeNull();
            agent.AgentId.ShouldBe(request.AgentId);
            agent.SuperAgentId.ShouldBe(request.SuperAgentId);
            agent.Name.ShouldBe(request.Name);
            agent.PhoneNumber.ShouldBe(request.PhoneNumber);
            agent.Email.ShouldBe(request.Email);
            agent.Location.ShouldBe(request.Location);
            agent.DailyLimit.ShouldBe(request.DailyLimit);
            agent.MinimumFloat.ShouldBe(request.MinimumFloat);
            
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
            result.IsSuccess().ShouldBeTrue();
            
            var agentResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetAgentById(agentId, CancellationToken.None);
            var agent = agentResult.Value.ShouldBeOfType<Success<AgentResponse>>().Value;
            agent.ShouldNotBeNull();
            agent.AgentId.ShouldBe(agentId);
            agent.Active.ShouldBeTrue();
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
            result.IsSuccess().ShouldBeTrue();
            
            var floatConfigurationResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetFloatConfigurationByAgentId(agentId, CancellationToken.None);
            var floatConfiguration = floatConfigurationResult.Value.ShouldBeOfType<Success<FloatConfigurationResponse>>().Value;
            floatConfiguration.ShouldNotBeNull();
            floatConfiguration.AgentId.ShouldBe(agentId);
            floatConfiguration.MinimumFloat.ShouldBe(request.MinimumFloat);
            floatConfiguration.MaximumFloat.ShouldBe(request.MaximumFloat);
            floatConfiguration.DailyFloatLimit.ShouldBe(request.DailyFloatLimit);
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
            result.IsSuccess().ShouldBeTrue();
            
            var floatHistoryResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetLastFloatEntries(agentId, 1, CancellationToken.None);
            var floatHistory = floatHistoryResult.Value.ShouldBeOfType<Success<FloatHistoryResponse[]>>().Value;
            floatHistory.ShouldNotBeNull();
            floatHistory.Count().ShouldBe(1);
            floatHistory[0].TransactionId.ShouldBe(request.TransactionId);
            floatHistory[0].AgentId.ShouldBe(agentId);
            floatHistory[0].Amount.ShouldBe(request.Amount);
            floatHistory[0].Narrative.ShouldBe(request.Narration);
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
            result.IsSuccess().ShouldBeTrue();
            
            var customerResult = await this.TestingContext.DockerHelper.AgencyBankingClient.GetCustomerByAccountNumber(request.AccountNumber, CancellationToken.None);
            var customer = customerResult.Value.ShouldBeOfType<Success<CustomerResponse>>().Value;
            customer.ShouldNotBeNull();
            customer.CustomerId.ShouldBe(request.CustomerId);
            customer.FullName.ShouldBe(request.FullName);
            customer.PhoneNumber.ShouldBe(request.PhoneNumber);
            customer.NationalId.ShouldBe(request.NationalId);
            customer.AccountNumber.ShouldBe(request.AccountNumber);
        }
    }
}

