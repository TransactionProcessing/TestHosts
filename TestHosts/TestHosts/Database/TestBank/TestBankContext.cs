namespace TestHosts.Database.TestBank
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Shared.General;

    public class TestBankContext : DbContext
    {
        private readonly String ConnectionString;

        public DbSet<HostConfiguration> HostConfigurations { get; set; }
        public DbSet<Deposit>  Deposits { get; set; }

        public TestBankContext()
        {
            // This is the migration connection string

            // Get connection string from configuration.
            this.ConnectionString = ConfigurationReader.GetConnectionString("TestBankReadModel");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBankContext" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public TestBankContext(String connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestBankContext" /> class.
        /// </summary>
        /// <param name="dbContextOptions">The database context options.</param>
        public TestBankContext(DbContextOptions<TestBankContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                optionsBuilder.UseSqlServer(this.ConnectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HostConfiguration>().HasKey(h => new
                                                                 {
                                                                     h.HostIdentifier
                                                                 });

            modelBuilder.Entity<Deposit>().HasKey(d => new
                                                                 {
                                                                     d.HostIdentifier,
                                                                     d.DepositId
                                                                 });

            base.OnModelCreating(modelBuilder);
        }
    }
}
