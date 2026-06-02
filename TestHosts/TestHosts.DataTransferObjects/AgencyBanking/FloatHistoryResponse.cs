using System;

namespace TestHosts.DataTransferObjects.AgencyBanking
{
    public class FloatHistoryResponse
    {
        public long Id { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public int OperationType { get; set; }
        public decimal Amount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Narrative { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
