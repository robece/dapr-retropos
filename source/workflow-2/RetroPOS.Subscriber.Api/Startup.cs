using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using RetroPOS.Subscriber.Api.Services;
using System;
using System.Net.Http;

namespace RetroPOS.Subscriber.Api
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
            Settings.DAPR_HTTP_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT"))) ? "6100" : Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            Settings.DAPR_GRPC_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT"))) ? "6200" : Environment.GetEnvironmentVariable("DAPR_GRPC_PORT");

            Settings.PUBSUB_COMPONENT_NAME = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PubSubComponentName"))) ? throw new ArgumentNullException("PubSubComponentName") : Environment.GetEnvironmentVariable("PubSubComponentName");
            Settings.PUBSUB_TOPIC_NAME = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PubSubTopicName"))) ? throw new ArgumentNullException("PubSubTopicName") : Environment.GetEnvironmentVariable("PubSubTopicName");
            Settings.OUTPUT_BINDING_COMPONENT_NAME = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OutputBindingComponentName"))) ? throw new ArgumentNullException("OutputBindingComponentName") : Environment.GetEnvironmentVariable("OutputBindingComponentName");

            services.AddControllers();
            services.AddHttpClient<ISubscriberService, SubscriberService>()
                .AddPolicyHandler(GetTimeoutPolicy())
                .SetHandlerLifetime(TimeSpan.FromMinutes(30));

            services.AddMvc(opts =>
            {
                opts.InputFormatters.Insert(0, new CloudNative.CloudEvents.CloudEventJsonInputFormatter());
            });
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
