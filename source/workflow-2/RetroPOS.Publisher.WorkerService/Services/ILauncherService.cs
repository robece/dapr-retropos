using System.Threading;

namespace RetroPOS.Publisher.WorkerService.Services
{
    public interface ILauncherService
    {
        void SendRequests(string address, int maxRequests, CancellationToken stoppingToken);
    }
}
