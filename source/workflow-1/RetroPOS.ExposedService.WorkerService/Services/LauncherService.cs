using Microsoft.Extensions.Logging;
using RetroPOS.ExposedService.WorkserService.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RetroPOS.ExposedService.WorkerService.Services
{
    public class LauncherService : ILauncherService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<LauncherService> logger;

        public LauncherService(HttpClient httpClient, ILogger<LauncherService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public void SendRequests(string address, int maxRequests, CancellationToken stoppingToken)
        {
            List<Exception> exceptions = new List<Exception>();
            int success = 0;
            int failures = 0;
            bool flagCancelled = false;


            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = stoppingToken };

            if (Parallel.For(0, maxRequests, parallelOptions, (i, loopState) =>
            {
                try
                {
                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"http://{address}:5000/api/queue/queuemessage");
                    HttpResponseMessage httpResponseMessage = httpClient.SendAsync(httpRequestMessage).Result;

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        success++;
                        logger.LogInformation($"OK! - Success: {success} Failures: {failures}");
                    }
                    else
                    {
                        failures++;
                        logger.LogError($"OOPSS! - Success: {success} Failures: {failures}");
                    }

                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    if ((parallelOptions.CancellationToken.IsCancellationRequested) && (!flagCancelled))
                    {
                        loopState.Stop();
                        flagCancelled = true;
                        FinishProcess(success, failures, exceptions);
                    }
                }
                catch (Exception ex)
                {
                    failures++;
                    logger.LogError("Exception handled, process should continue.");
                    exceptions.Add(ex);
                }
            }).IsCompleted)
            {
                if (!flagCancelled)
                {
                    FinishProcess(success, failures, exceptions);
                }
            }
        }

        void FinishProcess(int success, int failures, List<Exception> exceptions)
        {
            logger.LogInformation("Completed!");
            logger.LogInformation($"Success: {success}");
            logger.LogInformation($"Failures: {failures}");

            TextWriter tw0 = new StreamWriter(@"C:\Temp\Exceptions.txt");

            foreach (Exception ex in exceptions)
            {
                tw0.WriteLine(ex.Message);
                tw0.WriteLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    tw0.WriteLine(ex.InnerException.Message);
                    if (ex.InnerException.StackTrace != null)
                        tw0.WriteLine(ex.InnerException.StackTrace);
                }
            }

            tw0.Close();

            TextWriter tw1 = new StreamWriter(@"C:\Temp\Summary.txt");

            tw1.WriteLine($"Success: {success}");
            tw1.WriteLine($"Failures: {failures}");

            tw1.Close();
        }
    }
}
