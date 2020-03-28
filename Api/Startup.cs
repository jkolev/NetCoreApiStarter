using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = new Settings();
            Configuration.Bind(settings);

            AddRavenDb(services, settings.Database);

            services.AddControllers();
        }

        private void AddRavenDb(IServiceCollection services, Settings.DatabaseSettings databaseSettings)
        {
            var store = new DocumentStore
            {
                Urls = databaseSettings.Urls,
                Database = databaseSettings.DatabaseName,
                Certificate = new X509Certificate2(databaseSettings.CertPath, databaseSettings.CertPass)
            }.Initialize();

            services.AddSingleton<IDocumentStore>(store);

            services.AddScoped<IAsyncDocumentSession>(serviceProvider =>
            {
                return serviceProvider
                    .GetService<IDocumentStore>()
                    .OpenAsyncSession();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
