using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Shouldly;
using SimpleResults;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestHosts.DataTransferObjects.AgencyBanking;
using TestHosts.IntegrationTests;
using Table = Reqnroll.Table;

[Binding]
[Scope(Tag = "transactions")]
public class TransactionSteps
{
    private readonly TestingContext TestingContext;

    public TransactionSteps(TestingContext testingContext) {
        this.TestingContext = testingContext;
    }
    
    // =========================================================
    // DEPOSIT
    // =========================================================
    
    [When(@"agent ""(.*)"" performs a cash deposit with:")]
    public async Task WhenAgentPerformsDeposit(string agentId,
                                               Table table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new DepositRequest()
        {
            TransactionId = data["transactionId"],
            AgentId = agentId,
            CustomerId = data["customerId"],
            AccountNumber = data["accountNumber"],
            Amount = decimal.Parse(data["amount"]),
            Currency = data["currency"],
            Channel = data["channel"],
            Narration = data["narration"],
            ReferenceNumber = data["referenceNumber"]
        };

        Result response = await this.TestingContext.DockerHelper.AgencyBankingClient.Deposit(request, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
    }

    [When("agent {string} performs a cash withdrawal with:")]
    public async Task WhenAgentPerformsACashWithdrawalWith(string agentId, Table table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new WithdrawalRequest()
        {
            TransactionId = data["transactionId"],
            AgentId = agentId,
            CustomerId = data["customerId"],
            AccountNumber = data["accountNumber"],
            Amount = decimal.Parse(data["amount"]),
            Currency = data["currency"],
            Channel = data["channel"],
            Narration = data["narration"],
            ReferenceNumber = data["referenceNumber"]
        };

        Result response = await this.TestingContext.DockerHelper.AgencyBankingClient.Withdrawal(request, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
    }

    [When("agent {string} performs a cash withdrawal with which fails:")]
    public async Task WhenAgentPerformsACashWithdrawalWithWhichFails(string agentId, Table table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new WithdrawalRequest()
        {
            TransactionId = data["transactionId"],
            AgentId = agentId,
            CustomerId = data["customerId"],
            AccountNumber = data["accountNumber"],
            Amount = decimal.Parse(data["amount"]),
            Currency = data["currency"],
            Channel = data["channel"],
            Narration = data["narration"],
            ReferenceNumber = data["referenceNumber"]
        };

        Result response = await this.TestingContext.DockerHelper.AgencyBankingClient.Withdrawal(request, CancellationToken.None);
        response.IsFailed.ShouldBeTrue();
    }

    [When("agent {string} performs a cash deposit with which fails:")]
    public async Task WhenAgentPerformsACashDepositWithWhichFails(string agentId, DataTable table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new DepositRequest()
        {
            TransactionId = data["transactionId"],
            AgentId = agentId,
            CustomerId = data["customerId"],
            AccountNumber = data["accountNumber"],
            Amount = decimal.Parse(data["amount"]),
            Currency = data["currency"],
            Channel = data["channel"],
            Narration = data["narration"],
            ReferenceNumber = data["referenceNumber"]
        };

        Result response = await this.TestingContext.DockerHelper.AgencyBankingClient.Deposit(request, CancellationToken.None);
        response.IsFailed.ShouldBeTrue();
    }
    
    [When("agent {string} performs a balance enquiry with:")]
    public async Task WhenAgentPerformsABalanceEnquiryWith(string agentId, DataTable table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new BalanceEnquiryRequest()
        {
            AgentId = agentId,
            AccountNumber = data["accountNumber"],
        };

        Result<BalanceEnquiryResponse> response = await this.TestingContext.DockerHelper.AgencyBankingClient.BalanceEnquiry(request, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
        
        this.TestingContext.AddBalanceEnquiryResponse(request.AccountNumber, response.Data);
    }

    [Then("the available account balance for {string} should be returned as {int}")]
    public void ThenTheAvailableAccountBalanceForShouldBeReturnedAs(string customer, int balance)
    {
        var response = this.TestingContext.GetBalanceEnquiryResponse(customer);
        response.AvailableBalance.ShouldBe(balance);
    }
    
    [Then("the transaction id {string} should be successful and the transaction status should be {string}")]
    public async Task ThenTheTransactionIdShouldBeSuccessfulTheTransactionStatusShouldBe(string transactionId, string extectedStatus)
    {
        var response = await this.TestingContext.DockerHelper.AgencyBankingClient.GetTransactionById(transactionId, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
        response.Data.Status.ShouldBe(extectedStatus);
    }

    [Then("the transaction id {string} should fail And the transaction status should be {string}")]
    public async Task ThenTheTransactionIdShouldFailAndTheTransactionStatusShouldBe(string transactionId, string expectedStatus)
    {
        var response = await this.TestingContext.DockerHelper.AgencyBankingClient.GetTransactionById(transactionId, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
        response.Data.Status.ShouldBe(expectedStatus);
    }

    [Then("the transaction id {string} should fail And the transaction status should be {string} and the response code should be {string}")]
    public async Task ThenTheTransactionIdShouldFailAndTheTransactionStatusShouldBeAndTheResponseCodeShouldBe(string transactionId, string expectedStatus, string responsecode)
    {
        var response = await this.TestingContext.DockerHelper.AgencyBankingClient.GetTransactionById(transactionId, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
        response.Data.Status.ShouldBe(expectedStatus);
        response.Data.ResponseCode.ShouldBe(responsecode);
    }



    [Then("the response code for transaction id {string} should be {string}")]
    public async Task ThenTheResponseCodeForTransactionIdShouldBe(string transactionId, string responseCode)
    {
        var response = await this.TestingContext.DockerHelper.AgencyBankingClient.GetTransactionById(transactionId, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
        response.Data.ResponseCode.ShouldBe(responseCode);
    }




    [Then("the customer account {string} balance should be {int}")]
    public async Task ThenTheCustomerAccountBalanceShouldBe(string accountNumber, int amount)
    {
        var response = await this.TestingContext.DockerHelper.AgencyBankingClient.GetCustomerBalance(accountNumber, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
        response.Data.ShouldBe(amount);
    }

    [Then("the agent {string} float balance should be {int}")]
    public async Task ThenTheAgentFloatBalanceShouldBe(string agentId, int amount)
    {
        Result<AgentResponse> response = await this.TestingContext.DockerHelper.AgencyBankingClient.GetAgentById(agentId, CancellationToken.None);
        response.IsSuccess.ShouldBeTrue();
        response.Data.FloatBalance.ShouldBe(amount);
    }

    [When("agent {string} performs a transaction reversal with:")]
    public async Task WhenAgentPerformsATransactionReversalWith(string agentId, DataTable table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new ReversalRequest()
        {
            AgentId = agentId,
            TransactionId = data["reversalTransactionId"],
            OriginalTransactionId = data["originalTransactionId"],
            ReasonCode = data["reversalReason"],
            ReversedBy = data["initiatedBy"],
            Channel = data["channel"]
        };
        
        Result result = await this.TestingContext.DockerHelper.AgencyBankingClient.Reversal(request, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }



    /*
    // =========================================================
    // WITHDRAWAL
    // =========================================================

    [When(@"agent ""(.*)"" performs a cash withdrawal with:")]
    public async Task WhenAgentPerformsWithdrawal(
        string agentId,
        Table table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new
        {
            transactionId = data["transactionId"],
            agentId = agentId,
            customerId = data["customerId"],
            accountNumber = data["accountNumber"],
            amount = decimal.Parse(data["amount"]),
            currency = data["currency"],
            channel = data["channel"],
            narration = data["narration"],
            referenceNumber = data["referenceNumber"]
        };

        try
        {
            var response = await _api.PerformWithdrawal(request);

            _scenarioContext["ApiResponse"] = response;
            _scenarioContext["TransactionStatus"] = "SUCCESS";
        }
        catch (Exception ex)
        {
            _scenarioContext["LastError"] = ex.Message;
            _scenarioContext["TransactionStatus"] = "FAILED";
        }
    }

    // =========================================================
    // BALANCE ENQUIRY
    // =========================================================

    [When(@"agent ""(.*)"" performs a balance enquiry with:")]
    public async Task WhenAgentPerformsBalanceEnquiry(
        string agentId,
        Table table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new
        {
            transactionId = data["transactionId"],
            agentId = agentId,
            customerId = data["customerId"],
            accountNumber = data["accountNumber"],
            channel = data["channel"],
            referenceNumber = data["referenceNumber"]
        };

        try
        {
            var response = await _api.PerformBalanceEnquiry(request);

            _scenarioContext["BalanceResponse"] = response;
            _scenarioContext["TransactionStatus"] = "SUCCESS";
        }
        catch (Exception ex)
        {
            _scenarioContext["LastError"] = ex.Message;
            _scenarioContext["TransactionStatus"] = "FAILED";
        }
    }

    // =========================================================
    // REVERSAL
    // =========================================================

    [When(@"agent ""(.*)"" performs a transaction reversal with:")]
    public async Task WhenAgentPerformsReversal(
        string agentId,
        Table table)
    {
        var data = StepHelpers.ToDictionary(table);

        var request = new
        {
            reversalTransactionId = data["reversalTransactionId"],
            originalTransactionId = data["originalTransactionId"],
            reversalReason = data["reversalReason"],
            initiatedBy = data["initiatedBy"],
            channel = data["channel"]
        };

        try
        {
            var response = await _api.PerformReversal(request);

            _scenarioContext["ReversalResponse"] = response;
            _scenarioContext["TransactionStatus"] = "SUCCESS";
        }
        catch (Exception ex)
        {
            _scenarioContext["LastError"] = ex.Message;
            _scenarioContext["TransactionStatus"] = "FAILED";
        }
    }

    // =========================================================
    // SUCCESS ASSERTIONS
    // =========================================================

    [Then(@"the deposit transaction should be successful")]
    public void ThenDepositShouldBeSuccessful()
    {
        _scenarioContext["TransactionStatus"]
            .Should()
            .Be("SUCCESS");
    }

    [Then(@"the withdrawal transaction should be successful")]
    public void ThenWithdrawalShouldBeSuccessful()
    {
        _scenarioContext["TransactionStatus"]
            .Should()
            .Be("SUCCESS");
    }

    [Then(@"the balance enquiry should be successful")]
    public void ThenBalanceEnquiryShouldBeSuccessful()
    {
        _scenarioContext["TransactionStatus"]
            .Should()
            .Be("SUCCESS");
    }

    [Then(@"the reversal transaction should be successful")]
    public void ThenReversalShouldBeSuccessful()
    {
        _scenarioContext["TransactionStatus"]
            .Should()
            .Be("SUCCESS");
    }

    // =========================================================
    // FAILURE ASSERTIONS
    // =========================================================

    [Then(@"the deposit transaction should fail")]
    public void ThenDepositShouldFail()
    {
        _scenarioContext["TransactionStatus"]
            .Should()
            .Be("FAILED");
    }

    [Then(@"the withdrawal transaction should fail")]
    public void ThenWithdrawalShouldFail()
    {
        _scenarioContext["TransactionStatus"]
            .Should()
            .Be("FAILED");
    }

    [Then(@"the transaction status should be ""(.*)""")]
    public void ThenTransactionStatusShouldBe(string expectedStatus)
    {
        _scenarioContext["TransactionStatus"]
            .Should()
            .Be(expectedStatus);
    }

    [Then(@"the error message should contain ""(.*)""")]
    public void ThenErrorMessageShouldContain(string expectedMessage)
    {
        var error = _scenarioContext["LastError"]
            .ToString();

        error.Should().Contain(expectedMessage);
    }

    // =========================================================
    // BALANCE ASSERTIONS
    // =========================================================

    [Then(@"the customer account balance should increase by (.*)")]
    public void ThenCustomerBalanceShouldIncrease(decimal amount)
    {
        amount.Should().BeGreaterThan(0);
    }

    [Then(@"the customer account balance should decrease by (.*)")]
    public void ThenCustomerBalanceShouldDecrease(decimal amount)
    {
        amount.Should().BeGreaterThan(0);
    }

    [Then(@"the agent float balance should decrease by (.*)")]
    public void ThenAgentFloatShouldDecrease(decimal amount)
    {
        amount.Should().BeGreaterThan(0);
    }

    [Then(@"the agent float balance should increase by (.*)")]
    public void ThenAgentFloatShouldIncrease(decimal amount)
    {
        amount.Should().BeGreaterThan(0);
    }

    [Then(@"the customer account balance should be restored")]
    public void ThenCustomerBalanceRestored()
    {
        true.Should().BeTrue();
    }

    [Then(@"the agent float balance should be adjusted accordingly")]
    public void ThenFloatAdjusted()
    {
        true.ShouldBe().BeTrue();
    }

    // =========================================================
    // BALANCE ENQUIRY ASSERTIONS
    // =========================================================

    [Then(@"the available account balance should be returned")]
    public void ThenAvailableBalanceReturned()
    {
        _scenarioContext.ContainsKey("BalanceResponse")
            .Should()
            .BeTrue();
    }

    [Then(@"the ledger account balance should be returned")]
    public void ThenLedgerBalanceReturned()
    {
        _scenarioContext.ContainsKey("BalanceResponse")
            .Should()
            .BeTrue();
    }*/
}