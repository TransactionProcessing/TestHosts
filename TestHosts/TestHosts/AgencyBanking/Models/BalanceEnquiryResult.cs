// ============================================================
// ENTERPRISE TRANSACTION SERVICE
// WITH FULL FLOAT SERVICE INTEGRATION
// ============================================================

// ============================================================
// RESULT MODEL
// ============================================================

namespace TestHosts.AgencyBanking.Models {
    public class BalanceEnquiryResult {
        public string ResponseCode { get; set; }

        public string ResponseMessage { get; set; }

        public decimal AvailableBalance { get; set; }
    }
}