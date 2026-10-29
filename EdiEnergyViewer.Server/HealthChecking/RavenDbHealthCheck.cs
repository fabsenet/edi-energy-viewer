using Microsoft.Extensions.Diagnostics.HealthChecks;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;

namespace Fabsenet.EdiEnergyViewer.HealthChecking;

// IDocumentStore is resolved lazily, because its creation (connect + index creation) can fail as well
public class RavenDbHealthCheck(IServiceProvider serviceProvider, ILogger<RavenDbHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = serviceProvider.GetRequiredService<IDocumentStore>();
            var stats = await store.Maintenance.SendAsync(new GetStatisticsOperation(), cancellationToken);

            return HealthCheckResult.Healthy(data: new Dictionary<string, object>
            {
                ["Database"] = store.Database,
                ["CountOfDocuments"] = stats.CountOfDocuments,
                ["CountOfIndexes"] = stats.CountOfIndexes,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RavenDB health check failed");
            return HealthCheckResult.Unhealthy("Unexpected exception", ex);
        }
    }
}
