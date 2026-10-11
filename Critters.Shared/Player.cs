using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Critters.Shared;

[Table("roster")]
public class Player
{
    [Key]
    [Column("playerid")]
    [Required(ErrorMessage = "Введите ID игрока")]
    [StringLength(20, ErrorMessage = "ID не длиннее 20 символов")]
    public string PlayerId { get; set; } = "";

    [Column("jersey")]
    [Required(ErrorMessage = "Введите номер")]
    [Range(1, 99, ErrorMessage = "Номер должен быть от 1 до 99")]
    public int? Jersey { get; set; }

    [Column("fname")]
    [Required(ErrorMessage = "Введите имя")]
    [StringLength(50, ErrorMessage = "Имя не длиннее 50 символов")]
    public string FName { get; set; } = "";

    [Column("sname")]
    [Required(ErrorMessage = "Введите фамилию")]
    [StringLength(50, ErrorMessage = "Фамилия не длиннее 50 символов")]
    public string SName { get; set; } = "";

    [Column("position")]
    [Required(ErrorMessage = "Выберите позицию")]
    public string Position { get; set; } = "";

    [Column("birthday")]
    [Required(ErrorMessage = "Введите дату рождения")]
    public DateTime? Birthday { get; set; }

    [Column("height")]
    [Required(ErrorMessage = "Введите рост")]
    [Range(120, 230, ErrorMessage = "Рост должен быть от 120 до 230 см")]
    public int? Height { get; set; }

    [Column("weight")]
    [Required(ErrorMessage = "Введите вес")]
    [Range(30, 150, ErrorMessage = "Вес должен быть от 30 до 150 кг")]
    public int? Weight { get; set; }

    [Column("birthcity")]
    [Required(ErrorMessage = "Введите город рождения")]
    [StringLength(50, ErrorMessage = "Город не длиннее 50 символов")]
    public string BirthCity { get; set; } = "";

    [Column("birthstate")]
    [Required(ErrorMessage = "Выберите штат рождения")]
    public string BirthState { get; set; } = "";

    public override string ToString() =>
        $"{PlayerId} #{Jersey} {FName} {SName}, {Position}, {Birthday:yyyy-MM-dd}, {Height} см, {Weight} кг, {BirthCity} {BirthState}";
}
