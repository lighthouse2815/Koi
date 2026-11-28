using Koi.Web.Components.Admin;
using Koi.Web.Components.Shared;
using Koi.Web.Models;
using Koi.Web.Exceptions;
using Koi.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin")]
public class AccountController(SignInManager<IdentityUser<Guid>> signInManager) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl,
            Email = TempData["LoginEmail"] as string ?? "",
            RememberMe = TempData["LoginRememberMe"] is true
        };
        return new BlazorViewResult<LoginPage>(new { Model = model, LoginError = TempData["LoginError"] as string });
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var redirectUrl = Url.IsLocalUrl(model.ReturnUrl)
                ? model.ReturnUrl! : Url.Action("Index", "Dashboard", new
            {
                area = "Admin"
            })!;

            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                ? Json(new { redirectUrl })
                : LocalRedirect(redirectUrl);
        }
        throw new AppException(result.IsLockedOut ? ErrorCode.LOGIN_LOCKED_OUT : ErrorCode.LOGIN_INVALID_CREDENTIALS);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => new BlazorViewResult<AccessDeniedPage>()
    {
        StatusCode = StatusCodes.Status403Forbidden
    };
}
