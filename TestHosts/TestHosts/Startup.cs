using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Shared.Logger.TennantContext;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Logger;
using TestHosts.Common;
using TestHosts.Database.PataPawa;
using TestHosts.Database.TestBank;

namespace TestHosts
{
    using CoreWCF;
    using CoreWCF.Configuration;
    using CoreWCF.Description;
    using CoreWCF.IdentityModel.Protocols.WSTrust;
    using Database.PataPawa;
    using Database.TestBank;
    using HealthChecks.UI.Client;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog;
    using Shared.EntityFramework;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Shared.Middleware;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Metrics;
    using System.DirectoryServices.Protocols;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;
    using TestHosts.SoapServices;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    [ExcludeFromCodeCoverage]
    public class PendingPrePaymentProcessor : BackgroundService{
        private readonly IDbContextResolver<PataPawaContext> Resolver;

        public PendingPrePaymentProcessor(IDbContextResolver<PataPawaContext> resolver){
            this.Resolver = resolver;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken){
            while (stoppingToken.IsCancellationRequested == false){
                // TODO: may introduce a date filter
                using ResolvedDbContext<PataPawaContext>? resolvedContext = this.Resolver.Resolve("PataPawaReadModel");

                var pendingTransactions = await resolvedContext.Context.Transactions.Where(t => t.IsPending).OrderBy(t => t.Date).ToListAsync(stoppingToken);

                if (pendingTransactions.Any()){
                    // Process the pending transactions
                    foreach (Transaction pendingTransaction in pendingTransactions){

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
                        pendingTransaction.Charges = new List<TransactionCharge>{
                                                                                    new TransactionCharge{
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
                await pataPawaContext.Database.MigrateAsync(cancellationToken);
            }
            //else {
            //    await pataPawaContext.Database.EnsureCreatedAsync(cancellationToken);
            //}

            TestBankContext bankContext = scope.ServiceProvider.GetRequiredService<TestBankContext>();
            if (bankContext.Database.IsRelational()) {
                await bankContext.Database.MigrateAsync(cancellationToken);
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