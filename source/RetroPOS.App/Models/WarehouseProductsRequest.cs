using Newtonsoft.Json;

namespace RetroPOS.App.Models
{
    public class WarehouseProductsRequest
    {
        [JsonProperty("warehouseID")]
        public string WarehouseID { get; set; }
    }
}
