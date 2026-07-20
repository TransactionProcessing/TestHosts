
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Shared.EntityFramework;
using Shared.Extensions;
using Shared.General;
using Shared.Middleware;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sentry.Extensibility;
using Shared.Logger.TennantContext;
using TestHosts;
using TestHosts.AgencyBanking.Database;
using TestHosts.AgencyBanking.Endpoints;
using TestHosts.AgencyBanking.Services;
using TestHosts.Common;
using TestHosts.Database.PataPawa;
using TestHosts.Database.TestBank;
using TestHosts.Endpoints;
using TestHosts.SoapServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

try {

    WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory });

    // ----------------------------------------------------------------------
    // Load custom hosting configuration (your existing hosting.json setup)
    // ----------------------------------------------------------------------
    FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
    builder.Configuration.SetBasePath(fi.Directory!.FullName).AddJsonFile("hosting.json", optional: false, reloadOnChange: true).AddJsonFile("hosting.development.json", optional: true, reloadOnChange: true).AddJsonFile("/home/txnproc/config/appsettings.json", true, true).AddJsonFile($"/home/txnproc/config/appsettings.{builder.Environment.EnvironmentName}.json", optional: true).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

    ConfigurationReader.Initialise(builder.Configuration);
    // ----------------------------------------------------------------------
    // Configure Windows Service mode
    // ----------------------------------------------------------------------
    if (WindowsServiceHelpers.IsWindowsService())
        builder.Host.UseWindowsService();

    // ----------------------------------------------------------------------
    // Configure Kestrel
    // ----------------------------------------------------------------------
    builder.WebHost.ConfigureKestrel(options => { options.AllowSynchronousIO = true; });

    // ----------------------------------------------------------------------
    // Configure Sentry
    // ----------------------------------------------------------------------
    var sentrySection = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "Dsn", "N/A");
    if (sentrySection != "N/A") {
        // Replace the condition below if you intended to only enable Sentry in certain environments.
        if (builder.Environment.IsDevelopment() == false) {
            builder.WebHost.UseSentry(o => {
                o.Dsn = sentrySection;
                o.SendDefaultPii = true;
                o.MaxRequestBodySize = RequestSize.Always;
                o.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                o.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            });
        }
    }

    // ----------------------------------------------------------------------
    // Configure NLog
    // ----------------------------------------------------------------------
    string nlogConfigFilename = "nlog.config";
    if (builder.Environment.IsDevelopment()) {
        String devFile = Path.Combine(builder.Environment.ContentRootPath, "nlog.development.config");
        if (File.Exists(devFile))
            nlogConfigFilename = "nlog.development.config";
    }

    LogManager.Setup().LoadConfigurationFromFile(Path.Combine(builder.Environment.ContentRootPath, nlogConfigFilename));
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // ----------------------------------------------------------------------
    // Add application and framework services
    // ----------------------------------------------------------------------
    builder.Services.AddControllers().AddNewtonsoftJson(options => {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
        options.SerializerSettings.Formatting = Formatting.Indented;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });
    builder.Services.AddHealthChecks().AddSqlServer(connectionString: ConfigurationReader.GetConnectionString("HealthCheck"), healthQuery: "SELECT 1;", name: "Read Model Server", failureStatus: HealthStatus.Degraded, tags: new[] { "db", "sql", "sqlserver" });
    
    builder.Services.AddServiceModelServices().AddServiceModelMetadata();

    builder.Services.AddSingleton(typeof(IDbContextResolver<>), typeof(DbContextResolver<>));
    if (builder.Environment.IsEnvironment("IntegrationTest") || builder.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true) {
        builder.Services.AddDbContext<TestBankContext>(builder => builder.UseInMemoryDatabase(Constants.TestBankReadModelConfig));
        builder.Services.AddDbContext<PataPawaContext>(builder => builder.UseInMemoryDatabase(Constants.PataPawaReadModelConfig));
        builder.Services.AddDbContext<AgencyBankingDbContext>(builder => builder.UseInMemoryDatabase(Constants.AgencyBankingReadModelConfig));

    }
    else {
        String testBankConnectionString = ConfigurationReader.GetConnectionString(Constants.TestBankReadModelConfig);
        builder.Services.AddDbContext<TestBankContext>(builder => builder.UseSqlServer(testBankConnectionString));

        String pataPawaConnectionString = ConfigurationReader.GetConnectionString(Constants.PataPawaReadModelConfig);
        builder.Services.AddDbContext<PataPawaContext>(builder => builder.UseSqlServer(pataPawaConnectionString));

        String agencyBankingConnectionString = ConfigurationReader.GetConnectionString(Constants.AgencyBankingReadModelConfig);
        builder.Services.AddDbContext<AgencyBankingDbContext>(builder => builder.UseSqlServer(agencyBankingConnectionString));
    }

    builder.Services.AddScoped<TenantContext>(x => new TenantContext());
    builder.Services.AddSingleton<PataPawaPostPayService>();
    // Agency banking core services
    builder.Services.AddScoped<IFloatService, FloatService>();
    builder.Services.AddScoped<ILedgerService, LedgerService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<ISettlementService, SettlementService>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();
    // Add your background hosted service
    builder.Services.AddHostedService<PendingPrePaymentProcessor>(provider => {
        IDbContextResolver<PataPawaContext> contextResolver = provider.GetRequiredService<IDbContextResolver<PataPawaContext>>();
        return new PendingPrePaymentProcessor(contextResolver);
    });

    // Database initialization will now be handled by a hosted service
    builder.Services.AddHostedService<DatabaseInitializerHostedService>();

    builder.Services.AddScoped<TenantContext>(x => new TenantContext());
    builder.Services.AddSingleton<PataPawaPostPayService>();
    builder.Services.AddMvc();

    // Add authorization services so AuthorizationMiddleware can be constructed
    builder.Services.AddAuthorization();

    builder.Services.AddServiceModelServices().AddServiceModelMetadata();
    builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
    builder.Services.AddSingleton<IServiceBehavior, CorrelationIdBehavior>();


    bool logRequests = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
    bool logResponses = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
    LogLevel middlewareLogLevel = ConfigurationReader.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

    RequestResponseMiddlewareLoggingConfig config = new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

    builder.Services.AddSingleton(config);

    // ----------------------------------------------------------------------
    // Build the app
    // ----------------------------------------------------------------------
    WebApplication app = builder.Build();

    // Create a scoped logger and assign it
    using (IServiceScope scope = app.Services.CreateScope()) {
        ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger loggerObject = loggerFactory.CreateLogger("AppStartup");
        Shared.Logger.Logger.Initialise(loggerObject);
    }

    // ----------------------------------------------------------------------
    // Middleware pipeline
    // ----------------------------------------------------------------------
    if (app.Environment.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
    }

    // Custom middleware
    app.UseMiddleware<TenantMiddleware>();
    app.UseAgencyBankingRequestLogging();
    app.AddRequestResponseLogging();
    app.AddExceptionHandler();

    app.UseRouting();

    // Authorization middleware requires AddAuthorization() to be registered above
    app.UseAuthorization();

    // ----------------------------------------------------------------------
    // Endpoints
    // ----------------------------------------------------------------------
    app.MapControllers();

    // Map PataPawa minimal API endpoints implemented in PataPawaPrePaidEndpoints
    //app.MapPataPawaPrepayEndpoints();
    // Map developer minimal API endpoints
    //app.MapPataPawaDeveloperEndpoints();

    // Agency Banking
    app.MapStaticQueryEndpoints();
    app.MapAgencyBankingSystemSetupEndpoints();
    app.MapAgencyBankingAccountEndpoints();
    app.MapAgencyBankingAuthenticationEndpoints();
    app.MapAgencyBankingTransactionEndpoints();

    app.MapHealthChecks("health", new HealthCheckOptions { Predicate = _ => true, ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse });

    app.MapHealthChecks("healthui", new HealthCheckOptions { Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

    // ----------------------------------------------------------------------
    // CoreWCF setup
    // ----------------------------------------------------------------------
    app.UseServiceModel(builder => { builder.AddService<PataPawaPostPayService>(options => { options.DebugBehavior.IncludeExceptionDetailInFaults = true; }).AddServiceEndpoint<PataPawaPostPayService, IPataPawaPostPayService>(new BasicHttpBinding(), "/PataPawaPostPayService/basichttp"); });

    ServiceMetadataBehavior metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadataBehavior.HttpGetEnabled = true;

    // ----------------------------------------------------------------------
    // Start the application
    // ----------------------------------------------------------------------
    app.Run();

    Shared.Logger.Logger.LogWarning("Application started successfully");
}
catch (Exception ex) {
    Shared.Logger.Logger.LogError("Application stopped due to exception", ex);
    throw;
}
finally {
    LogManager.Shutdown();
}


public static class Constants {
    public static readonly String PataPawaReadModelConfig = "PataPawaReadModel";
    public static readonly String TestBankReadModelConfig = "TestBankReadModel";
    public static readonly String AgencyBankingReadModelConfig = "AgencyBankingReadModel";
}
