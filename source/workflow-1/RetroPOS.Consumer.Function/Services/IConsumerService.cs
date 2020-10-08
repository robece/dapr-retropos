using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RetroPOS.Consumer.Function.Services
{
    public interface IConsumerService
    {
        Task<bool> SendToStateAsync(string jsonPayload, ILogger logger);
    }
}
