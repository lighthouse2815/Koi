using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Koi.Web.Middleware;

public class AppExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ErrorCode? error = null;
        try
        {
            await next(context);
        }
        catch (Exception exception) when (
            !context.Response.HasStarted && (error = GetError(exception)) != null)
        {
            context.Response.StatusCode = error!.StatusCode;
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                await context.Response.WriteAsJsonAsync(new { message = error.Message });
                return;
            }
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync(error.Message);
        }
    }

    private static ErrorCode? GetError(Exception exception) => exception switch
    {
        AppException appException => appException.Error,
        DbUpdateConcurrencyException => ErrorCode.DATA_CONFLICT,
        DbUpdateException
        {
            InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } postgres
        } databaseException when postgres.ConstraintName == "FK_Drinks_Category_CategoryId"
            && databaseException.Entries.Any(entry => entry.State == EntityState.Deleted && entry.Entity is Category)
            => ErrorCode.CATEGORY_HAS_DRINKS,
        DbUpdateException
        {
            InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } postgres
        } databaseException when postgres.ConstraintName == "FK_Stores_StoreCities_CityId"
            && databaseException.Entries.Any(entry => entry.State == EntityState.Deleted && entry.Entity is StoreCity)
            => ErrorCode.STORE_CITY_HAS_STORES,
        _ => null
    };
}
