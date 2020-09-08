using Newtonsoft.Json;

namespace RetroPOS.Audit.Api.Models
{
    public class RegistrationRequest
    {
        [JsonProperty("productID")]
        public string ProductID { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("productDescription")]
        public string ProductDescription { get; set; }

        [JsonProperty("productWarehouse")]
        public string ProductWarehouse { get; set; }
    }
}
