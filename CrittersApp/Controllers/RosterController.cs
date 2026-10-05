using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrittersApp.Data;
using CrittersApp.Models;
using CrittersApp.Services;

namespace CrittersApp.Controllers
{
    public class RosterController: Controller
    {
        private readonly CrittersContext _db;
        private readonly IRosterValidationClient _validator;

        public RosterController(CrittersContext db, IRosterValidationClient validator)
        {
            _db = db;
            _validator = validator;
        }

        public async Task<IActionResult> Index()
        {
            var players = await _db.Roster
            .Where(p => p.PlayerId != "playerid")  
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
            try
            {
                var errors = await _validator.ValidateAsync(model);
                foreach (var e in errors)
                    ModelState.AddModelError(string.Empty, e);
            }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty,
                        "Служба проверки правил недоступна. Попробуйте позже.");
                }

            if (!string.IsNullOrWhiteSpace(model.PlayerId))
            {
                model.PlayerId = model.PlayerId.Trim();

                if (await _db.Roster.AnyAsync(p => p.PlayerId == model.PlayerId))
                    ModelState.AddModelError(nameof(model.PlayerId),
                        $"Игрок с ID «{model.PlayerId}» уже существует.");
            }

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

        private void FillDropdowns()
        {
            ViewBag.Positions = new List<string> { "C", "LW", "RW", "D", "G" };
            ViewBag.States = new List<string>
            {
                "MA","CT","OH","NJ","NY","IL","MN","MI","CO","ME","DE","BC","ON","QU","SW","RU","UK","CZ","FI"
            };
        }

    }
}