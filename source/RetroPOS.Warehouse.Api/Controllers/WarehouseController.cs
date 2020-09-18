using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RetroPOS.Warehouse.Api.Models;
using RetroPOS.Warehouse.Api.Services;
using System.Threading.Tasks;

namespace RetroPOS.Warehouse.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService warehouseSvc;
        private readonly ILogger<WarehouseController> logger;

        public WarehouseController(IWarehouseService warehouseSvc, ILogger<WarehouseController> logger)
        {
            this.warehouseSvc = warehouseSvc;
            this.logger = logger;
        }

        [HttpPost("productupdateregistration")]
        public async Task<IActionResult> ProductUpdateRegistration([FromBody] ProductUpdateRegistrationRequest request)
        {
            var result = await warehouseSvc.ProductUpdateRegistration(request);

            if (result)
                await warehouseSvc.NotifySubscriptors(request);

            return Ok(result);
        }

        [HttpPost("productdeletion")]
        public async Task<IActionResult> ProductDeletion([FromBody] ProductDeletionRequest request)
        {
            var result = await warehouseSvc.ProductDeletion(request);
            return Ok(result);
        }

        [HttpPost("warehouseproducts")]
        public async Task<IActionResult> WarehouseProducts([FromBody] ProductsRequest request)
        {
            var result = await warehouseSvc.WarehouseProducts(request);
            return Ok(result.Products);
        }
    }
}
