using ClServApp.Server.Models;
using ClServApp.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClServApp.Server.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public EmployeesController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }


        // ==========================================
        // Получить всех сотрудников
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees =
                    await _databaseService.GetEmployeesAsync();

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ошибка получения сотрудников.",
                        error = ex.Message
                    });
            }
        }


        // ==========================================
        // Получить должности
        // ==========================================

        [HttpGet("positions")]
        public async Task<IActionResult> GetPositions()
        {
            try
            {
                var positions =
                    await _databaseService.GetPositionsAsync();

                return Ok(
                    positions.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name
                    })
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ошибка получения должностей.",
                        error = ex.Message
                    });
            }
        }


        // ==========================================
        // Получить отделы
        // ==========================================

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments =
                    await _databaseService.GetDepartmentsAsync();

                return Ok(
                    departments.Select(d => new
                    {
                        id = d.Id,
                        name = d.Name
                    })
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ошибка получения отделов.",
                        error = ex.Message
                    });
            }
        }


        // ==========================================
        // Добавить сотрудника
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> AddEmployee(
            [FromBody] EmployeeCreate employee)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employee.Name))
                {
                    return BadRequest(new
                    {
                        message = "Введите ФИО сотрудника."
                    });
                }

                if (employee.PositionId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Выберите должность."
                    });
                }

                if (employee.DepartmentId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Выберите отдел."
                    });
                }

                if (employee.Salary < 0)
                {
                    return BadRequest(new
                    {
                        message = "Зарплата не может быть отрицательной."
                    });
                }

                await _databaseService.AddEmployeeAsync(employee);

                return Ok(new
                {
                    message = "Сотрудник успешно добавлен."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ошибка добавления сотрудника.",
                        error = ex.Message
                    });
            }
        }


        // ==========================================
        // Изменить сотрудника
        // ==========================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(
            int id,
            [FromBody] EmployeeCreate employee)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Некорректный ID сотрудника."
                    });
                }

                if (string.IsNullOrWhiteSpace(employee.Name))
                {
                    return BadRequest(new
                    {
                        message = "Введите ФИО сотрудника."
                    });
                }

                if (employee.PositionId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Выберите должность."
                    });
                }

                if (employee.DepartmentId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Выберите отдел."
                    });
                }

                if (employee.Salary < 0)
                {
                    return BadRequest(new
                    {
                        message = "Зарплата не может быть отрицательной."
                    });
                }

                await _databaseService.UpdateEmployeeAsync(
                    id,
                    employee);

                return Ok(new
                {
                    message = "Сотрудник успешно изменён."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ошибка изменения сотрудника.",
                        error = ex.Message
                    });
            }
        }


        // ==========================================
        // Удалить сотрудника
        // ==========================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Некорректный ID сотрудника."
                    });
                }

                await _databaseService.DeleteEmployeeAsync(id);

                return Ok(new
                {
                    message = "Сотрудник успешно удалён."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "Не удалось удалить сотрудника.",
                        error = ex.Message
                    });
            }
        }
    }
}