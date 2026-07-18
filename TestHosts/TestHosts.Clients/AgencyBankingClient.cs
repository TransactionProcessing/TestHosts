using Microsoft.Extensions.Logging;
using Shared.Results;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Text;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.Clients
{
    public interface IAgencyBankingClient
    {
        Task<Result> InitializeSystem(SystemInitializationRequest request, CancellationToken cancellationToken);
        Task<Result> GoLive(GoLiveRequest request, CancellationToken cancellationToken);
        Task<Result> CreateGlAccount(CreateGlAccountRequest request, CancellationToken cancellationToken);
        Task<Result> CreateSettlementAccount(CreateSettlementAccountRequest request, CancellationToken cancellationToken);
        Task<Result> CreateSuperAgent(CreateSuperAgentRequest request, CancellationToken cancellationToken);
        Task<Result> CreateRetailAgent(CreateAgentRequest request, CancellationToken cancellationToken);
        Task<Result> ActivateAgent(string agentId, ActivateAgentRequest request, CancellationToken cancellationToken);
        Task<Result> ConfigureFloat(ConfigureFloatRequest request, CancellationToken cancellationToken);
        Task<Result> CreditFloat(FloatCreditRequest request, CancellationToken cancellationToken);
        Task<Result> CreateCustomer(CreateCustomerRequest request, CancellationToken cancellationToken);

        // Query methods returning typed results
        Task<SimpleResults.Result<SystemConfigurationDto>> GetSystemConfiguration(CancellationToken cancellationToken);
        Task<SimpleResults.Result<GlAccountResponse>> GetGlAccountByCode(string glCode, CancellationToken cancellationToken);
        Task<SimpleResults.Result<SettlementAccountResponse>> GetSettlementAccountByNumber(string accountNumber, CancellationToken cancellationToken);
        Task<SimpleResults.Result<SuperAgentResponse>> GetSuperAgentById(string agentId, CancellationToken cancellationToken);
        Task<SimpleResults.Result<AgentResponse>> GetAgentById(string agentId, CancellationToken cancellationToken);
        Task<SimpleResults.Result<FloatConfigurationResponse>> GetFloatConfigurationByAgentId(string agentId, CancellationToken cancellationToken);
        Task<SimpleResults.Result<CustomerResponse>> GetCustomerByAccountNumber(string accountNumber, CancellationToken cancellationToken);
        Task<SimpleResults.Result<Decimal>> GetCustomerBalance(string accountNumber, CancellationToken cancellationToken);
        Task<SimpleResults.Result<GoLiveRecordResponse>> GetGoLiveRecord(CancellationToken cancellationToken);
        Task<Result<FloatHistoryResponse[]>> GetLastFloatEntries(string agentId, int count, CancellationToken cancellationToken);
        // Transaction endpoints
        Task<Result> Deposit(DepositRequest request, CancellationToken cancellationToken);
        Task<Result> Withdrawal(WithdrawalRequest request, CancellationToken cancellationToken);
        Task<Result<BalanceEnquiryResponse>> BalanceEnquiry(BalanceEnquiryRequest request, CancellationToken cancellationToken);
        Task<Result> MiniStatement(MiniStatementRequest request, CancellationToken cancellationToken);
        Task<Result> Reversal(ReversalRequest request, CancellationToken cancellationToken);
        Task<SimpleResults.Result<TransactionDto>> GetTransactionById(string transactionId, CancellationToken cancellationToken);
    }

    public class AgencyBankingClient : ClientProxyBase.ClientProxyBase, IAgencyBankingClient
    {

        /// <summary>
        /// The base address
        /// </summary>
        private String BaseAddress;

        /// <summary>
        /// The base address resolver
        /// </summary>
        private readonly Func<String, String> BaseAddressResolver;


        public AgencyBankingClient(Func<String, String> baseAddressResolver,
                                   HttpClient httpClient,
                                   Func<object, string> serialise,
                                   Func<string, Type, object> deserialise) : base(httpClient, serialise, deserialise) {
            this.BaseAddressResolver = baseAddressResolver;
            this.BaseAddress = baseAddressResolver("AgencyBankingHost");
        }

        // SYSTEM
        public async Task<Result> InitializeSystem(SystemInitializationRequest request,
                                                   CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/system/initialize");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while initializing the system.", ex);

                throw exception;
            }
        }

        public async Task<Result> GoLive(GoLiveRequest request,
                                         CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/system/go-live");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while going live.", ex);

                throw exception;
            }
        }

        // GL ACCOUNTS
        public async Task<Result> CreateGlAccount(CreateGlAccountRequest request,
                                                  CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/glaccounts");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while creating GL account.", ex);

                throw exception;
            }
        }

        // SETTLEMENT
        public async Task<Result> CreateSettlementAccount(CreateSettlementAccountRequest request,
                                                          CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/settlement/accounts");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while creating settlement account.", ex);

                throw exception;
            }
        }

        // AGENTS
        public async Task<Result> CreateSuperAgent(CreateSuperAgentRequest request,
                                                   CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/agents/super-agent");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while creating super agent.", ex);

                throw exception;
            }
        }

        public async Task<Result> CreateRetailAgent(CreateAgentRequest request,
                                                    CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/agents");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while creating retail agent.", ex);

                throw exception;
            }
        }

        public async Task<Result> ActivateAgent(string agentId,
                                                ActivateAgentRequest request,
                                                CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/agents/{agentId}/activate");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while activating retail agent.", ex);

                throw exception;
            }
        }

        // FLOAT
        public async Task<Result> ConfigureFloat(ConfigureFloatRequest request,
                                                 CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/float/configure");

            try {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while configuring float.", ex);

                throw exception;
            }
        }

        public async Task<Result> CreditFloat(FloatCreditRequest request, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/float/credit");

            try
            {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while crediting float.", ex);

                throw exception;
            }
        }
        
        // CUSTOMERS
        public async Task<Result> CreateCustomer(CreateCustomerRequest request, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/customers");

            try
            {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("An error occurred while creating customer.", ex);

                throw exception;
            }
        }

        // QUERY
        public async Task<Result<SystemConfigurationDto>> GetSystemConfiguration(CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/system/configuration");

            try {
                var httpResult = await this.Get<SystemConfigurationDto>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving system configurations.", ex);
                throw exception;
            }
        }

        public async Task<Result<GlAccountResponse>> GetGlAccountByCode(string glCode, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/glaccounts/{glCode}");

            try {
                var httpResult = await this.Get<GlAccountResponse>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving GL account.", ex);
                throw exception;
            }
        }

        public async Task<Result<SettlementAccountResponse>> GetSettlementAccountByNumber(string accountNumber, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/settlement/accounts/{accountNumber}");

            try {
                var httpResult = await this.Get<SettlementAccountResponse>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving settlement account.", ex);
                throw exception;
            }
        }

        public async Task<Result<SuperAgentResponse>> GetSuperAgentById(string agentId, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/agents/super-agent/{agentId}");

            try {
                var httpResult = await this.Get<SuperAgentResponse>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving super agent.", ex);
                throw exception;
            }
        }

        public async Task<Result<AgentResponse>> GetAgentById(string agentId, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/agents/{agentId}");

            try {
                var httpResult = await this.Get<AgentResponse>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving agent.", ex);
                throw exception;
            }
        }

        public async Task<Result<FloatConfigurationResponse>> GetFloatConfigurationByAgentId(string agentId, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/float/configuration/{agentId}");

            try {
                var httpResult = await this.Get<FloatConfigurationResponse>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving float configuration.", ex);
                throw exception;
            }
        }

        public async Task<Result<CustomerResponse>> GetCustomerByAccountNumber(string accountNumber, CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/customers/{accountNumber}");

            try {
                var httpResult = await this.Get<CustomerResponse>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving customer.", ex);
                throw exception;
            }
        }

        public async Task<Result<Decimal>> GetCustomerBalance(String accountNumber,
                                                              CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/customers/{accountNumber}/balance");

            try {
                var httpResult = await this.Get<Decimal>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving customer balance.", ex);
                throw exception;
            }
        }

        public async Task<Result<GoLiveRecordResponse>> GetGoLiveRecord(CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/system/go-live/records");

            try {
                var httpResult = await this.Get<GoLiveRecordResponse>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex) {
                Exception exception = new Exception("An error occurred while retrieving go-live records.", ex);
                throw exception;
            }
        }

        public async Task<Result<FloatHistoryResponse[]>> GetLastFloatEntries(string agentId, int count, CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/float-history/{agentId}/{count}");

            try
            {
                var httpResult = await this.Get<FloatHistoryResponse[]>(requestUri, cancellationToken);

                if (httpResult.IsFailed)
                    return ResultHelpers.CreateFailure(httpResult);

                return Result.Success(httpResult.Data);
            }
            catch (Exception ex)
            {
                Exception exception = new Exception("An error occurred while retrieving float history entries.", ex);
                throw exception;
            }
        }

        // TRANSACTIONS
        public async Task<Result> Deposit(DepositRequest request, CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/transactions/deposit");

            try
            {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Exception exception = new Exception("An error occurred while performing deposit.", ex);
                throw exception;
            }
        }

        public async Task<Result> Withdrawal(WithdrawalRequest request, CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/transactions/withdrawal");

            try
            {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Exception exception = new Exception("An error occurred while performing withdrawal.", ex);
                throw exception;
            }
        }

        public async Task<Result<BalanceEnquiryResponse>> BalanceEnquiry(BalanceEnquiryRequest request, CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/transactions/balance-enquiry");

            try
            {
                Result<BalanceEnquiryResponse> result = await this.Post<BalanceEnquiryRequest,BalanceEnquiryResponse>(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success(result.Data);
            }   
            catch (Exception ex)
            {
                Exception exception = new Exception("An error occurred while performing balance enquiry.", ex);
                throw exception;
            }
        }

        public async Task<Result> MiniStatement(MiniStatementRequest request, CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/transactions/mini-statement");

            try
            {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Exception exception = new Exception("An error occurred while requesting mini statement.", ex);
                throw exception;
            }
        }

        public async Task<Result> Reversal(ReversalRequest request, CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/agencybanking/transactions/reversal");

            try
            {
                Result result = await this.Post(requestUri, request, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Exception exception = new Exception("An error occurred while performing reversal.", ex);
                throw exception;
            }
        }

        public async Task<Result<TransactionDto>> GetTransactionById(String transactionId,
                                                                     CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/agencybanking/transactions/{transactionId}");

            try
            {
                var result = await this.Get<TransactionDto>(requestUri, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success(result.Data);
            }
            catch (Exception ex)
            {
                Exception exception = new Exception("An error occurred while retrieving the transaction by ID.", ex);
                throw exception;
            }
        }

        private String BuildRequestUrl(String route)
        {
            if (string.IsNullOrEmpty(this.BaseAddress))
            {
                this.BaseAddress = this.BaseAddressResolver("AgencyBankingHost");
            }

            String requestUri = $"{this.BaseAddress}{route}";
            return requestUri;
        }
    }
}
