using Koi.Web.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Middleware;

public class AppExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (
            !context.Response.HasStarted && (exception is AppException or DbUpdateConcurrencyException))
        {
            var error = exception is AppException appException ? appException.Error : ErrorCode.DATA_CONFLICT;
            context.Response.StatusCode = error.StatusCode;
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                await context.Response.WriteAsJsonAsync(new { message = error.Message });
                return;
            }
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync(error.Message);
        }
    }
}
