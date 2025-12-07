using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cinema.Attributes
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");

            if (role != "Administrador")
            {
                context.Result = new RedirectToActionResult("Login", "Utilizador", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
