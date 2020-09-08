using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace RetroPOS.Audit.Api.Controllers
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
                new { pubsubname = Settings.PRODUCTREGISTRATION_PUBSUB_COMPONENT_NAME, topic = Settings.PRODUCTREGISTRATION_PUBSUB_TOPIC_NAME, route = "/audit/registration"}
            });
        }
    }
}
