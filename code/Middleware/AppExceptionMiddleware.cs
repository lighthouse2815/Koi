using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            if (context.Request.Headers["X-Requested-With"] != "XMLHttpRequest"
                && HttpMethods.IsPost(context.Request.Method)
                && context.Request.HasFormContentType
                && context.Request.RouteValues["area"]?.ToString() == "Admin"
                && context.Request.RouteValues["controller"]?.ToString() == "Account"
                && context.Request.RouteValues["action"]?.ToString() == "Login"
                && (error == ErrorCode.LOGIN_INVALID_CREDENTIALS || error == ErrorCode.LOGIN_LOCKED_OUT
                    || error!.Code == ErrorCode.INVALID_INPUT.Code))
            {
                var form = await context.Request.ReadFormAsync();
                var tempData = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context);
                tempData["LoginError"] = error!.Message;
                var email = form["Email"].ToString();
                if (email.Length <= 256)
                    tempData["LoginEmail"] = email;
                tempData["LoginRememberMe"] = form["RememberMe"].Contains("true");
                tempData.Save();
                var returnUrl = form["ReturnUrl"].ToString();
                var query = string.IsNullOrEmpty(returnUrl) ? QueryString.Empty : QueryString.Create("returnUrl", returnUrl);
                context.Response.Redirect(context.Request.PathBase + context.Request.Path + query);
                return;
            }

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
