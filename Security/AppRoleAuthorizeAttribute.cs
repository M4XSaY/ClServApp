using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClServApp.Server.Security
{
    public static class AppRoles
    {
        public const string Administrator = "administrator";
        public const string Sales = "sales";
        public const string Warehouse = "warehouse";
        public const string Supervisor = "supervisor";
    }

    /// <summary>
    /// Учебная проверка роли. Роль передается клиентом в HTTP-заголовке X-App-Role.
    /// Это не заменяет полноценную аутентификацию ASP.NET Core Identity/JWT,
    /// но позволяет продемонстрировать ролевое разграничение доступа в курсовом проекте.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AppRoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly HashSet<string> _allowedRoles;

        public AppRoleAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = new HashSet<string>(
                allowedRoles ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Request.Headers["X-App-Role"]
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(role))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Роль пользователя не выбрана. Выполните вход в систему."
                });
                return;
            }

            if (!_allowedRoles.Contains(role))
            {
                context.Result = new ObjectResult(new
                {
                    message = "Для выбранной роли эта операция недоступна."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
