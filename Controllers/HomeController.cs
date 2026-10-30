using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Koi.Web.Models;
using Koi.Web.Data;

namespace Koi.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Cover()
    {
        return View();
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult OurTea()
    {
        return View();
    }

    public IActionResult Recruit()
    {
        return View();
    }

    public IActionResult News(string? category, int page = 1)
    {
        category = category?.ToUpperInvariant();
        if (category is not ("NEWS" or "MEDIA")) category = null;
        var articles = NewsCatalog.Articles
            .Where(article => article.Id != 2782 && (category == null || article.Category == category))
            .OrderByDescending(article => article.Date)
            .ToArray();
        const int pageSize = 5;
        var pageCount = Math.Max(1, (int)Math.Ceiling(articles.Length / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        return View(new NewsViewModel(
            NewsCatalog.Articles.Single(article => article.Id == 2782),
            articles.Skip((page - 1) * pageSize).Take(pageSize).ToArray(),
            category, page, pageCount));
    }

    public IActionResult NewsDetail(int id)
    {
        var article = NewsCatalog.Articles.FirstOrDefault(article => article.Id == id);
        return article == null ? NotFound() : View(article);
    }

    public IActionResult Impact()
    {
        return View();
    }

    public IActionResult Responsibility() => View();
    public IActionResult Precision() => View();
    public IActionResult People() => View();
    public IActionResult Connections() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
