using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Logger;
using TestHosts.Database.PataPawa;
using TestHosts.Database.TestBank;

namespace TestHosts
{
    using Database.PataPawa;
    using Microsoft.EntityFrameworkCore;
    using Shared.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [ExcludeFromCodeCoverage]
    public class PendingPrePaymentProcessor : BackgroundService{
        private readonly IDbContextResolver<PataPawaContext> Resolver;

        public PendingPrePaymentProcessor(IDbContextResolver<PataPawaContext> resolver){
            this.Resolver = resolver;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken){
            while (stoppingToken.IsCancellationRequested == false){
                try {
                    // TODO: may introduce a date filter
                    using ResolvedDbContext<PataPawaContext>? resolvedContext = this.Resolver.Resolve("PataPawaReadModel");

                    var pendingTransactions = await resolvedContext.Context.Transactions.Where(t => t.IsPending).OrderBy(t => t.Date).ToListAsync(stoppingToken);

                    if (pendingTransactions.Any()) {
                        // Process the pending transactions
                        foreach (Transaction pendingTransaction in pendingTransactions) {

                            PrePayMeter meter = await resolvedContext.Context.PrePayMeters.SingleAsync(m => m.MeterNumber == pendingTransaction.MeterNumber, stoppingToken);

                            pendingTransaction.Status = 0;
                            pendingTransaction.Messaage = "success";
                            pendingTransaction.Vendor = "support";
                            pendingTransaction.MeterNumber = meter.MeterNumber;
                            pendingTransaction.ResultCode = "elec000";
                            pendingTransaction.StandardTokenAmt = 64;
                            pendingTransaction.StandardTokenTax = 0;
                            pendingTransaction.Units = 6.1m;
                            pendingTransaction.Token = Guid.NewGuid().ToString("N");
                            pendingTransaction.StandardTokenRctNum = "Ce001OVS3709952";
                            pendingTransaction.Date = DateTime.Now;
                            pendingTransaction.TotalAmount = 400;
                            pendingTransaction.Charges = new List<TransactionCharge> {
                                new TransactionCharge {
                                    ERCCharge = 3.19m,
                                    ForexCharge = 0.47m,
                                    FuelIndexCharge = 2.47m,
                                    InflationAdjustment = 0,
                                    MonthlyFC = 13.27m,
                                    REPCharge = 1.39m,
                                    TotalTax = 15.21m
                                }
                            };
                            pendingTransaction.CustomerName = meter.CustomerName;
                            pendingTransaction.Reference = DateTime.Now.ToString("yyyyMMddhhmmsssfff");
                            pendingTransaction.IsPending = false;

                            await resolvedContext.Context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex) {
                    Logger.LogError("Error processing pending transactions", ex);
                    // Let the service contuine to run and attempt processing in the next cycle
                }

                await Task.Delay(TimeSpan.FromMinutes(1),stoppingToken);
            }
        }
    }
}

public sealed class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning("Starting database initialization...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            PataPawaContext pataPawaContext = scope.ServiceProvider.GetRequiredService<PataPawaContext>();

            if (pataPawaContext.Database.IsRelational()) {
                // Example: apply migrations or seed data
                pataPawaContext.Database.Migrate();
            }
            //else {
            //    await pataPawaContext.Database.EnsureCreatedAsync(cancellationToken);
            //}

            TestBankContext bankContext = scope.ServiceProvider.GetRequiredService<TestBankContext>();
            if (bankContext.Database.IsRelational()) {
                bankContext.Database.Migrate();
            }
            //else {
            //    await bankContext.Database.EnsureCreatedAsync(cancellationToken);
            //}

            Logger.LogWarning("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError("Database initialization failed.", ex);
            throw; // Let the host fail fast if initialization is critical
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}