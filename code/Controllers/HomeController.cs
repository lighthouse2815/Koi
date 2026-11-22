using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace Koi.Web.Controllers;

public class HomeController(
    SiteContentService siteContent,
    NewsService newsService,
    CategoryService categoryService
) : Controller
{
    public IActionResult Cover() => RedirectToAction(nameof(Index));

    public async Task<IActionResult> Index()
    {
        var news = await newsService.GetForHomeAsync();
        var categories = await categoryService.GetListAsync(forHome: true);
        return View(new HomeViewModel(news, categories));
    }

    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> OurTea() => View(await siteContent.GetOurTeaAsync());

    public IActionResult Recruit()
    {
        return View();
    }

    public async Task<IActionResult> News(string? category, int page = 1) => View(await siteContent.GetNewsAsync(category, page));

    public async Task<IActionResult> NewsDetail(Guid id)
    {
        var article = await siteContent.GetArticleAsync(id);
        return article == null ? NotFound() : View(article);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewsPreview(Guid id)
    {
        var article = await siteContent.GetArticleAsync(id);
        if (article == null)
            return NotFound();
        ViewData["IsAdminPreview"] = true;
        return View("NewsDetail", article);
    }

    public async Task<IActionResult> Store(Guid s = default, int page = 1) => View(await siteContent.GetStoresAsync(s, page));

    public IActionResult Impact()
    {
        return View();
    }

    public IActionResult Responsibility() => View();
    public IActionResult Precision() => View();
    public IActionResult People() => View();
    public IActionResult Connections() => View();
    public IActionResult Contact() => View();
    public async Task<IActionResult> Menu() => View(await siteContent.GetMenusAsync());

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
