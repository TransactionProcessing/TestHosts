namespace TestHosts.Database.TestBank
{
    using Microsoft.EntityFrameworkCore;
    using Shared.General;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

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
    }
}
