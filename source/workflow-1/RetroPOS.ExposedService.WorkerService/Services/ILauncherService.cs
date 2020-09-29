using System.Threading;

namespace RetroPOS.ExposedService.WorkserService.Services
{
    public interface ILauncherService
    {
        void SendRequests(string address, int maxRequests, CancellationToken stoppingToken);
    }
}
