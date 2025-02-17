using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Coflnet.Sky.Mayor.Models;
using Microsoft.Extensions.Logging;

namespace Coflnet.Sky.Mayor.Services;

public class MayorService
{
    private readonly ILogger<MayorService> _logger;
    private readonly Table<ModelElectionPeriod> electionPeriods;

    public MayorService(ILogger<MayorService> logger, ISession session)
    {
        _logger = logger;
        var mapping = new MappingConfiguration().Define(
            new Map<ModelElectionPeriod>()
                .TableName("election_periods")
                .PartitionKey(e => e.Year)
                .Column(e => e.Year, cm => cm.WithName("year"))
                .Column(e => e.Candidates, cm => cm.WithName("candidates"))
        );
        electionPeriods = new Table<ModelElectionPeriod>(session, mapping);
        electionPeriods.CreateIfNotExists();
    }

    public async Task<ModelElectionPeriod> GetElectionPeriod(int year)
    {
        return await electionPeriods.Where(p => p.Year == year).FirstOrDefault().ExecuteAsync();
    }

    internal async Task<IEnumerable<ModelElectionPeriod>> GetElectionPeriods(long from, long to)
    {
        return await electionPeriods.Where(p => p.Year >= from && p.Year <= to).ExecuteAsync();
    }

    internal async Task InsertElectionPeriods(List<ModelElectionPeriod> periods)
    {
        foreach (var period in periods)
        {
            await electionPeriods.Insert(period).ExecuteAsync();
        }
    }
}