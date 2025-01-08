using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using NLog;
using NLog.Web;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace Fabsenet.EdiEnergyViewer;

public class Program
{
    public static void Main(string[] args)
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

            if (!string.IsNullOrEmpty(configuration["DatabaseCertificate"]))
            {
                var certificateFilePath = configuration["DatabaseCertificate"];
                if (!File.Exists(certificateFilePath)) throw new Exception($"certificate files does not exist: {certificateFilePath}");
                store.Certificate = X509CertificateLoader.LoadCertificateFromFile(certificateFilePath);
            }

            store.Initialize();

            log.Debug($"Creating RavenDB indexe");
            IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store);

            return store;
        });

        builder.Services.AddCors();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(c => c.AllowAnyOrigin());
        }

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

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
