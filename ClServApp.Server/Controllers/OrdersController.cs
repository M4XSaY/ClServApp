using ClServApp.Server.Models;
using ClServApp.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClServApp.Server.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public OrdersController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            try
            {
                var orders = await _databaseService.GetOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении заказов.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<List<OrderDetail>>> GetOrderDetails(int id)
        {
            try
            {
                var details = await _databaseService.GetOrderDetailsAsync(id);
                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении деталей заказа.",
                    error = ex.Message
                });
            }
        }

        // ==========================================
        // Добавленные методы для создания заказов
        // ==========================================

        [HttpGet("form-data")]
        public async Task<IActionResult> GetFormData()
        {
            try
            {
                var clients = await _databaseService.GetClientsAsync();
                var products = await _databaseService.GetProductsAsync();
                return Ok(new { clients, products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при загрузке данных для формы.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto dto)
        {
            try
            {
                await _databaseService.CreateOrderAsync(dto);
                return Ok(new { message = "Заказ успешно создан" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Ошибка при создании заказа.",
                    error = ex.Message
                });
            }
        }
    }
}