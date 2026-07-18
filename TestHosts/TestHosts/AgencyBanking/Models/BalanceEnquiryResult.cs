// ============================================================
// ENTERPRISE TRANSACTION SERVICE
// WITH FULL FLOAT SERVICE INTEGRATION
// ============================================================

// ============================================================
// RESULT MODEL
// ============================================================

using System;

namespace TestHosts.AgencyBanking.Models {
    public class BalanceEnquiryResult {
        public String ResponseCode { get; set; }

        public string ResponseMessage { get; set; }

        public decimal AvailableBalance { get; set; }
    }
}