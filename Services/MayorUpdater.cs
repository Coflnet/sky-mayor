using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cassandra.Data.Linq;
using Coflnet.Sky.Mayor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Coflnet.Sky.Mayor.Services;

public class MayorUpdater : BackgroundService
{
    private readonly ILogger<MayorUpdater> logger;
    private readonly MayorService mayorService;

    public MayorUpdater(ILogger<MayorUpdater> logger, MayorService mayorService)
    {
        this.logger = logger;
        this.mayorService = mayorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var url = "https://api.hypixel.net/v2/resources/skyblock/election";
        logger.LogInformation("Starting mayor data update");
        var client = new HttpClient();
        while (!stoppingToken.IsCancellationRequested)
        {
            await Update(url, client);
            await Task.Delay(1000 * 60 * 60, stoppingToken);
        }
    }

    private async Task Update(string url, HttpClient client)
    {
        var data = await client.GetStringAsync(url);
        var electionData = JsonConvert.DeserializeObject<ElectionResult>(data);
        if (electionData?.mayor == null)
        {
            logger.LogWarning("No active mayor found in API response. Skipping historic mayor insertion.");
            try
            {
                mayorService.SetCurrentElection(electionData?.current);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Failed to set current election when mayor data was missing");
            }
            return;
        }

        var mayor = electionData.mayor;
        var candidates = mayor.election?.candidates ?? new List<Candidate>();
        var perks = mayor.perks ?? new List<Perk>();

        var electionPeriod = new ModelElectionPeriod
        {
            Year = mayor.election?.year ?? 0,
            Winner = new ModelWinner
            {
                Key = mayor.key,
                Name = mayor.name,
                Perks = perks.Select(p => new ModelPerk
                {
                    Name = p.name,
                    Description = p.description,
                    Minister = p.minister
                }).ToList(),
                Votes = candidates.OrderByDescending(c => c.votes).FirstOrDefault()?.votes ?? 0,
                Minister = mayor.minister
            },
            Candidates = candidates.Select(c => new ModelCandidate
            {
                Key = c.key,
                Name = c.name,
                Perks = (c.perks ?? new List<Perk>()).Select(p => new ModelPerk
                {
                    Name = p.name,
                    Description = p.description,
                    Minister = p.minister
                }).ToList(),
                Votes = c.votes
            }).ToList(),
        };

        await mayorService.InsertElectionPeriods(new List<ModelElectionPeriod> { electionPeriod });
        mayorService.SetCurrentElection(electionData.current);
        logger.LogInformation("Updated mayor data for year {Year} current election winner {leader}", electionPeriod.Year, electionData.current?.candidates?.OrderByDescending(c => c.votes).FirstOrDefault()?.name);
    }
}