using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Smuggler;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace EdiEnergyViewer.Tests;

/// <summary>
/// xUnit class fixture that creates a fresh isolated database on the already-running local
/// RavenDB server, imports SampleData.ravendbdump into it, and tears it down after all
/// tests in the class have run.
///
/// Connection settings are resolved from (in priority order):
///   1. User secrets  (keys: DatabaseUrl, DatabaseCertificate)
///   2. Environment variables prefixed with EDIENERGYVIEWER_
///   3. Fallback: http://127.0.0.1:8080
/// </summary>
public sealed class RavenDbFixture : IAsyncLifetime
{
    private static readonly IConfiguration Config = new ConfigurationBuilder()
        .AddUserSecrets<RavenDbFixture>()
        .AddEnvironmentVariables("EDIENERGYVIEWER_")
        .Build();

    // Unique per test run so parallel runs never collide.
    private readonly string _testDbName = $"EdiDocsIntTest-{Guid.NewGuid():N}";

    public IDocumentStore Store { get; }

    public RavenDbFixture()
    {
        Store = CreateStore(_testDbName);
    }

    public async Task InitializeAsync()
    {
        // Create the temporary database.
        await Store.Maintenance.Server.SendAsync(
            new CreateDatabaseOperation(new DatabaseRecord(_testDbName)));

        // Import the production snapshot.
        var dumpPath = Path.Combine(AppContext.BaseDirectory, "SampleData.ravendbdump");
        var operation = await Store.Smuggler.ImportAsync(
            new DatabaseSmugglerImportOptions
            {
                OperateOnTypes = DatabaseItemType.Documents
            },
            dumpPath);
        await operation.WaitForCompletionAsync(TimeSpan.FromMinutes(2));

        // Wait until all indexes are up to date before tests run.
        await WaitForIndexingAsync();
    }

    public async Task DisposeAsync()
    {
        await Store.Maintenance.Server.SendAsync(
            new DeleteDatabasesOperation(_testDbName, hardDelete: true));
        Store.Dispose();
    }

    private static IDocumentStore CreateStore(string database)
    {
        var url = Config["DatabaseUrl"] ?? "http://127.0.0.1:8080";

        var store = new DocumentStore { Urls = [url], Database = database };

        var certPath = Config["DatabaseCertificate"];
        if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
        {
            store.Certificate = System.Security.Cryptography.X509Certificates.X509CertificateLoader
                .LoadPkcs12FromFile(certPath, password: null);
        }

        store.Initialize();
        return store;
    }

    private async Task WaitForIndexingAsync(TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow.Add(timeout ?? TimeSpan.FromSeconds(30));
        while (DateTime.UtcNow < deadline)
        {
            var stats = await Store.Maintenance.SendAsync(
                new Raven.Client.Documents.Operations.GetStatisticsOperation());
            if (!stats.StaleIndexes.Any())
                return;
            await Task.Delay(250);
        }
    }
}
