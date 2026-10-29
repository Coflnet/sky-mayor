using Microsoft.AspNetCore.Hosting;
using Coflnet.Sky.Mayor.Infrastructure;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Coflnet.Sky.Mayor
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await DatabaseReadiness.CheckAsync(host.Services);
            await host.RunAsync();
        }

        /// <summary>
        /// Create the host builder.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>IHostBuilder</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .ConfigureTransportSecurity();
                });
    }
}
