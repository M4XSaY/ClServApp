using ClServApp.Server.Models;
using ClServApp.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClServApp.Server.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public ClientsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Client>>> GetClients()
        {
            try
            {
                var clients = await _databaseService.GetClientsAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении клиентов.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddClient(
    [FromBody] ClientCreate client)
        {
            try
            {
                await _databaseService.AddClientAsync(client);

                return Ok(new
                {
                    message = "Клиент успешно добавлен."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при добавлении клиента.",
                    error = ex.Message
                });
            }
        }
    }
}