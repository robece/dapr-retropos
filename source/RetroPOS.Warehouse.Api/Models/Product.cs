using Newtonsoft.Json;

namespace RetroPOS.Warehouse.Api.Models
{
    public class Product
    {
        [JsonProperty("warehouseID")]
        public string WarehouseID { get; set; }

        [JsonProperty("productID")]
        public string ProductID { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("productQuantity")]
        public string ProductQuantity { get; set; }

        [JsonProperty("productDescription")]
        public string ProductDescription { get; set; }
    }
}
