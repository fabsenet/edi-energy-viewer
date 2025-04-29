using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.StaticFiles;
using NLog;
using NLog.Web;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace Fabsenet.EdiEnergyViewer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var log = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "development"
        });

        builder.Configuration.AddEnvironmentVariables("EDIENERGYVIEWER_");
        // Add services to the container.

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

        builder.Host.UseNLog();
        builder.Logging.AddNLogWeb();

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi("v1");

        builder.Services.AddSingleton<IDocumentStore>(provider =>
        {
            log.Debug($"Creating DocumentStore");

            var configuration = builder.Configuration;

            var store = new DocumentStore()
            {
                Urls = [configuration["DatabaseUrl"]],
                Database = configuration["DatabaseName"]
            };

            var certPath = configuration["DatabaseCertificate"];
            if (!string.IsNullOrEmpty(certPath))
            {
                if (!File.Exists(certPath)) throw new Exception($"certificate files does not exist: {certPath}");

                var limits = new Pkcs12LoaderLimits { PreserveStorageProvider = true };
                store.Certificate = X509CertificateLoader.LoadPkcs12(File.ReadAllBytes(certPath), null, X509KeyStorageFlags.MachineKeySet, limits);
            }

            store.Initialize();

            log.Debug($"Creating RavenDB indexe");
            IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store);

            return store;
        });

        builder.Services.AddCors();
        builder.Services.AddResponseCaching();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(c => c.AllowAnyOrigin());
        }
        app.UseResponseCaching();
        app.UseDefaultFiles();
        app.MapStaticAssets();

        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "v1");
        });
        app.UseDeveloperExceptionPage();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("/index.html", new StaticFileOptions() { OnPrepareResponse = SetCacheHeaderForAppIndex });

        await app.RunAsync();
    }

    private static void SetCacheHeaderForAppIndex(StaticFileResponseContext context)
    {
        context.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
    }
}
