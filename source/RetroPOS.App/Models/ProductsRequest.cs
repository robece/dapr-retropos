using Newtonsoft.Json;

namespace RetroPOS.App.Models
{
    public class ProductsRequest
    {
        [JsonProperty("warehouseID")]
        public string WarehouseID { get; set; }
    }
}
