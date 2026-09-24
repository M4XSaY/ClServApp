using ClServApp.Server.Models;
using ClServApp.Server.Services;
using ClServApp.Server.Security;
using Microsoft.AspNetCore.Mvc;

namespace ClServApp.Server.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public ProductsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // ==========================================
        // Получение всех товаров
        // GET: /api/products
        // ==========================================

        [HttpGet]
        [AppRoleAuthorize(AppRoles.Administrator, AppRoles.Sales, AppRoles.Warehouse, AppRoles.Supervisor)]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            try
            {
                var products = await _databaseService.GetProductsAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ошибка при получении товаров.",
                    error = ex.Message
                });
            }
        }


        // ==========================================
        // Добавление товара
        // POST: /api/products
        // ==========================================

        [HttpPost]
        [AppRoleAuthorize(AppRoles.Administrator)]
        public async Task<IActionResult> AddProduct(
            [FromBody] Product product)
        {
            try
            {
                await _databaseService.AddProductAsync(product);

                return Ok(new
                {
                    message = "Товар успешно добавлен."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Ошибка при добавлении товара.",
                    error = ex.Message
                });
            }
        }


        // ==========================================
        // Редактирование товара
        // PUT: /api/products/{id}
        // ==========================================

        [HttpPut("{id}")]
        [AppRoleAuthorize(AppRoles.Administrator)]
        public async Task<IActionResult> UpdateProduct(
    int id,
    [FromBody] Product product)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Некорректный ID товара."
                    });
                }

                product.Id = id;

                await _databaseService.UpdateProductAsync(product);

                return Ok(new
                {
                    message = "Товар успешно изменён."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Ошибка при изменении товара.",
                    error = ex.Message
                });
            }
        }


        // ==========================================
        // Удаление товара
        // DELETE: /api/products/{id}
        // ==========================================

        [HttpDelete("{id}")]
        [AppRoleAuthorize(AppRoles.Administrator)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _databaseService.DeleteProductAsync(id);

                return Ok(new
                {
                    message = "Товар успешно удалён."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Ошибка при удалении товара.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("categories")]
        [AppRoleAuthorize(AppRoles.Administrator, AppRoles.Sales, AppRoles.Warehouse, AppRoles.Supervisor)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _databaseService.GetCategoriesAsync();

                return Ok(
                    categories.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name
                    })
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Не удалось загрузить категории.",
                    error = ex.Message
                });
            }
        }
    }
}