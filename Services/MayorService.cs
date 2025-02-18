using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public MayorService(ILogger<MayorService> logger, ISession session)
    {
        _logger = logger;
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
        var ids = Enumerable.Range(from - 1, to - from + 1);
        var all = new List<ModelElectionPeriod>();
        foreach (var item in ids.Batch(100).ToList())
        {
            var data = (await electionPeriods.Where(p => item.Contains(p.Year)).ExecuteAsync()).ToList();
            all.AddRange(data.Select(ConvertFromDb()));
        }
        return all;
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
        }
    }

    public class ElectionStorage
    {
        public int Year { get; set; }
        public string MayorJson { get; set; }
        public string CandidatesJson { get; set; }
    }
}