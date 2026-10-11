using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CrittersApp.Models;
using CrittersApp.Services;

namespace CrittersApp.Controllers;

public class RosterController : Controller
{
    private readonly IRosterClient _service;
    private readonly ILogger<RosterController> _logger;

    public RosterController(IRosterClient service, ILogger<RosterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            return View(await _service.GetPlayersAsync());
        }
        catch (Exception ex)
        {
            ReportUnavailable(ex);
            return View(Array.Empty<Player>());
        }
    }

    public async Task<IActionResult> Create()
    {
        await FillDropdownsAsync();
        return View(new Player { Birthday = new DateTime(2000, 1, 1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Player model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var errors = await _service.AddPlayerAsync(model);
                if (errors.Count == 0)
                    return RedirectToAction(nameof(Index));

                foreach (var e in errors)
                    ModelState.AddModelError(e.Field, e.Message);
            }
            catch (Exception ex)
            {
                ReportUnavailable(ex);
            }
        }

        await FillDropdownsAsync();
        return View(model);
    }

    private async Task FillDropdownsAsync()
    {
        try
        {
            var options = await _service.GetOptionsAsync();
            ViewBag.Positions = new SelectList(options.Positions);
            ViewBag.States = new SelectList(options.States);
        }
        catch (Exception ex)
        {
            ReportUnavailable(ex);
            ViewBag.Positions = ViewBag.States = new SelectList(Array.Empty<string>());
        }
    }

    private void ReportUnavailable(Exception ex)
    {
        _logger.LogError(ex, "Ошибка обращения к серверу");
        var message = $"Сервер недоступен: {ex.Message}";
        if (ModelState[string.Empty]?.Errors.Any(e => e.ErrorMessage == message) != true)
            ModelState.AddModelError(string.Empty, message);
    }
}
