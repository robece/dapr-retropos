using RetroPOS.Subscriber.Api.Models;
using System.Threading.Tasks;

namespace RetroPOS.Subscriber.Api.Services
{
    public interface ISubscriberService
    {
        Task<bool> SendToOutputBinding(InternalMessage message);
    }
}