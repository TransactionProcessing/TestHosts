using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestHosts
{
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //At this stage, we only need our hosting file for ip and ports
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(fi.Directory.FullName)
                                                                  .AddJsonFile("hosting.json", optional: false)
                                                                  .AddJsonFile("hosting.development.json", optional: true)
                                                                  .AddEnvironmentVariables().Build();

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseWindowsService();
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                                                 {
                                                     webBuilder.UseStartup<Startup>();
                                                     webBuilder.UseConfiguration(config);
                                                     webBuilder.UseKestrel();
                                                     webBuilder.ConfigureKestrel((context,
                                                                                  options) => {
                                                                                     options.AllowSynchronousIO = true;
                                                                                 });
                                                 });
            return hostBuilder;
        }
    }
}
