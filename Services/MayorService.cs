using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Coflnet.Sky.Mayor.Models;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json;

namespace Coflnet.Sky.Mayor.Services;

public class MayorService
{
    private readonly ILogger<MayorService> _logger;
    private readonly Table<ElectionStorage> electionPeriods;
    private readonly object currentLock = new object();
    private ModelElectionPeriod currentElection = null;
    private ModelCandidate currentMayor = null;

    /// <summary>
    /// In memory index of all election periods keyed by year.
    /// Holds only the lightweight data needed by the range endpoint
    /// (winner + perk names, no candidates and no perk descriptions),
    /// which is tiny because only about a dozen mayors rotate.
    /// </summary>
    private readonly ConcurrentDictionary<int, ModelElectionPeriod> cache = new();
    private volatile bool cacheLoaded = false;
    private readonly SemaphoreSlim loadLock = new(1, 1);

    public MayorService(ILogger<MayorService> logger, ISession session)
    {
        _logger = logger;
        logger.LogInformation("Setting up MayorService database tables");
        var mapping = new MappingConfiguration().Define(
            new Map<ElectionStorage>()
                .TableName("election_periods")
                .PartitionKey(e => e.Year)
                .Column(e => e.Year, cm => cm.WithName("year"))
                .Column(e => e.MayorJson, cm => cm.WithName("mayor"))
                .Column(e => e.CandidatesJson, cm => cm.WithName("candidates"))
        );
        electionPeriods = new Table<ElectionStorage>(session, mapping);
        electionPeriods.CreateIfNotExists();
    }

    public async Task<ModelElectionPeriod> GetElectionPeriod(int year)
    {
        var element = await electionPeriods.Where(p => p.Year == year).FirstOrDefault().ExecuteAsync();
        if (element == null)
        {
            return null;
        }
        return ConvertFromDb()(element);
    }

    internal async Task<IEnumerable<ModelElectionPeriod>> GetElectionPeriods(int from, int to)
    {
        if (!cacheLoaded)
        {
            await LoadCache();
        }

        // years from-1 .. to inclusive, served entirely from memory
        var result = new List<ModelElectionPeriod>();
        for (var year = from - 1; year <= to; year++)
        {
            if (cache.TryGetValue(year, out var period))
            {
                result.Add(CloneForResponse(period));
            }
        }
        return result;
    }

    /// <summary>
    /// Loads every election period into the in memory cache. Runs a single full
    /// table scan; it is cheap because there are only a few hundred small rows.
    /// Safe to call multiple times, only the first call hits the database.
    /// </summary>
    public async Task LoadCache()
    {
        if (cacheLoaded)
        {
            return;
        }
        await loadLock.WaitAsync();
        try
        {
            if (cacheLoaded)
            {
                return;
            }
            var all = (await electionPeriods.ExecuteAsync()).Select(ConvertFromDb());
            foreach (var period in all)
            {
                cache[period.Year] = ToLightweight(period);
            }
            cacheLoaded = true;
            _logger.LogInformation("Loaded {Count} election periods into memory", cache.Count);
        }
        finally
        {
            loadLock.Release();
        }
    }

    /// <summary>
    /// Strips an election period down to the data the range endpoint needs:
    /// the winner with perk names, without candidates or perk descriptions.
    /// </summary>
    private static ModelElectionPeriod ToLightweight(ModelElectionPeriod period)
    {
        return new ModelElectionPeriod
        {
            Year = period.Year,
            Candidates = null,
            Winner = period.Winner == null ? null : new ModelWinner
            {
                Key = period.Winner.Key,
                Name = period.Winner.Name,
                Votes = period.Winner.Votes,
                Minister = period.Winner.Minister,
                Perks = period.Winner.Perks?.Select(p => new ModelPerk
                {
                    Name = p.Name,
                    Minister = p.Minister
                }).ToList()
            }
        };
    }

    /// <summary>
    /// Returns a fresh wrapper so the caller can set Start/End without mutating
    /// the shared cached instance. The winner is immutable in the response so it
    /// is shared by reference.
    /// </summary>
    private static ModelElectionPeriod CloneForResponse(ModelElectionPeriod period)
    {
        return new ModelElectionPeriod
        {
            Year = period.Year,
            Candidates = null,
            Winner = period.Winner
        };
    }

    private static Func<ElectionStorage, ModelElectionPeriod> ConvertFromDb()
    {
        return p => new ModelElectionPeriod
        {
            Year = p.Year,
            Winner = JsonConvert.DeserializeObject<ModelWinner>(p.MayorJson ?? "{}"),
            Candidates = JsonConvert.DeserializeObject<List<ModelCandidate>>(p.CandidatesJson ?? "[]")
        };
    }

    internal async Task InsertElectionPeriods(List<ModelElectionPeriod> periods)
    {
        foreach (var period in periods)
        {
            await electionPeriods.Insert(new()
            {
                Year = period.Year,
                MayorJson = JsonConvert.SerializeObject(period.Winner),
                CandidatesJson = JsonConvert.SerializeObject(period.Candidates)
            }).ExecuteAsync();
            // keep the in memory index in sync so the range endpoint stays fast and current
            cache[period.Year] = ToLightweight(period);
        }
    }

    internal void SetCurrentMayor(Models.Mayor mayor)
    {
        lock (currentLock)
        {
            currentMayor = mayor == null
                ? null
                : new ModelWinner
                {
                    Key = mayor.key,
                    Name = mayor.name,
                    Perks = MapPerks(mayor.perks),
                    Votes = mayor.election?.candidates?.OrderByDescending(candidate => candidate.votes).FirstOrDefault()?.votes ?? 0,
                    Minister = mayor.minister
                };
        }
    }

    internal void SetCurrentElection(Current current)
    {
        if (current is null)
        {
            lock (currentLock)
            {
                currentElection = null;
            }
            return;
        }

        var mapped = new ModelElectionPeriod
        {
            Year = current.year,
            Candidates = current.candidates?.Select(MapCandidate).ToList()
        };

        lock (currentLock)
        {
            currentElection = mapped;
        }
    }

    internal ModelCandidate GetCurrentMayor()
    {
        lock (currentLock)
        {
            return currentMayor;
        }
    }

    public ModelCandidate GetCurrentLeader()
    {
        ModelElectionPeriod ce;
        lock (currentLock)
        {
            ce = currentElection;
        }
        if (ce == null || ce.Candidates == null || ce.Candidates.Count == 0) return null;
        return ce.Candidates.OrderByDescending(c => c.Votes).FirstOrDefault();
    }

    private static ModelCandidate MapCandidate(Candidate candidate)
    {
        if (candidate == null)
        {
            return null;
        }

        return new ModelCandidate
        {
            Key = candidate.key,
            Name = candidate.name,
            Perks = MapPerks(candidate.perks),
            Votes = candidate.votes
        };
    }

    private static List<ModelPerk> MapPerks(IEnumerable<Perk> perks)
    {
        return perks?.Select(perk => new ModelPerk
        {
            Name = perk.name,
            Description = perk.description,
            Minister = perk.minister
        }).ToList() ?? new List<ModelPerk>();
    }

    public class ElectionStorage
    {
        public int Year { get; set; }
        public string MayorJson { get; set; }
        public string CandidatesJson { get; set; }
    }
}