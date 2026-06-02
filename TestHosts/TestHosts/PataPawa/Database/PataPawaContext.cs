using Microsoft.EntityFrameworkCore;
using Shared.General;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestHosts.PataPawa.Database
{
    public class PataPawaContext : DbContext
    {
        private readonly String ConnectionString;
        public PataPawaContext()
        {
            // This is the migration connection string

            // Get connection string from configuration.
            this.ConnectionString = ConfigurationReader.GetConnectionString(Constants.PataPawaReadModelConfig);
        }

        public PataPawaContext(String connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public PataPawaContext(DbContextOptions<PataPawaContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<PostPaidAccount> PostPaidAccounts { get; set; }

        public DbSet<PostPaidBill> PostPaidBills { get; set; }
        public DbSet<PrePayUser> PrePayUsers{ get; set; }

        public DbSet<PrePayMeter> PrePayMeters { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionCharge> TransactionCharges { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                optionsBuilder.UseSqlServer(this.ConnectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<PostPaidAccount>().HasKey(p => p.AccountId);

            modelBuilder.Entity<PostPaidBill>().HasKey(p => p.PostPaidBillId);

            modelBuilder.Entity<PrePayUser>().HasKey(p => p.UserId);
            modelBuilder.Entity<PrePayMeter>().HasKey(p => p.MeterNumber);
            modelBuilder.Entity<Transaction>().HasKey(p => p.TransactionId);
            modelBuilder.Entity<Transaction>().Property(e => e.TransactionId).ValueGeneratedOnAdd();
            modelBuilder.Entity<TransactionCharge>().HasKey(p => p.TransactionChargeId);
            modelBuilder.Entity<TransactionCharge>().Property(e => e.TransactionChargeId).ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);
        }

        private async Task SetDbInSimpleMode(CancellationToken cancellationToken)
        {
            var dbName = this.Database.GetDbConnection().Database;

            var connection = this.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            // 1. Check current recovery model
            await using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
SELECT recovery_model_desc
FROM sys.databases
WHERE name = @dbName;
";
            var param = checkCommand.CreateParameter();
            param.ParameterName = "@dbName";
            param.Value = dbName;
            checkCommand.Parameters.Add(param);

            var result = await checkCommand.ExecuteScalarAsync(cancellationToken);
            var currentRecoveryModel = result?.ToString();

            if (currentRecoveryModel != "SIMPLE")
            {
                // 2. Alter database outside transaction
                await using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = $@"
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE;
ALTER DATABASE [{dbName}] SET MULTI_USER;
";
                // Execute outside EF transaction
                await alterCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public virtual async Task MigrateAsync(CancellationToken cancellationToken)
        {
            if (this.Database.IsSqlServer())
            {
                try
                {
                    await this.Database.MigrateAsync(cancellationToken);
                    await this.SetDbInSimpleMode(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    throw new InvalidOperationException("An error occurred while migrating the database.", ex);
                }
            }
        }
    }
}
