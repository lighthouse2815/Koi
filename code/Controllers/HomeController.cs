using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;

namespace Koi.Web.Controllers;

public class HomeController(SiteContentService site) : Controller
{
    [HttpPost]
    public IActionResult Language(string culture, string? returnUrl)
    {
        if (culture is not ("vi" or "en")) return BadRequest();
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax });
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action(nameof(Index))!);
    }

    public IActionResult Cover()
    {
        return View();
    }

    public async Task<IActionResult> Index() => View(await site.GetHomeAsync());

    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> OurTea() => View(await site.GetOurTeaAsync());

    public IActionResult Recruit()
    {
        return View();
    }

    public async Task<IActionResult> News(string? category, int page = 1) => View(await site.GetNewsAsync(category, page));

    public async Task<IActionResult> NewsDetail(int id)
    {
        var article = await site.GetArticleAsync(id, publishedOnly: true);
        return article == null ? NotFound() : View(article);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewsPreview(int id)
    {
        var article = await site.GetArticleAsync(id, publishedOnly: false);
        if (article == null) return NotFound();
        ViewData["IsAdminPreview"] = true;
        return View("NewsDetail", article);
    }

    public async Task<IActionResult> Store(int c = 0, int s = 0, int b = 0, int page = 1) => View(await site.GetStoresAsync(c, s, b, page));

    public IActionResult Impact()
    {
        return View();
    }

    public IActionResult Responsibility() => View();
    public IActionResult Precision() => View();
    public IActionResult People() => View();
    public IActionResult Connections() => View();
    public IActionResult Contact() => View();
    public async Task<IActionResult> Menu() => View(await site.GetMenusAsync());

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
