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
        }
    }
}
