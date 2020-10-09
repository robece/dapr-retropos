using System.Threading;

namespace RetroPOS.ExposedService.WorkerService.Services
{
    public interface ILauncherService
    {
        void SendRequests(string address, int maxRequests, CancellationToken stoppingToken);
    }
}
