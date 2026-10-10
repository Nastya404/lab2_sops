using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CrittersApp.Data;
using CrittersApp.Models;
using CrittersApp.Services;

namespace CrittersApp.Controllers;

public class RosterController : Controller
{
    private readonly CrittersContext _db;
    private readonly IRosterValidationClient _validator;
    private readonly ILogger<RosterController> _logger;

    public RosterController(CrittersContext db, IRosterValidationClient validator,
                            ILogger<RosterController> logger)
    {
        _db = db;
        _validator = validator;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var players = await _db.Roster
            .AsNoTracking()
            .OrderBy(p => p.SName)
            .ToListAsync();
        return View(players);
    }

    public IActionResult Create()
    {
        FillDropdowns();
        return View(new Roster { Birthday = new DateTime(2000, 1, 1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Roster model)
    {
        model.PlayerId = model.PlayerId?.Trim() ?? "";

        if (!RosterLookups.Positions.Contains(model.Position))
            ModelState.AddModelError(nameof(model.Position), "Недопустимая позиция.");
        if (!RosterLookups.States.Contains(model.BirthState))
            ModelState.AddModelError(nameof(model.BirthState), "Недопустимый штат рождения.");

        // Правила предметной области проверяет удалённая служба (WCF или gRPC).
        // Вызываем её только для формально корректной записи.
        if (ModelState.IsValid)
            await ValidateRulesAsync(model);

        if (model.PlayerId.Length > 0 &&
            await _db.Roster.AnyAsync(p => p.PlayerId == model.PlayerId))
            ModelState.AddModelError(nameof(model.PlayerId),
                $"Игрок с ID «{model.PlayerId}» уже существует.");

        if (model.Jersey.HasValue &&
            await _db.Roster.AnyAsync(p => p.Jersey == model.Jersey))
            ModelState.AddModelError(nameof(model.Jersey),
                $"Номер {model.Jersey} уже занят другим игроком.");

        if (!ModelState.IsValid)
        {
            FillDropdowns();
            return View(model);
        }

        _db.Roster.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateRulesAsync(Roster model)
    {
        try
        {
            foreach (var error in await _validator.ValidateAsync(model))
                ModelState.AddModelError(string.Empty, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка вызова службы проверки правил");
            ModelState.AddModelError(string.Empty,
                $"Служба проверки правил недоступна: {ex.Message}");
        }
    }

    private void FillDropdowns()
    {
        ViewBag.Positions = new SelectList(RosterLookups.Positions);
        ViewBag.States = new SelectList(RosterLookups.States);
    }
}
