using System;
using System.Threading.Tasks;
using Cassandra;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Coflnet.Sky.Mayor.Infrastructure;

public class DatabaseReadinessTests
{
    [Test]
    public async Task ReadinessQueriesTheDatabaseOnEveryCheck()
    {
        var session = new Mock<ISession>();
        session.Setup(s => s.ExecuteAsync(It.IsAny<IStatement>())).ReturnsAsync(new RowSet());
        using var services = new ServiceCollection().AddSingleton(session.Object).BuildServiceProvider();
        await DatabaseReadiness.CheckAsync(services);
        await DatabaseReadiness.CheckAsync(services);
        session.Verify(s => s.ExecuteAsync(It.Is<SimpleStatement>(q => q.QueryString == "SELECT key FROM system.local")), Times.Exactly(2));
    }

    [Test]
    public void UnavailableDatabaseFailsTheStartupCheck()
    {
        using var services = new ServiceCollection().AddSingleton<ISession>(_ =>
            throw new InvalidOperationException("synthetic unavailable database")).BuildServiceProvider();
        Assert.ThrowsAsync<InvalidOperationException>(() => DatabaseReadiness.CheckAsync(services));
    }

    [Test]
    public void StalledDatabaseIsBoundedForStartupAndReadiness()
    {
        var session = new Mock<ISession>();
        session.Setup(s => s.ExecuteAsync(It.IsAny<IStatement>())).Returns(new TaskCompletionSource<RowSet>().Task);
        using var services = new ServiceCollection().AddSingleton(session.Object).BuildServiceProvider();
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        Assert.ThrowsAsync<TimeoutException>(() => DatabaseReadiness.CheckAsync(services));
        Assert.That(elapsed.Elapsed, Is.LessThan(TimeSpan.FromSeconds(7)));
    }
}
