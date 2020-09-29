using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RetroPOS.ExposedService.WorkserService.Services;
using System.Threading;
using System.Threading.Tasks;

namespace RetroPOS.ExposedService.WorkserService
{
    public class Worker : BackgroundService
    {
        private readonly ILauncherService launcherSvc;
        private readonly ILogger<Worker> logger;

        public Worker(ILauncherService launcherSvc, ILogger<Worker> logger)
        {
            this.launcherSvc = launcherSvc;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => { launcherSvc.SendRequests(Program.ADDRESS, Program.MAX_REQUESTS, stoppingToken); });
        }
    }
}
