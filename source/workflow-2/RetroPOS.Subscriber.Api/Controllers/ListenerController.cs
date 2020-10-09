using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RetroPOS.Subscriber.Api.Models;
using RetroPOS.Subscriber.Api.Services;
using System.Threading.Tasks;

namespace RetroPOS.Subscriber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListenerController : ControllerBase
    {
        private readonly ISubscriberService subscriberSvc;

        public ListenerController(ISubscriberService subscriberSvc)
        {
            this.subscriberSvc = subscriberSvc;
        }

        [HttpPost("processevent")]
        public async Task<IActionResult> ProcessEvent(CloudEvent cloudEvent)
        {
            var message = ((JToken)cloudEvent.Data).ToObject<InternalMessage>();
            var result = await subscriberSvc.SendToOutputBinding(message);

            ActionResult actionResult = (result) ? StatusCode(200) : StatusCode(500);
            return actionResult;
        }
    }
}