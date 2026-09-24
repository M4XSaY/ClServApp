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
                return Ok(await _databaseService.GetStockAsync());
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
                return Ok(await _databaseService.GetLowStockAsync());
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

        [HttpGet("warehouses")]
        public async Task<IActionResult> GetWarehouses()
        {
            try
            {
                var warehouses = await _databaseService.GetWarehousesAsync();
                return Ok(warehouses.Select(w => new { id = w.Id, name = w.Name }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении списка складов.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] StockCreate item)
        {
            try
            {
                var id = await _databaseService.AddStockAsync(item);
                return Ok(new { message = "Складская позиция успешно добавлена.", id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при добавлении складской позиции.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] StockCreate item)
        {
            try
            {
                await _databaseService.UpdateStockAsync(id, item);
                return Ok(new { message = "Складская позиция успешно изменена." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при изменении складской позиции.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            try
            {
                await _databaseService.DeleteStockAsync(id);
                return Ok(new { message = "Складская позиция успешно удалена." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при удалении складской позиции.",
                    error = ex.Message
                });
            }
        }
    }
}
