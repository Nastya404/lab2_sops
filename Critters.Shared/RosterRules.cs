namespace Critters.Shared;

public static class RosterRules
{
    public static List<string> Validate(RosterData r)
    {
        var minDate = new DateTime(1980, 1, 1);
        var maxDate = new DateTime(2016, 12, 31);
        var errors = new List<string>();

        if (r.Birthday.Date < minDate || r.Birthday.Date > maxDate)
            errors.Add("Дата рождения должна быть в диапазоне с 01.01.1980 по 31.12.2016.");
        int age = DateTime.Today.Year - r.Birthday.Year;
        if (r.Birthday.Date > DateTime.Today.AddYears(-age)) age--;

        if (r.Position == "G")
        {
            if (r.Height < 180) errors.Add("Вратарь (G) должен иметь рост не менее 180 см.");
            if (r.Weight < 80) errors.Add("Вратарь (G) должен весить не менее 80 кг.");
        }
        if (r.Position == "C" && r.Birthday.Year < 2000 && r.Jersey >= 15)
            errors.Add("Центральный нападающий (C), родившийся до 2000 года, должен иметь номер меньше 15.");
        if (r.BirthState == "MA" && r.Height > 185 && r.Weight < 90)
            errors.Add("Игрок из штата MA с ростом выше 185 см должен весить не менее 90 кг.");
        if (age < 25 && r.Position == "D" && r.Height < 185 && r.Weight > 85)
            errors.Add("Защитник (D) младше 25 лет с ростом менее 185 см не должен весить больше 85 кг.");

        return errors;
    }
}