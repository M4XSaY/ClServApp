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
                var details =
                    await _databaseService.GetOrderDetailsAsync(id);

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

        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody] OrderCreate order)
        {
            try
            {
                await _databaseService.AddOrderAsync(order);

                return Ok(new
                {
                    message = "Заказ успешно создан."
                });
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

        [HttpGet("{id}/edit")]
        public async Task<ActionResult<OrderEdit>> GetOrderForEdit(int id)
        {
            try
            {
                var order =
                    await _databaseService.GetOrderForEditAsync(id);

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении заказа для редактирования.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(
    int id,
    [FromBody] OrderEdit order)
        {
            try
            {
                await _databaseService.UpdateOrderAsync(id, order);

                return Ok(new
                {
                    message = "Заказ успешно изменён."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при изменении заказа.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await _databaseService.DeleteOrderAsync(id);

                return Ok(new
                {
                    message = "Заказ успешно удалён."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Ошибка при удалении заказа.",
                    error = ex.Message
                });
            }
        }
    }
}