using System.Diagnostics;
using Koi.Web.Components.Home;
using Koi.Web.Components.Shared;
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
        return new BlazorViewResult<HomePage>(new { Model = new HomeViewModel(news, categories) });
    }

    public async Task<IActionResult> OurTea()
    {
        var categories = await categoryService.GetListAsync(includeDrinks: true);
        var featured = await featuredDrinkService.GetListAsync();
        return new BlazorViewResult<OurTeaPage>(new { Model = new OurTeaViewModel(categories, featured) });
    }

    public async Task<IActionResult> Menu()
        => new BlazorViewResult<MenuPage>(new { Model = await menuService.GetListAsync() });

    public async Task<IActionResult> News(string? category, int page = 1)
        => new BlazorViewResult<NewsPage>(new { Model = await newsArticleService.GetNewsAsync(category, page) });

    public async Task<IActionResult> NewsDetail(Guid id)
    {
        var article = await newsArticleService.GetArticleAsync(id);
        return article == null ? NotFound() : new BlazorViewResult<NewsDetailPage>(new { Model = article });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewsPreview(Guid id)
    {
        var article = await newsArticleService.GetArticleAsync(id);
        if (article == null)
            return NotFound();
        return new BlazorViewResult<NewsDetailPage>(new { Model = article, IsAdminPreview = true });
    }

    public async Task<IActionResult> Store(Guid s = default, int page = 1)
        => new BlazorViewResult<StorePage>(new { Model = await storeService.GetStoresAsync(s, page) });

    public IActionResult Impact() => new BlazorViewResult<ImpactPage>();

    public IActionResult Responsibility() => new BlazorViewResult<ResponsibilityPage>();

    public IActionResult Precision() => new BlazorViewResult<PrecisionPage>();

    public IActionResult People() => new BlazorViewResult<PeoplePage>();

    public IActionResult Connections() => new BlazorViewResult<ConnectionsPage>();

    public IActionResult Contact() => new BlazorViewResult<ContactPage>();

    public IActionResult About() => new BlazorViewResult<AboutPage>();

    public IActionResult Recruit() => new BlazorViewResult<RecruitPage>();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return new BlazorViewResult<ErrorPage>(new { Model = model });
    }
}
