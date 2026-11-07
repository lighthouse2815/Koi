using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Koi.Web.Filters;

public class CategoryNameConflictFilterAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();
        if (result.Exception is AppException exception
            && exception.Error == ErrorCode.CATEGORY_NAME_EXISTS
            && context.Controller is Controller controller
            && context.ActionArguments.TryGetValue("model", out var value)
            && value is CategoryEditViewModel model)
        {
            controller.ModelState.AddModelError(nameof(model.Name), exception.Error.Message);
            context.HttpContext.Response.StatusCode = exception.Error.StatusCode;
            result.Result = controller.View("Edit", model);
            result.ExceptionHandled = true;
        }
    }
}
