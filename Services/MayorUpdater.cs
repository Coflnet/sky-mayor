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
        var client = new HttpClient();
        while(!stoppingToken.IsCancellationRequested)
        {
            await Update(url, client);
            await Task.Delay(1000 * 60 * 60, stoppingToken);
        }
    }

    private async Task Update(string url, HttpClient client)
    {
        var data = await client.GetStringAsync(url);
        var electionData = JsonConvert.DeserializeObject<ElectionResult>(data);
        var electionPeriod = new ModelElectionPeriod
        {
            Year = electionData.mayor.election.year,
            Winner = new ModelWinner
            {
                Key = electionData.mayor.key,
                Name = electionData.mayor.name,
                Perks = electionData.mayor.perks.Select(p => new ModelPerk
                {
                    Name = p.name,
                    Description = p.description,
                    Minister = p.minister
                }).ToList(),
                Votes = electionData.mayor.election.candidates.OrderByDescending(c => c.votes).First().votes,
                Minister = electionData.mayor.minister
            },
            Candidates = electionData.mayor.election.candidates.Select(c => new ModelCandidate
            {
                Key = c.key,
                Name = c.name,
                Perks = c.perks.Select(p => new ModelPerk
                {
                    Name = p.name,
                    Description = p.description,
                    Minister = p.minister
                }).ToList(),
                Votes = c.votes
            }).ToList(),
        };
        await mayorService.InsertElectionPeriods(new List<ModelElectionPeriod> { electionPeriod });
        logger.LogInformation("Updated mayor data for year {Year} current election winner {leader}", electionPeriod.Year, electionPeriod.Candidates?.OrderByDescending(c => c.Votes).FirstOrDefault()?.Name);
    }
}