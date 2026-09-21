using ClServApp.Server.Models;
using ClServApp.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClServApp.Server.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public StockController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<StockItem>>> GetStock()
        {
            try
            {
                var stock = await _databaseService.GetStockAsync();

                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении складских остатков.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("low")]
        public async Task<ActionResult<List<LowStockItem>>> GetLowStock()
        {
            try
            {
                var items = await _databaseService.GetLowStockAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении товаров для пополнения.",
                    error = ex.Message
                });
            }
        }
    }
}