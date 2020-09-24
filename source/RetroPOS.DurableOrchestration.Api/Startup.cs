using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RetroPOS.DurableOrchestration.Api;
using System;

[assembly: FunctionsStartup(typeof(Startup))]
namespace RetroPOS.DurableOrchestration.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            Settings.DAPR_HTTP_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT"))) ? "5100" : Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            Settings.DAPR_GRPC_PORT = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT"))) ? "5200" : Environment.GetEnvironmentVariable("DAPR_GRPC_PORT");

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AzureWebJobsStorage")))
                throw new ArgumentNullException("AzureWebJobsStorage");

            Settings.DURABLE_BINDING_COMPONENT_NAME = (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DurableBindingComponentName"))) ? throw new ArgumentNullException("DurableBindingComponentName") : Environment.GetEnvironmentVariable("DurableBindingComponentName");

            Settings.MAX_WORKLOADS = 5;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MaxWorkloads")))
            {
                if (!int.TryParse(Environment.GetEnvironmentVariable("MaxWorkloads"), out Settings.MAX_WORKLOADS))
                {
                    Settings.MAX_WORKLOADS = 5;
                }
            }

            Settings.MAX_REQUESTS_PER_WORKLOAD = 10;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MaxRequestsPerWorkload")))
            {
                if (!int.TryParse(Environment.GetEnvironmentVariable("MaxRequestsPerWorkload"), out Settings.MAX_REQUESTS_PER_WORKLOAD))
                {
                    Settings.MAX_REQUESTS_PER_WORKLOAD = 10;
                }
            }
        }
    }
}
