using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RetroPOS.Audit.Api.Models;
using RetroPOS.Audit.Api.Services;
using System.Threading.Tasks;

namespace RetroPOS.Product.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService auditSvc;
        private readonly ILogger<AuditController> logger;

        public AuditController(IAuditService auditSvc, ILogger<AuditController> logger)
        {
            this.auditSvc = auditSvc;
            this.logger = logger;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Registration(CloudEvent cloudEvent)
        {
            var request = ((JToken)cloudEvent.Data).ToObject<RegistrationRequest>();
            var result = await auditSvc.GenerateAuditFile(request);

            return Ok(result);
        }
    }
}
