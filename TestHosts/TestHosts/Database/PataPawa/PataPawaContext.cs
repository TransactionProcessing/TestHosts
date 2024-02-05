using Microsoft.EntityFrameworkCore;
using Shared.General;
using System;

namespace TestHosts.Database.PataPawa
{
    public class PataPawaContext : DbContext
    {
        private readonly String ConnectionString;
        
        public PataPawaContext()
        {
            // This is the migration connection string

            // Get connection string from configuration.
            this.ConnectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
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
    }
}
