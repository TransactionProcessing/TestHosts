namespace TestHosts.Database.MobileWallet
{
    using Microsoft.EntityFrameworkCore;
    using Shared.General;
    using System;

    public class MobileWalletContext : DbContext
    {
        private readonly String ConnectionString;

        public MobileWalletContext()
        {
            this.ConnectionString = ConfigurationReader.GetConnectionString(Constants.MobileWalletReadModelConfig);
        }

        public MobileWalletContext(String connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public MobileWalletContext(DbContextOptions<MobileWalletContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<MobileWalletClient> Clients { get; set; }
        public DbSet<MobileWalletAccessToken> AccessTokens { get; set; }
        public DbSet<MobileWalletAccount> Accounts { get; set; }
        public DbSet<MobileWalletTransaction> Transactions { get; set; }
        public DbSet<MobileWalletReversal> Reversals { get; set; }
        public DbSet<MobileWalletWebhookSubscription> WebhookSubscriptions { get; set; }
        public DbSet<MobileWalletWebhookDelivery> WebhookDeliveries { get; set; }
        public DbSet<MobileWalletAuditEntry> AuditEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString) == false)
            {
                optionsBuilder.UseSqlServer(this.ConnectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MobileWalletClient>().HasKey(c => c.ClientId);
            modelBuilder.Entity<MobileWalletAccessToken>().HasKey(t => t.TokenId);
            modelBuilder.Entity<MobileWalletAccessToken>().HasIndex(t => t.Token).IsUnique();
            modelBuilder.Entity<MobileWalletAccount>().HasKey(a => a.AccountReference);
            modelBuilder.Entity<MobileWalletTransaction>().HasKey(t => t.TransactionReference);
            modelBuilder.Entity<MobileWalletTransaction>().HasIndex(t => new { t.ClientId, t.IdempotencyKey }).IsUnique();
            modelBuilder.Entity<MobileWalletTransaction>().HasIndex(t => t.ExternalReference);
            modelBuilder.Entity<MobileWalletReversal>().HasKey(r => r.ReversalReference);
            modelBuilder.Entity<MobileWalletReversal>().HasIndex(r => new { r.ClientId, r.IdempotencyKey }).IsUnique();
            modelBuilder.Entity<MobileWalletWebhookSubscription>().HasKey(s => s.SubscriptionId);
            modelBuilder.Entity<MobileWalletWebhookDelivery>().HasKey(d => d.DeliveryId);
            modelBuilder.Entity<MobileWalletAuditEntry>().HasKey(a => a.AuditEntryId);

            base.OnModelCreating(modelBuilder);
        }
    }

    public sealed class MobileWalletClient
    {
        public String ClientId { get; set; } = String.Empty;
        public String ClientSecret { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String? CallbackUrl { get; set; }
        public Boolean IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class MobileWalletAccessToken
    {
        public Guid TokenId { get; set; }
        public String Token { get; set; } = String.Empty;
        public String ClientId { get; set; } = String.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class MobileWalletAccount
    {
        public String AccountReference { get; set; } = String.Empty;
        public String AccountType { get; set; } = "wallet";
        public String Status { get; set; } = "active";
        public String Currency { get; set; } = "USD";
        public Decimal AvailableBalance { get; set; }
        public String GivenName { get; set; } = String.Empty;
        public String FamilyName { get; set; } = String.Empty;
        public String? Msisdn { get; set; }
        public String? EmailAddress { get; set; }
        public String KycStatus { get; set; } = "not_provided";
        public String? IdentityType { get; set; }
        public String? IdentityNumber { get; set; }
        public String? DateOfBirth { get; set; }
        public String? Nationality { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class MobileWalletTransaction
    {
        public String TransactionReference { get; set; } = String.Empty;
        public String ClientId { get; set; } = String.Empty;
        public String TransactionType { get; set; } = String.Empty;
        public String Status { get; set; } = "pending";
        public String StatusMessage { get; set; } = "Transaction accepted for asynchronous processing.";
        public Decimal Amount { get; set; }
        public String Currency { get; set; } = "USD";
        public String DebitAccountReference { get; set; } = String.Empty;
        public String CreditAccountReference { get; set; } = String.Empty;
        public String? ExternalReference { get; set; }
        public String IdempotencyKey { get; set; } = String.Empty;
        public String RequestPayload { get; set; } = String.Empty;
        public String? CallbackUrl { get; set; }
        public String? MetadataJson { get; set; }
        public Boolean BalanceApplied { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public sealed class MobileWalletReversal
    {
        public String ReversalReference { get; set; } = String.Empty;
        public String ClientId { get; set; } = String.Empty;
        public String OriginalTransactionReference { get; set; } = String.Empty;
        public String Reason { get; set; } = String.Empty;
        public String Status { get; set; } = "pending";
        public String StatusMessage { get; set; } = "Reversal accepted for asynchronous processing.";
        public String IdempotencyKey { get; set; } = String.Empty;
        public String RequestPayload { get; set; } = String.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public sealed class MobileWalletWebhookSubscription
    {
        public Guid SubscriptionId { get; set; }
        public String ClientId { get; set; } = String.Empty;
        public String EventType { get; set; } = "*";
        public String CallbackUrl { get; set; } = String.Empty;
        public Boolean IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class MobileWalletWebhookDelivery
    {
        public Guid DeliveryId { get; set; }
        public String ClientId { get; set; } = String.Empty;
        public String EventType { get; set; } = String.Empty;
        public String ResourceReference { get; set; } = String.Empty;
        public String CallbackUrl { get; set; } = String.Empty;
        public String Payload { get; set; } = String.Empty;
        public String Status { get; set; } = "pending";
        public Int32 AttemptCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public String? LastError { get; set; }
    }

    public sealed class MobileWalletAuditEntry
    {
        public Guid AuditEntryId { get; set; }
        public String ResourceType { get; set; } = String.Empty;
        public String ResourceReference { get; set; } = String.Empty;
        public String Action { get; set; } = String.Empty;
        public String Actor { get; set; } = String.Empty;
        public String HttpMethod { get; set; } = String.Empty;
        public String Path { get; set; } = String.Empty;
        public String? IdempotencyKey { get; set; }
        public String? RequestPayload { get; set; }
        public String? ResponsePayload { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
