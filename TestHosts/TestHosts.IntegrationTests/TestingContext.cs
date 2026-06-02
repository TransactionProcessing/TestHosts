using System;
using System.Collections.Generic;
using Shared.Logger;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.IntegrationTests
{
    /// <summary>
    /// 
    /// </summary>
    public class TestingContext
    {
        public DockerHelper DockerHelper { get; set; }
        
        public NlogLogger Logger;

        public TestingContext()
        {
        
        }
        
        private Dictionary<String, BalanceEnquiryResponse> BalanceEnquiryResponses { get; set; } = new Dictionary<String, BalanceEnquiryResponse>();
        
        public BalanceEnquiryResponse GetBalanceEnquiryResponse(String customerId) => this.BalanceEnquiryResponses[customerId];

        public void AddBalanceEnquiryResponse(String customerId,
                                              BalanceEnquiryResponse response) {
            this.BalanceEnquiryResponses.TryAdd(customerId, response);
        }

        private Dictionary<String, DepositRequest> DepositRequests { get; set; } = new Dictionary<String, DepositRequest>();
        public DepositRequest GetDepositRequest(String transactionId) => this.DepositRequests[transactionId];

        public void AddDepositRequest(String transactionId,
                                      DepositRequest request) {
            this.DepositRequests.Add(transactionId, request);
        }

        private Dictionary<String, WithdrawalRequest> WithdrawalRequests { get; set; } = new Dictionary<String, WithdrawalRequest>();
        public WithdrawalRequest GetWithdrawalRequest(String transactionId) => this.WithdrawalRequests[transactionId];

        public void AddWithdrawalRequest(String transactionId,
                                         WithdrawalRequest request) {
            this.WithdrawalRequests.Add(transactionId, request);
        }
    }
}