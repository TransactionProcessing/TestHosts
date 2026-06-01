// ============================================================
// ENTERPRISE TRANSACTION SERVICE
// WITH FULL FLOAT SERVICE INTEGRATION
// ============================================================

using System.Collections.Generic;
// ============================================================
// RESULT MODEL
// ============================================================

namespace TestHosts.AgencyBanking.Models {
    public class MiniStatementResult {
        public string ResponseCode { get; set; }

        public string ResponseMessage { get; set; }

        public List<MiniStatementItem> Transactions { get; set; }
    }
}