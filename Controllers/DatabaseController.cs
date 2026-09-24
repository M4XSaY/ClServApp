using ClServApp.Server.Services;
using ClServApp.Server.Security;
using Microsoft.AspNetCore.Mvc;

namespace ClServApp.Server.Controllers
{
    [ApiController]
    [Route("api/database")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public DatabaseController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet("test")]
        [AppRoleAuthorize(AppRoles.Administrator)]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool result = await _databaseService.TestConnectionAsync();

                return Ok(new
                {
                    success = result,
                    message = "Подключение к PostgreSQL успешно."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка подключения к PostgreSQL.",
                    error = ex.Message
                });
            }
        }
    }
}