using Microsoft.AspNetCore.Mvc;
using RetroPOS.Publisher.Api.Models;
using RetroPOS.Publisher.Api.Services;
using System;
using System.Threading.Tasks;

namespace RetroPOS.Publisher.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IPublisherService publisherSvc;

        public NotificationController(IPublisherService publisherSvc)
        {
            this.publisherSvc = publisherSvc;
        }

        [HttpPost("notify")]
        public async Task<IActionResult> Notify()
        {
            InternalMessage message = new InternalMessage();
            message.MessageId = Guid.NewGuid().ToString();
            message.Description = $"Message {message.MessageId} description";

            var result = await publisherSvc.NotifySubscriptors(message);

            ActionResult actionResult = (result) ? StatusCode(200) : StatusCode(500);
            return actionResult;
        }
    }
}