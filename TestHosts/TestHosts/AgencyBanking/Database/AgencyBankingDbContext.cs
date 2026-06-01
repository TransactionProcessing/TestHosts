using System;
using Microsoft.EntityFrameworkCore;
using Shared.General;
using TestHosts.AgencyBanking.Database.Entities;

namespace TestHosts.AgencyBanking.Database;

public class AgencyBankingDbContext : DbContext
{
    private readonly String ConnectionString;
    public AgencyBankingDbContext(DbContextOptions<AgencyBankingDbContext> options) : base(options) {
    }

    public AgencyBankingDbContext()
    {
        // This is the migration connection string

        // Get connection string from configuration.
        this.ConnectionString = ConfigurationReader.GetConnectionString("AgencyBankingReadModel");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AgencyBankingDbContext" /> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public AgencyBankingDbContext(String connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
    public DbSet<GlAccount> GlAccounts { get; set; }
    public DbSet<SettlementAccount> SettlementAccounts { get; set; }
    public DbSet<SuperAgent> SuperAgents { get; set; }
    public DbSet<FloatConfiguration> FloatConfigurations { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<TransactionTypeConfiguration> TransactionTypeConfigurations { get; set; } 
    public DbSet<FeeConfiguration> FeeConfigurations { get; set; }
    public DbSet<CommissionConfiguration> CommissionConfigurations { get; set; }
    public DbSet<SettlementWindow> SettlementWindows { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<ApiClient> ApiClients { get; set; }
    public DbSet<LimitConfiguration> LimitConfigurations { get; set; }
    public DbSet<AmlRule> AmlRules { get; set; }
    public DbSet<GoLiveRecord> GoLiveRecords { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<FloatHistory> FloatHistories { get; set; }
    public DbSet<FloatReservation> FloatReservations { get; set; }
    public DbSet<SettlementRecord> SettlementRecords { get; set; }
    public DbSet<SettlementBatch> SettlementBatches { get; set; }
       
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>()
            .HasKey(x => x.AgentId);

        modelBuilder.Entity<Account>()
            .HasKey(x => x.AccountNumber);

        modelBuilder.Entity<TransactionEntity>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<LedgerEntry>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<AuditLog>()
            .HasKey(x => x.Id);
    }
}