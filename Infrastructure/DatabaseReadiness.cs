using System;
using System.Threading;
using System.Threading.Tasks;
using Cassandra;
using Microsoft.Extensions.DependencyInjection;

namespace Coflnet.Sky.Mayor.Infrastructure;

internal static class DatabaseReadiness
{
    public static Task CheckAsync(IServiceProvider services, CancellationToken cancellationToken = default) =>
        Task.Run(async () =>
        {
            var session = services.GetRequiredService<ISession>();
            await session.ExecuteAsync(new SimpleStatement("SELECT key FROM system.local").SetReadTimeoutMillis(3000));
        }, cancellationToken).WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
}
