using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Koi.Web.Models;

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
