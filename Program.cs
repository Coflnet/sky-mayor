using System.IO;
using System.Linq;
using Coflnet.Sky.Mayor.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

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
        public static void Main(string[] args)
        {
            var all = JsonConvert.DeserializeObject<ModelElectionPeriod[]>(File.ReadAllText("previous.json"));
            var options = all.SelectMany(c => c.Candidates?.SelectMany(c => c.Perks).Select(p => p.Name) ?? []).Distinct().ToList();
            File.WriteAllText("options.json", JsonConvert.SerializeObject(options));
            CreateHostBuilder(args).Build().Run();
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
                              .UseUrls("http://0.0.0.0:8080/");
                });
    }
}
