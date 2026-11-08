using Koi.Web.Exceptions;

namespace Koi.Web.Middleware;

public class AppExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException exception) when (!context.Response.HasStarted)
        {
            context.Response.StatusCode = exception.Error.StatusCode;
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                await context.Response.WriteAsJsonAsync(new { message = exception.Error.Message });
                return;
            }
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync(exception.Error.Message);
        }
    }
}
