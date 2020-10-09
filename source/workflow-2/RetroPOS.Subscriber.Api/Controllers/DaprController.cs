using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace RetroPOS.Subscriber.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DaprController : ControllerBase
    {
        private readonly ILogger<DaprController> logger;

        public DaprController(ILogger<DaprController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("subscribe")]
        public ActionResult<IEnumerable<string>> Subscribe()
        {
            return new OkObjectResult(new[]{
                new { pubsubname = Settings.PUBSUB_COMPONENT_NAME, topic = Settings.PUBSUB_TOPIC_NAME, route = "/api/listener/processevent"}
            });
        }
    }
}