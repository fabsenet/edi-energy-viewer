using NLog.Web;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace EdiEnergyViewer.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var log = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Trace);

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
                    var certificateFilePath = configuration["EdiDocsDatabaseCertificate"];
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

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "v1");
                });
                app.UseCors(c => c.AllowAnyOrigin());
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
