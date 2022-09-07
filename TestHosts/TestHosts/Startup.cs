using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace TestHosts
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using CoreWCF;
    using CoreWCF.Configuration;
    using CoreWCF.Description;
    using Database.PataPawa;
    using Database.TestBank;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Shared.EntityFramework;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using TestHosts.SoapServices;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public class Startup
    {
        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true).AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;
        }

        public static IConfigurationRoot Configuration { get; set; }

        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);

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

            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") || Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true)
            {
                services.AddDbContext<TestBankContext>(builder => builder.UseInMemoryDatabase("TestBankReadModel"));
                DbContextOptionsBuilder<TestBankContext> bankContextBuilder = new DbContextOptionsBuilder<TestBankContext>();
                bankContextBuilder = bankContextBuilder.UseInMemoryDatabase("TestBankReadModel");
                services.AddSingleton<Func<String, TestBankContext>>(cont => (connectionString) => { return new TestBankContext(bankContextBuilder.Options); });

                services.AddDbContext<PataPawaContext>(builder => builder.UseInMemoryDatabase("PataPawaReadModel"));
                DbContextOptionsBuilder<PataPawaContext> pataPawaBuilder = new DbContextOptionsBuilder<PataPawaContext>();
                pataPawaBuilder = pataPawaBuilder.UseInMemoryDatabase("PataPawaReadModel");
                services.AddSingleton<Func<String, PataPawaContext>>(cont => (connectionString) => { return new PataPawaContext(pataPawaBuilder.Options); });
            }
            else
            {
                String testBankConnectionString = ConfigurationReader.GetConnectionString("TestBankReadModel");
                services.AddDbContext<TestBankContext>(builder => builder.UseSqlServer(testBankConnectionString));
                services.AddSingleton<Func<String, TestBankContext>>(cont => (connectionString) => { return new TestBankContext(testBankConnectionString); });
                
                String pataPawaConnectionString = ConfigurationReader.GetConnectionString("PataPawaReadModel");
                services.AddDbContext<PataPawaContext>(builder => builder.UseSqlServer(pataPawaConnectionString));
                services.AddSingleton<Func<String, PataPawaContext>>(cont => (connectionString) => { return new PataPawaContext(pataPawaConnectionString); });
            }

            services.AddSingleton<PataPawaPostPayService>();
            services.AddMvc();

            services.AddServiceModelServices().AddServiceModelMetadata();
            services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            String nlogConfigFilename = "nlog.config";

            if (env.IsDevelopment())
            {
                nlogConfigFilename = $"nlog.{env.EnvironmentName}.config";
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(env.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("TestHosts");

            Logger.Initialise(logger);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.AddRequestLogging();
            //app.AddResponseLogging();
            //app.AddExceptionHandler();

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
            this.InitializeDatabase(app);
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            // Configure an explicit none credential type for WSHttpBinding as it defaults to Windows which requires extra configuration in ASP.NET
            var myWSHttpBinding = new WSHttpBinding(SecurityMode.Transport);
            myWSHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            app.UseServiceModel(builder => {
                                    builder.AddService<PataPawaPostPayService>((serviceOptions) => {
                                                                                  serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
                                                                              })
                                           // Add a BasicHttpBinding at a specific endpoint
                                           .AddServiceEndpoint<PataPawaPostPayService, IPataPawaPostPayService>(new BasicHttpBinding(), "/PataPawaPostPayService/basichttp");
                                    // Add a WSHttpBinding with Transport Security for TLS
                                    //.AddServiceEndpoint<PataPawaPrePayService, IPataPawaPrePayService>(myWSHttpBinding, "/EchoService/WSHttps");
                                });

            var serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
            serviceMetadataBehavior.HttpGetEnabled = true;

            

        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                TestBankContext testbankDbContext = serviceScope.ServiceProvider.GetRequiredService<TestBankContext>();
                if (testbankDbContext.Database.IsRelational())
                {
                    testbankDbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                    testbankDbContext.Database.Migrate();
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
}
