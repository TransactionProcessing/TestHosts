using System;
using System.Collections.Generic;

namespace TestHosts.DataTransferObjects.AgencyBanking;

public class DepositRequest
{
    public string TransactionId { get; set; }
    public string CustomerId { get; set; }

    public string AgentId { get; set; }

    public string AccountNumber { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public String Channel { get; set; }
    public String Narration { get; set; }
    public String ReferenceNumber { get; set; }
}

public class WithdrawalRequest
{
    public string TransactionId { get; set; }

    public string CustomerId { get; set; }

    public string AgentId { get; set; }

    public string AccountNumber { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public String Channel { get; set; }
    public String Narration { get; set; }
    public String ReferenceNumber { get; set; }
}

public class BalanceEnquiryRequest
{
    public string AgentId { get; set; }

    public string AccountNumber { get; set; }
}

public class BalanceEnquiryResponse
{
    public string ResponseCode { get; set; }

    public string ResponseMessage { get; set; }

    public decimal AvailableBalance { get; set; }
}

public class MiniStatementRequest
{
    public string AgentId { get; set; }

    public string AccountNumber { get; set; }

    public int Count { get; set; } = 5;
}

public class MiniStatementResponse
{
    public string ResponseCode { get; set; }

    public string ResponseMessage { get; set; }

    public List<MiniStatementItem> Transactions { get; set; }
}

public class MiniStatementItem
{
    public DateTime TransactionDate { get; set; }

    public string TransactionType { get; set; }

    public decimal Amount { get; set; }
}

public class TransactionResponse
{
    public string ResponseCode { get; set; }

    public string ResponseMessage { get; set; }

    public string TransactionId { get; set; }

    public decimal Amount { get; set; }
}