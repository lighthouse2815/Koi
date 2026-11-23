using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace Koi.Web.Controllers;

public class HomeController(
    NewsArticleService newsArticleService,
    CategoryService categoryService,
    FeaturedDrinkService featuredDrinkService,
    StoreService storeService,
    MenuService menuService
) : Controller
{
    public async Task<IActionResult> Index()
    {
        var news = await newsArticleService.GetForHomeAsync();
        var categories = await categoryService.GetListAsync(forHome: true);
        return View(new HomeViewModel(news, categories));
    }

    public async Task<IActionResult> OurTea()
    {
        var categories = await categoryService.GetListAsync(includeDrinks: true);
        var featured = await featuredDrinkService.GetListAsync();
        return View(new OurTeaViewModel(categories, featured));
    }

    public async Task<IActionResult> Menu() => View(await menuService.GetListAsync());

    public async Task<IActionResult> News(string? category, int page = 1)
        => View(await newsArticleService.GetNewsAsync(category, page));

    public async Task<IActionResult> NewsDetail(Guid id)
    {
        var article = await newsArticleService.GetArticleAsync(id);
        return article == null ? NotFound() : View(article);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewsPreview(Guid id)
    {
        var article = await newsArticleService.GetArticleAsync(id);
        if (article == null)
            return NotFound();
        ViewData["IsAdminPreview"] = true;
        return View("NewsDetail", article);
    }

    public async Task<IActionResult> Store(Guid s = default, int page = 1)
        => View(await storeService.GetStoresAsync(s, page));

    public IActionResult Impact() => View();

    public IActionResult Responsibility() => View();

    public IActionResult Precision() => View();

    public IActionResult People() => View();

    public IActionResult Connections() => View();

    public IActionResult Contact() => View();

    public IActionResult About() => View();

    public IActionResult Recruit() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
