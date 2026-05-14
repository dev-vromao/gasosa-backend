using gasosa_backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace gasosa_backend.Filters
{
    /// <summary>
    /// Bloqueia a ação se o usuário autenticado estiver com Banido = true.
    /// Retorna 403 com { banido = true, message = ... }.
    /// Deve ser usado em conjunto com [Authorize].
    /// </summary>
    public class BloqueiaBanidoAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var userManager = context.HttpContext.RequestServices
                .GetRequiredService<UserManager<Usuario>>();

            var usuario = await userManager.GetUserAsync(context.HttpContext.User);

            if (usuario != null && usuario.Banido)
            {
                context.Result = new ObjectResult(new
                {
                    banido = true,
                    message = "Sua conta foi banida porque suas avaliações receberam muitos dislikes da comunidade. Você ainda pode visualizar postos e avaliações, mas não pode publicar nem interagir."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}
