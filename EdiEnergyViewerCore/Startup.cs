using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace EdiEnergyViewerCore
{
    public class Startup
    {
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration,
            ILoggerFactory loggerFactory)
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
                services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

                services.AddLogging();

                log.LogDebug($"Creating DocumentStore");

                var store = new DocumentStore()
                {
                    Urls = new[] { Configuration["EdiDocsDatabaseUrl"] },
                    Database = Configuration["EdiDocsDatabaseName"]
                };
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseMvc(); 
            app.UseStatusCodePages();
        }
    }
}
