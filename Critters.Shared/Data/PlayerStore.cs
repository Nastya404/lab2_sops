using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Critters.Shared.Data;

// Хранилище игроков на сервере. Используется и WCF-, и gRPC-службой.
public class PlayerStore
{
    private readonly IDbContextFactory<CrittersContext> _dbFactory;

    public PlayerStore(IDbContextFactory<CrittersContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<List<Player>> GetAllAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Roster.AsNoTracking().OrderBy(p => p.SName).ToListAsync();
    }

    // Проверяет запись (поля, правила, уникальность ID и номера) и сохраняет её.
    // Пустой список ошибок — запись сохранена.
    public async Task<List<FieldError>> AddAsync(Player player)
    {
        Normalize(player);
        var errors = PlayerRules.Validate(player);

        await using var db = await _dbFactory.CreateDbContextAsync();

        if (player.PlayerId.Length > 0 && await db.Roster.AnyAsync(p => p.PlayerId == player.PlayerId))
            errors.Add(new(nameof(Player.PlayerId), $"Игрок с ID «{player.PlayerId}» уже существует"));
        if (player.Jersey.HasValue && await db.Roster.AnyAsync(p => p.Jersey == player.Jersey))
            errors.Add(new(nameof(Player.Jersey), $"Номер {player.Jersey} уже занят другим игроком"));

        if (errors.Count > 0)
            return errors;

        db.Roster.Add(player);
        await db.SaveChangesAsync();
        return errors;
    }

    private static void Normalize(Player p)
    {
        p.PlayerId = (p.PlayerId ?? "").Trim();
        p.FName = (p.FName ?? "").Trim();
        p.SName = (p.SName ?? "").Trim();
        p.BirthCity = (p.BirthCity ?? "").Trim();
        p.Position = (p.Position ?? "").Trim().ToUpperInvariant();
        p.BirthState = (p.BirthState ?? "").Trim().ToUpperInvariant();
        p.Birthday = p.Birthday?.Date;
    }
}

public static class CrittersDataExtensions
{
    // Подключение БД для службы. Путь — "Database" из appsettings.json,
    // относительный путь считается от папки проекта службы.
    public static IServiceCollection AddCrittersData(this IServiceCollection services,
        IConfiguration config, string contentRoot)
    {
        var dbPath = GetDatabasePath(config, contentRoot);
        services.AddDbContextFactory<CrittersContext>(o => o.UseSqlite($"Data Source={dbPath}"));
        services.AddSingleton<PlayerStore>();
        return services;
    }

    public static string GetDatabasePath(IConfiguration config, string contentRoot) =>
        Path.GetFullPath(Path.Combine(contentRoot, config["Database"] ?? "../CRITTERS.db"));
}
