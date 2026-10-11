using System.ComponentModel.DataAnnotations;

namespace CrittersApp.Models;
public class Player
{
    [Display(Name = "ID игрока")]
    public string? PlayerId { get; set; }

    [Display(Name = "Номер")]
    public int? Jersey { get; set; }

    [Display(Name = "Имя")]
    public string? FName { get; set; }

    [Display(Name = "Фамилия")]
    public string? SName { get; set; }

    [Display(Name = "Позиция")]
    public string? Position { get; set; }

    [Display(Name = "Дата рождения")]
    [DataType(DataType.Date)]
    public DateTime? Birthday { get; set; }

    [Display(Name = "Рост, см")]
    public int? Height { get; set; }

    [Display(Name = "Вес, кг")]
    public int? Weight { get; set; }

    [Display(Name = "Город рождения")]
    public string? BirthCity { get; set; }

    [Display(Name = "Штат рождения")]
    public string? BirthState { get; set; }
}

// Ошибка проверки с сервера: Field — имя поля (пусто — ошибка всей записи)
public record FieldError(string Field, string Message);

// Допустимые значения для выпадающих списков (приходят с сервера)
public record PlayerOptions(IReadOnlyList<string> Positions, IReadOnlyList<string> States);
