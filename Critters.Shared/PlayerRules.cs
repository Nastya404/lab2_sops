using System.ComponentModel.DataAnnotations;

namespace Critters.Shared;

// Ошибка проверки: Field — имя свойства Player (пусто — ошибка всей записи)
public record FieldError(string Field, string Message);

public static class PlayerRules
{
    private static readonly DateTime MinBirthday = new(1980, 1, 1);
    private static readonly DateTime MaxBirthday = new(2016, 12, 31);

    public static List<FieldError> Validate(Player p)
    {
        var errors = ValidateFields(p);
        if (errors.Count == 0)
            errors.AddRange(ValidateBusinessRules(p));
        return errors;
    }

    // Обязательность, длины, диапазоны (атрибуты Player) и допустимые значения списков
    private static List<FieldError> ValidateFields(Player p)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(p, new ValidationContext(p), results, validateAllProperties: true);
        var errors = results
            .Select(r => new FieldError(r.MemberNames.FirstOrDefault() ?? "", r.ErrorMessage ?? ""))
            .ToList();

        if (p.Position.Length > 0 && !PlayerLookups.Positions.Contains(p.Position))
            errors.Add(new(nameof(Player.Position), "Недопустимая позиция"));
        if (p.BirthState.Length > 0 && !PlayerLookups.States.Contains(p.BirthState))
            errors.Add(new(nameof(Player.BirthState), "Недопустимый штат рождения"));
        if (p.Birthday is { } b && (b.Date < MinBirthday || b.Date > MaxBirthday))
            errors.Add(new(nameof(Player.Birthday), "Дата рождения должна быть в диапазоне с 01.01.1980 по 31.12.2016"));

        return errors;
    }

    // Правила предметной области (вызываются, когда все поля заполнены корректно)
    private static IEnumerable<FieldError> ValidateBusinessRules(Player p)
    {
        var birthday = p.Birthday!.Value.Date;
        int age = DateTime.Today.Year - birthday.Year;
        if (birthday > DateTime.Today.AddYears(-age)) age--;

        if (p.Position == "G")
        {
            if (p.Height < 180) yield return new("", "Вратарь (G) должен иметь рост не менее 180 см.");
            if (p.Weight < 80) yield return new("", "Вратарь (G) должен весить не менее 80 кг.");
        }
        if (p.Position == "C" && birthday.Year < 2000 && p.Jersey >= 15)
            yield return new("", "Центральный нападающий (C), родившийся до 2000 года, должен иметь номер меньше 15.");
        if (p.BirthState == "MA" && p.Height > 185 && p.Weight < 90)
            yield return new("", "Игрок из штата MA с ростом выше 185 см должен весить не менее 90 кг.");
        if (age < 25 && p.Position == "D" && p.Height < 185 && p.Weight > 85)
            yield return new("", "Защитник (D) младше 25 лет с ростом менее 185 см не должен весить больше 85 кг.");
    }
}
