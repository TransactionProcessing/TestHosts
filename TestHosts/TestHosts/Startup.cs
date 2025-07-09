using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using TestHosts.Common;

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
    using Shared.EntityFramework;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Shared.Middleware;
    using Shared.TennantContext;
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

    public class Startup
    {
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
                                                                      .AddJsonFile($"/home/txnproc/config/appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                                                      .AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;
        }

        public static IConfigurationRoot Configuration { get; set; }

        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);

            services.AddHealthChecks().AddSqlServer(connectionString: ConfigurationReader.GetConnectionString("HealthCheck"),
                                                    healthQuery: "SELECT 1;",
                                                    name: "Read Model Server",
                                                    failureStatus: HealthStatus.Degraded,
                                                    tags: new[] { "db", "sql", "sqlserver" });

            services.AddControllers().AddNewtonsoftJson(options =>
                                                        {
                                                            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                                            options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
                                                            options.SerializerSettings.Formatting = Formatting.Indented;
                                                            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                                                            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                                        });

            // Register the Swagger generator, defining 1 or more Swagger documents
            //services.AddSwaggerGen(c =>
            //                       {
            //                           c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            //                       });
            services.AddSingleton(typeof(IDbContextResolver<>), typeof(DbContextResolver<>));
            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") || Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true)
            {
                services.AddDbContext<TestBankContext>(builder => builder.UseInMemoryDatabase("TestBankReadModel"));
                services.AddDbContext<PataPawaContext>(builder => builder.UseInMemoryDatabase("PataPawaReadModel"));

            }
            else
            {
                String testBankConnectionString = ConfigurationReader.GetConnectionString("TestBankReadModel");
                services.AddDbContext<TestBankContext>(builder => builder.UseSqlServer(testBankConnectionString));
                
                String pataPawaConnectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
                services.AddDbContext<PataPawaContext>(builder => builder.UseSqlServer(pataPawaConnectionString));
            }
            services.AddScoped<TenantContext>(x => new TenantContext());
            services.AddSingleton<PataPawaPostPayService>();
            services.AddMvc();

            services.AddServiceModelServices().AddServiceModelMetadata();
            services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
            services.AddSingleton<IServiceBehavior, CorrelationIdBehavior>();


            bool logRequests = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
            bool logResponses = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
            LogLevel middlewareLogLevel = ConfigurationReader.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

            RequestResponseMiddlewareLoggingConfig config =
                new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

            services.AddSingleton(config);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            String nlogConfigFilename = "nlog.config";

            if (env.IsDevelopment())
            {
                var developmentNlogConfigFilename = "nlog.development.config";
                if (File.Exists(Path.Combine(env.ContentRootPath, developmentNlogConfigFilename)))
                {
                    nlogConfigFilename = developmentNlogConfigFilename;
                }
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(env.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("TestHosts");

            Logger.Initialise(logger);
            app.UseMiddleware<TenantMiddleware>();
            app.AddRequestLogging();
            app.AddResponseLogging();
            app.AddExceptionHandler();

            app.UseRouting();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            //app.UseSwagger();

            //// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            //// specifying the Swagger JSON endpoint.
            //app.UseSwaggerUI(c =>
            //                 {
            //                     c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            //                     c.RoutePrefix = string.Empty;
            //                 });

            // this will do the initial DB population
            InitializeDatabase(app).Wait(CancellationToken.None);
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("health", new HealthCheckOptions()
                                                    {
                                                        Predicate = _ => true,
                                                        ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
                                                    });
                endpoints.MapHealthChecks("healthui", new HealthCheckOptions()
                                                      {
                                                          Predicate = _ => true,
                                                          ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                                                      });
            });

            app.UseServiceModel(builder => {
                                    builder.AddService<PataPawaPostPayService>((serviceOptions) => {
                                                                                  serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
                                    })
                                           // Add a BasicHttpBinding at a specific endpoint
                                           .AddServiceEndpoint<PataPawaPostPayService, IPataPawaPostPayService>(new BasicHttpBinding(), "/PataPawaPostPayService/basichttp");
                                    });

            ServiceMetadataBehavior serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
            serviceMetadataBehavior.HttpGetEnabled = true;

            

        }

        async Task InitializeDatabase(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                TestBankContext testbankDbContext = serviceScope.ServiceProvider.GetRequiredService<TestBankContext>();
                if (testbankDbContext.Database.IsRelational())
                {
                    testbankDbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                    try {
                        await testbankDbContext.MigrateAsync(CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }

                PataPawaContext pataPawaContext = serviceScope.ServiceProvider.GetRequiredService<PataPawaContext>();
                if (pataPawaContext.Database.IsRelational())
                {
                    pataPawaContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                    pataPawaContext.Database.Migrate();
                }
            }
        }
    }

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
