using System.Threading.Tasks;

namespace RetroPOS.ExposedService.Api.Services
{
    public interface IQueueService
    {
        Task<bool> QueueMessageAsync();
    }
}