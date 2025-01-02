using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace EdiEnergyViewerCore
{
    public class Startup
    {
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var log = _loggerFactory.CreateLogger<Startup>();
            try
            {
                log.LogTrace($"ConfigureServices() started.");
                services.AddControllersWithViews().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

                services.AddLogging();

                log.LogDebug($"Creating DocumentStore");

                var store = new DocumentStore()
                {
                    Urls = new[] { Configuration["DatabaseUrl"] },
                    Database = Configuration["DatabaseName"]
                };

                if (!string.IsNullOrEmpty(Configuration["DatabaseCertificate"]))
                {
                    string certificateFilePath = Configuration["EdiDocsDatabaseCertificate"];
                    if (!File.Exists(certificateFilePath)) throw new Exception($"certificate files does not exist: {certificateFilePath}");
                    store.Certificate = new X509Certificate2(certificateFilePath);
                }

                store.Initialize();

                log.LogDebug($"Creating RavenDB indexe");
                IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store);
                services.AddSingleton<IDocumentStore>(store);

                log.LogTrace($"ConfigureServices() ended.");
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, $"ConfigureServices failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();
            app.UseStatusCodePages();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
