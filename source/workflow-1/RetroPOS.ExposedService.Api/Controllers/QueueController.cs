using Microsoft.AspNetCore.Mvc;
using RetroPOS.ExposedService.Api.Services;
using System.Threading.Tasks;

namespace RetroPOS.ExposedService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService exposedSvc;

        public QueueController(IQueueService exposedSvc)
        {
            this.exposedSvc = exposedSvc;
        }

        [HttpPost("queuemessage")]
        public async Task<IActionResult> QueueMessage()
        {
            var result = await exposedSvc.QueueMessageAsync();

            ActionResult actionResult = (result) ? StatusCode(200) : StatusCode(500);
            return actionResult;
        }
    }
}