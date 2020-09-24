using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using RetroPOS.Consumer.Api;
using System;

[assembly: FunctionsStartup(typeof(Startup))]
namespace RetroPOS.Consumer.Api
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
        }
    }
}
