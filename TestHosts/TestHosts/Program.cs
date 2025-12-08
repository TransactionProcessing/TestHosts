
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
    using System.Security;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Shared.Logger.TennantContext;
    using TestHosts;
    using TestHosts.Common;
    using TestHosts.Database.PataPawa;
    using TestHosts.Database.TestBank;
    using TestHosts.SoapServices;
    using LogLevel = Microsoft.Extensions.Logging.LogLevel;

    const String PataPawaReadModelKey = "PataPawaReadModel";
    const String TestBankReadModelKey = "TestBankReadModel";
try {

        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory });
        
        // ----------------------------------------------------------------------
        // Load custom hosting configuration (your existing hosting.json setup)
        // ----------------------------------------------------------------------
        FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
        builder.Configuration.SetBasePath(fi.Directory!.FullName).AddJsonFile("hosting.json", optional: false, reloadOnChange: true).AddJsonFile("hosting.development.json", optional: true, reloadOnChange: true)
            .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
            .AddJsonFile($"/home/txnproc/config/appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

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
        builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });
    builder.Services.AddHealthChecks().AddSqlServer(connectionString: ConfigurationReader.GetConnectionString("HealthCheck"),
            healthQuery: "SELECT 1;",
            name: "Read Model Server",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "sql", "sqlserver" }); ;
        builder.Services.AddServiceModelServices().AddServiceModelMetadata();

        builder.Services.AddSingleton(typeof(IDbContextResolver<>), typeof(DbContextResolver<>));
        if (builder.Environment.IsEnvironment("IntegrationTest") || builder.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true)
        {
            builder.Services.AddDbContext<TestBankContext>(builder => builder.UseInMemoryDatabase(TestBankReadModelKey));
            builder.Services.AddDbContext<PataPawaContext>(builder => builder.UseInMemoryDatabase(PataPawaReadModelKey));

        }
        else
        {
            String testBankConnectionString = ConfigurationReader.GetConnectionString(TestBankReadModelKey);
            builder.Services.AddDbContext<TestBankContext>(builder => builder.UseSqlServer(testBankConnectionString));

            String pataPawaConnectionString = ConfigurationReader.GetConnectionString(PataPawaReadModelKey);
            builder.Services.AddDbContext<PataPawaContext>(builder => builder.UseSqlServer(pataPawaConnectionString));
        }
        builder.Services.AddScoped<TenantContext>(x => new TenantContext());
        builder.Services.AddSingleton<PataPawaPostPayService>();
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

        builder.Services.AddServiceModelServices().AddServiceModelMetadata();
        builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
        builder.Services.AddSingleton<IServiceBehavior, CorrelationIdBehavior>();


        bool logRequests = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
        bool logResponses = ConfigurationReader.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
        LogLevel middlewareLogLevel = ConfigurationReader.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

        RequestResponseMiddlewareLoggingConfig config =
            new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

        builder.Services.AddSingleton(config);

    // ----------------------------------------------------------------------
    // Build the app
    // ----------------------------------------------------------------------
    WebApplication app = builder.Build();

    // Create a scoped logger and assign it
     using (var scope = app.Services.CreateScope())
     {
         var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
         var loggerObject = loggerFactory.CreateLogger("AppStartup");
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
        app.AddRequestLogging();
        app.AddResponseLogging();
        app.AddExceptionHandler();

        app.UseRouting();

// ----------------------------------------------------------------------
// Endpoints
// ----------------------------------------------------------------------
        app.MapControllers();

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
