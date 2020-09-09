using Newtonsoft.Json;

namespace RetroPOS.App.Models
{
    public class ProductDeletionRequest
    {
        [JsonProperty("warehouseID")]
        public string WarehouseID { get; set; }

        [JsonProperty("productID")]
        public string ProductID { get; set; }
    }
}
