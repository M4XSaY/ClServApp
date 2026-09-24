using ClServApp.Server.Models;
using ClServApp.Server.Services;
using ClServApp.Server.Security;
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

        // GET: api/clients
        [HttpGet]
        [AppRoleAuthorize(AppRoles.Administrator, AppRoles.Sales, AppRoles.Supervisor)]
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

        // GET: api/clients/{id}
        [HttpGet("{id:int}")]
        [AppRoleAuthorize(AppRoles.Administrator, AppRoles.Sales, AppRoles.Supervisor)]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Некорректный ID клиента."
                });
            }

            try
            {
                var client = await _databaseService.GetClientAsync(id);

                if (client == null)
                {
                    return NotFound(new
                    {
                        message = "Клиент не найден."
                    });
                }

                return Ok(client);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении клиента.",
                    error = ex.Message
                });
            }
        }

        // POST: api/clients
        [HttpPost]
        [AppRoleAuthorize(AppRoles.Administrator, AppRoles.Sales)]
        public async Task<IActionResult> AddClient(
            [FromBody] ClientCreate client)
        {
            if (client == null)
            {
                return BadRequest(new
                {
                    message = "Данные клиента не переданы."
                });
            }

            var validationError = ValidateClient(client);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message = validationError
                });
            }

            try
            {
                var clientId =
                    await _databaseService.AddClientAsync(client);

                return CreatedAtAction(
                    nameof(GetClient),
                    new { id = clientId },
                    new
                    {
                        message = "Клиент успешно добавлен.",
                        id = clientId
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

        // PUT: api/clients/{id}
        [HttpPut("{id:int}")]
        [AppRoleAuthorize(AppRoles.Administrator, AppRoles.Sales)]
        public async Task<IActionResult> UpdateClient(
            int id,
            [FromBody] ClientCreate client)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Некорректный ID клиента." });
            }

            if (client == null)
            {
                return BadRequest(new { message = "Данные клиента не переданы." });
            }

            var validationError = ValidateClient(client);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            try
            {
                await _databaseService.UpdateClientAsync(id, client);
                return Ok(new { message = "Клиент успешно изменён." });
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
                    message = "Ошибка при изменении клиента.",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/clients/{id}
        [HttpDelete("{id:int}")]
        [AppRoleAuthorize(AppRoles.Administrator, AppRoles.Sales)]
        public async Task<IActionResult> DeleteClient(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Некорректный ID клиента."
                });
            }

            try
            {
                await _databaseService.DeleteClientAsync(id);

                return Ok(new
                {
                    message = "Клиент успешно удалён."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при удалении клиента.",
                    error = ex.Message
                });
            }
        }

        private static string? ValidateClient(ClientCreate client)
        {
            if (string.IsNullOrWhiteSpace(client.Name))
                return "Наименование клиента не может быть пустым.";

            if (string.IsNullOrWhiteSpace(client.Type))
                return "Необходимо указать тип клиента.";

            if (string.IsNullOrWhiteSpace(client.Phone))
                return "Телефон клиента не может быть пустым.";

            if (string.IsNullOrWhiteSpace(client.Address))
                return "Адрес доставки не может быть пустым.";

            return null;
        }
    }


}