using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RetroPOS.ExposedService.WorkerService.Services;
using RetroPOS.ExposedService.WorkserService.Services;
using System;
using System.Net.Http;

namespace RetroPOS.ExposedService.WorkserService
{
    public class Program
    {
        public static string ADDRESS = string.Empty;
        public static int MAX_REQUESTS = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("Service IP Address:");
            ADDRESS = Console.ReadLine();

            Console.WriteLine("Max Requests:");
            MAX_REQUESTS = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Starting");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddDebug();
                    logging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddHttpClient<ILauncherService, LauncherService>()
                    .AddPolicyHandler(GetTimeoutPolicy())
                    .SetHandlerLifetime(TimeSpan.FromMinutes(30));
                });

        static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(80));
        }
    }
}
