using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CrittersApp.Models;

namespace CrittersApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(RosterController.Index), "Roster");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
