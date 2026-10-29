using System.Text.Json;
using Fabsenet.EdiEnergyViewer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;

namespace EdiEnergyViewer.Tests;

/// <summary>
/// Integration tests for the <c>/alive</c> health check endpoint.
/// </summary>
public class AliveEndpointTests(RavenDbFixture fixture) : IClassFixture<RavenDbFixture>
{
    [Fact]
    public async Task Alive_ReturnsHealthyJsonReport_WithRavenDbEntry()
    {
        await using var factory = BuildFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/alive");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());

        var ravenDb = json.RootElement.GetProperty("results").EnumerateArray()
            .Single(r => r.GetProperty("key").GetString() == "RavenDB");
        Assert.Equal("Healthy", ravenDb.GetProperty("value").GetProperty("status").GetString());
    }

    private WebApplicationFactory<Program> BuildFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDocumentStore));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton(fixture.Store);
            });
        });
}
