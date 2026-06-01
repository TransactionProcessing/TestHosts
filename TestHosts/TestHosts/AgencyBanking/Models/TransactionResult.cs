// ============================================================
// ENTERPRISE TRANSACTION SERVICE
// WITH FULL FLOAT SERVICE INTEGRATION
// ============================================================

// ============================================================
// RESULT MODEL
// ============================================================

namespace TestHosts.AgencyBanking.Models {
    public class TransactionResult {
        public bool Success { get; set; }

        public string ResponseCode { get; set; } = "";

        public string TransactionId { get; set; } = "";
    }
}