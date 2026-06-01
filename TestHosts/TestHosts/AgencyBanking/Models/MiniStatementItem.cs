// ============================================================
// ENTERPRISE TRANSACTION SERVICE
// WITH FULL FLOAT SERVICE INTEGRATION
// ============================================================

using System;
// ============================================================
// RESULT MODEL
// ============================================================

namespace TestHosts.AgencyBanking.Models {
    public class MiniStatementItem {
        public DateTime TransactionDate { get; set; }

        public string TransactionType { get; set; }

        public decimal Amount { get; set; }
    }
}