using System.Text.Json;
using Fabsenet.EdiEnergyViewer;
using Fabsenet.EdiEnergyViewer.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;

namespace EdiEnergyViewer.Tests;

/// <summary>
/// Integration tests for <see cref="Fabsenet.EdiEnergyViewer.Controllers.EdiDocumentsController"/>.
/// Uses an embedded RavenDB instance seeded with all 1051 documents from SampleData.json to
/// reproduce the production environment closely enough to catch query-level bugs such as the
/// silent truncation caused by <c>.Take(1024)</c> when the collection exceeds 1024 documents.
/// </summary>
public class EdiDocumentsControllerTests(RavenDbFixture fixture) : IClassFixture<RavenDbFixture>
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Regression test: MSCONS MIG 2.5 (IsLatestVersion = true) must appear in the response
    /// even though the total document count (1051) exceeds the former hard-coded Take(1024) limit.
    /// </summary>
    [Fact]
    public async Task GetAllEdiDocuments_IncludesMsconsMig25_WhenCollectionExceedsFormerTakeLimit()
    {
        await using var factory = BuildFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/EdiDocuments");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var docs = JsonSerializer.Deserialize<List<EdiDocumentSlim>>(content, JsonOptions);

        Assert.NotNull(docs);
        Assert.True(docs.Count > 1024,
            $"Expected more than 1024 documents but got {docs.Count}. " +
            "The endpoint is likely still truncating results with Take(1024).");

        var mscons25 = docs.FirstOrDefault(d =>
            d.DocumentName == "MSCONS MIG 2.5" && d.IsMig && d.IsLatestVersion);
        Assert.NotNull(mscons25);
    }

    private WebApplicationFactory<Program> BuildFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Testing");
            builder.ConfigureServices(services =>
            {
                // Replace the real DocumentStore (which connects to a configured RavenDB URL)
                // with the embedded test store seeded from SampleData.json.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDocumentStore));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton(fixture.Store);
            });
        });
}
