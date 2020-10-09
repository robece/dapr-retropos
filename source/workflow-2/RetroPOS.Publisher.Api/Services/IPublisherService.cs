using RetroPOS.Publisher.Api.Models;
using System.Threading.Tasks;

namespace RetroPOS.Publisher.Api.Services
{
    public interface IPublisherService
    {
        Task<bool> NotifySubscriptors(InternalMessage message);
    }
}