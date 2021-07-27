using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Play.Common.MassTransit;
using Play.Common.MongoDb;
using Play.Inventory.Service.Entities;
using Play.Invertory.Service.Clients;
using Polly;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace Play.Inventory.Service
{
    public class Startup
    {
        private const string AllowedOriginSetting = "AllowedOrigin";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo()
                    .AddMongoRepository<InventoryItem>("inventoryitems")
                    .AddMongoRepository<CatalogItem>("catalogitems")
                    .AddMassTransitWithRabbitMq();

            HttpClientConfiguration(services);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));

                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[AllowedOriginSetting])
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /*Other way how services can comunicate (via httpClient) CatalogClient*/
        private static void HttpClientConfiguration(IServiceCollection services)
        {
            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44352/");
            })
                        .AddTransientHttpErrorPolicy(builder =>
                            builder.Or<TimeoutRejectedException>()
                                    .WaitAndRetryAsync(
                                        5,
                                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                    /*onRetry: (outcome, timeSpan, retryAttempt) =>
                                    {
                                        var serviceProvider = services.BuildServiceProvider();
                                        serviceProvider.GetService<ILogger<CatalogClient>>().LogWarning($"Delaying for {timeSpan.TotalSeconds} seconds, then making retry {retryAttempt}");
                                    }*/
                                    )
                        ) /*Policy for handling errors*/
                        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>()
                                                                        .CircuitBreakerAsync(
                                                                            3,
                                                                            TimeSpan.FromSeconds(15)
                                                                        /*onBreak: (outcome, timespan) =>
                                                                        {
                                                                            var serviceProvider = services.BuildServiceProvider();
                                                                            serviceProvider.GetService<ILogger<CatalogClient>>().LogWarning($"Delaying for {timeSpan.TotalSeconds} seconds, then making retry {retryAttempt}");
                                                                        }*/
                                                                        ))
                        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
        }
    }
}
