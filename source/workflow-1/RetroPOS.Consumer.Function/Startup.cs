using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RetroPOS.Consumer.Function;
using RetroPOS.Consumer.Function.Services;
using System;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]
namespace RetroPOS.Consumer.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Settings.DAPR_HTTP_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT"))) ? "6100" : Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            Settings.DAPR_GRPC_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT"))) ? "6200" : Environment.GetEnvironmentVariable("DAPR_GRPC_PORT");

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AzureWebJobsStorage")))
                throw new ArgumentNullException("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ServiceBusConnectionString")))
                throw new ArgumentNullException("ServiceBusConnectionString");

            Settings.CONSUMER_STATE_COMPONENT_NAME = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConsumerStateComponentName"))) ? throw new ArgumentNullException("ConsumerStateComponentName") : Environment.GetEnvironmentVariable("ConsumerStateComponentName");

            builder.Services.AddHttpClient<IConsumerService, ConsumerService>()
                .AddPolicyHandler(GetTimeoutPolicy())
                .SetHandlerLifetime(TimeSpan.FromMinutes(30));
        }

        IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(60));
        }
    }
}
