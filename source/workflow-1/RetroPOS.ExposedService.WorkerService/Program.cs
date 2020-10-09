using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RetroPOS.ExposedService.WorkerService.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace RetroPOS.ExposedService.WorkserService
{
    public class Program
    {
        public static string ADDRESS = string.Empty;
        public static int MAX_REQUESTS = 0;

        static string IsIPAddress(string address)
        {
            string result = string.Empty;
            IPAddress iPAddress;
            bool isIP = IPAddress.TryParse(address, out iPAddress);

            if (isIP)
                result = iPAddress.ToString();

            return result;
        }

        public static void Main(string[] args)
        {
            List<string> exceptions = new List<string>();

            if (args.Length == 0)
            {
                Console.WriteLine("Service IP Address:");
                ADDRESS = IsIPAddress(Console.ReadLine());

                Console.WriteLine("Max Requests:");
                MAX_REQUESTS = Convert.ToInt32(Console.ReadLine());

                if (ADDRESS.Length == 0)
                    exceptions.Add("Invalid address");

                if (MAX_REQUESTS == 0)
                    exceptions.Add("Invalid max requests");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--address")
                    {
                        if (i < args.Length - 1)
                            if (!string.IsNullOrEmpty(args[i + 1]))
                                ADDRESS = IsIPAddress(args[i + 1]);
                    }

                    if (args[i] == "--max-requests")
                    {
                        if (i < args.Length - 1)
                            if (!string.IsNullOrEmpty(args[i + 1]))
                                int.TryParse(args[i + 1], out MAX_REQUESTS);
                    }
                }

                if (ADDRESS.Length == 0)
                    exceptions.Add("Invalid address, verify parameter --address");

                if (MAX_REQUESTS == 0)
                    exceptions.Add("Invalid max requests, verify parameter --max-requests");
            }

            if (exceptions.Count == 0)
            {
                Console.WriteLine($"Starting with ADDRESS: {ADDRESS} and MAX REQUESTS: {MAX_REQUESTS}");
                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                Console.WriteLine("Exceptions:");
                foreach (string s in exceptions)
                    Console.WriteLine($"- {s}");
            }
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
