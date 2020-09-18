using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RetroPOS.DurableOrchestration.Api;

[assembly: FunctionsStartup(typeof(Startup))]
namespace RetroPOS.DurableOrchestration.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
        }
    }
}
