using Newtonsoft.Json;
using RetroPOS.Audit.Api.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetroPOS.Audit.Api.Services
{
    public class AuditService : IAuditService
    {
        private readonly HttpClient httpClient;

        public AuditService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> GenerateAuditFile(ProductUpdateRegistrationRequest request)
        {
            bool result = false;

            var metadata = new Dictionary<string, string>();
            metadata.Add("blobName", $"{request.ProductID}.json");
            metadata.Add("ContentType", "application/json");

            var payload = new
            {
                data = JsonConvert.SerializeObject(request),
                metadata = metadata,
                operation = "create"
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Settings.DAPR_SIDECAR_BASEURL}/v1.0/bindings/{Settings.AUDIT_BINDING_COMPONENT_NAME}", stringContent);

            if (response.IsSuccessStatusCode)
                result = true;

            return result;
        }
    }
}
