using Newtonsoft.Json;

namespace RetroPOS.Warehouse.Api.Models
{
    public class ProductDeletionRequest
    {
        [JsonProperty("warehouseID")]
        public string WarehouseID { get; set; }

        [JsonProperty("productID")]
        public string ProductID { get; set; }
    }
}
