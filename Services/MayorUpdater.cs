using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Coflnet.Sky.Mayor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Coflnet.Sky.Mayor.Services;

public class MayorUpdater : BackgroundService
{
    private const string ElectionUrl = "https://api.hypixel.net/v2/resources/skyblock/election";
    private static readonly DateTimeOffset VotingOpenAnchor = new(2019, 6, 12, 23, 15, 0, TimeSpan.Zero);
    private static readonly TimeSpan VotingCycle = TimeSpan.FromDays(5) + TimeSpan.FromHours(4);
    private static readonly TimeSpan FastVotingWindow = TimeSpan.FromHours(6);
    private static readonly TimeSpan FastUpdateInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SlowUpdateInterval = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan MinimumDelay = TimeSpan.FromSeconds(1);
    private readonly ILogger<MayorUpdater> logger;
    private readonly MayorService mayorService;

    public MayorUpdater(ILogger<MayorUpdater> logger, MayorService mayorService)
    {
        this.logger = logger;
        this.mayorService = mayorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Starting mayor data update with {FastInterval} refreshes for the first {FastWindow} after each voting open every {VotingCycle}, and {SlowInterval} refreshes otherwise",
            FastUpdateInterval,
            FastVotingWindow,
            VotingCycle,
            SlowUpdateInterval);
        using var client = new HttpClient();

        try
        {
            await mayorService.LoadCache();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to warm the in memory election period cache on startup");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Update(ElectionUrl, client, stoppingToken);
            await Task.Delay(GetDelayUntilNextUpdate(DateTimeOffset.UtcNow), stoppingToken);
        }
    }

    private static TimeSpan GetDelayUntilNextUpdate(DateTimeOffset now)
    {
        var lastVotingOpen = GetLastVotingOpen(now);
        var timeSinceVotingOpen = now - lastVotingOpen;
        var inFastVotingWindow = timeSinceVotingOpen < FastVotingWindow;
        var interval = inFastVotingWindow ? FastUpdateInterval : SlowUpdateInterval;
        var timeUntilWindowBoundary = inFastVotingWindow
            ? FastVotingWindow - timeSinceVotingOpen
            : lastVotingOpen.Add(VotingCycle) - now;
        var delay = interval < timeUntilWindowBoundary ? interval : timeUntilWindowBoundary;

        return delay > TimeSpan.Zero ? delay : MinimumDelay;
    }

    private static DateTimeOffset GetLastVotingOpen(DateTimeOffset now)
    {
        if (now <= VotingOpenAnchor)
        {
            return VotingOpenAnchor;
        }

        var elapsedTicks = (now - VotingOpenAnchor).Ticks;
        var completedCycles = elapsedTicks / VotingCycle.Ticks;
        return VotingOpenAnchor.AddTicks(completedCycles * VotingCycle.Ticks);
    }

    private async Task Update(string url, HttpClient client, CancellationToken stoppingToken)
    {
        try
        {
            var data = await client.GetStringAsync(url, stoppingToken);
            var electionData = JsonConvert.DeserializeObject<ElectionResult>(data);

            mayorService.SetCurrentMayor(electionData?.mayor);
            mayorService.SetCurrentElection(electionData?.current);

            if (electionData?.mayor == null)
            {
                logger.LogWarning("No active mayor found in API response. Skipping historic mayor insertion.");
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
            logger.LogInformation("Updated mayor data for year {Year} current election winner {Leader}", electionPeriod.Year, electionData.current?.candidates?.OrderByDescending(c => c.votes).FirstOrDefault()?.name);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update mayor data from {Url}", url);
        }
    }
}