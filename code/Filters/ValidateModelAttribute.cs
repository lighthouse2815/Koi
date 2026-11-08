using Koi.Web.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Koi.Web.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var message = context.ModelState.Values.SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            ?? ErrorCode.INVALID_INPUT.Message;
        throw new AppException(ErrorCode.INVALID_INPUT with { Message = message });
    }
}
