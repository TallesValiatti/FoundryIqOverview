using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FoundryIqOverview.WebSite.Models;

namespace FoundryIqOverview.WebSite.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult History()
    {
        return View();
    }

    public IActionResult Platform()
    {
        return View();
    }

    public IActionResult Customers()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
