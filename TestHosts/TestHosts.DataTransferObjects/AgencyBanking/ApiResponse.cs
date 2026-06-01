namespace TestHosts.DataTransferObjects.AgencyBanking;

    public class ApiResponse
    {
        public string ResponseCode { get; set; } = "";

        public string ResponseMessage { get; set; } = "";

        public string? TransactionId { get; set; }
    }

    // ============================================================
// SQL SERVER TABLES
// ============================================================

/*

CREATE TABLE Agents (
    AgentId NVARCHAR(50) PRIMARY KEY,
    TerminalId NVARCHAR(50),
    Pin NVARCHAR(500),
    FloatBalance DECIMAL(18,2),
    Active BIT
);

CREATE TABLE Accounts (
    AccountNumber NVARCHAR(50) PRIMARY KEY,
    AccountName NVARCHAR(200),
    Balance DECIMAL(18,2),
    Currency NVARCHAR(3),
    Active BIT
);

CREATE TABLE Transactions (
    TransactionId NVARCHAR(100) PRIMARY KEY,
    TransactionType NVARCHAR(50),
    AgentId NVARCHAR(50),
    CustomerAccount NVARCHAR(50),
    Amount DECIMAL(18,2),
    Status NVARCHAR(20),
    ResponseCode NVARCHAR(10),
    CreatedAt DATETIME2
);

CREATE TABLE LedgerEntries (
    Id BIGINT IDENTITY PRIMARY KEY,
    TransactionId NVARCHAR(100),
    DebitAccount NVARCHAR(50),
    CreditAccount NVARCHAR(50),
    Amount DECIMAL(18,2),
    CreatedAt DATETIME2
);

CREATE TABLE AuditLogs (
    Id BIGINT IDENTITY PRIMARY KEY,
    TransactionId NVARCHAR(100),
    Action NVARCHAR(100),
    Status NVARCHAR(50),
    CreatedAt DATETIME2
);

*/

// ============================================================
// APPSETTINGS.JSON
// ============================================================

/*

{
  "ConnectionStrings": {
    "DefaultConnection":
      "Server=localhost;Database=AgencyBankingHost;
       Trusted_Connection=True;
       TrustServerCertificate=True;"
  }
}*/