using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using RetroPOS.ExposedService.Api.Services;
using System;
using System.Net.Http;

namespace RetroPOS.ExposedService.Api
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
            Settings.DAPR_HTTP_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT"))) ? "5100" : Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            Settings.DAPR_GRPC_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT"))) ? "5200" : Environment.GetEnvironmentVariable("DAPR_GRPC_PORT");

            Settings.OUTPUT_BINDING_COMPONENT_NAME = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OutputBindingComponentName"))) ? throw new ArgumentNullException("OutputBindingComponentName") : Environment.GetEnvironmentVariable("OutputBindingComponentName");

            services.AddControllers();
            services.AddHttpClient<IQueueService, QueueService>()
                .AddPolicyHandler(GetTimeoutPolicy())
                .SetHandlerLifetime(TimeSpan.FromMinutes(30));
        }

        IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(60));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
