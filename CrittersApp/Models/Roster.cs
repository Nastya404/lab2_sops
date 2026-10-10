using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrittersApp.Models;

[Table("roster")]
public class Roster
{
    [Key]
    [Column("playerid")]
    [Required(ErrorMessage = "Введите ID игрока")]
    [StringLength(20, ErrorMessage = "ID не длиннее 20 символов")]
    [Display(Name = "ID игрока")]
    public string PlayerId { get; set; } = "";

    [Column("jersey")]
    [Required(ErrorMessage = "Введите номер")]
    [Range(1, 99, ErrorMessage = "Номер должен быть от 1 до 99")]
    [Display(Name = "Номер")]
    public int? Jersey { get; set; }

    [Column("fname")]
    [Required(ErrorMessage = "Введите имя")]
    [StringLength(50)]
    [Display(Name = "Имя")]
    public string FName { get; set; } = "";

    [Column("sname")]
    [Required(ErrorMessage = "Введите фамилию")]
    [StringLength(50)]
    [Display(Name = "Фамилия")]
    public string SName { get; set; } = "";

    [Column("position")]
    [Required(ErrorMessage = "Выберите позицию")]
    [Display(Name = "Позиция")]
    public string Position { get; set; } = "";

    [Column("birthday")]
    [Required(ErrorMessage = "Введите дату рождения")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateTime Birthday { get; set; }

    [Column("height")]
    [Required(ErrorMessage = "Введите рост")]
    [Range(120, 230, ErrorMessage = "Рост должен быть от 120 до 230 см")]
    [Display(Name = "Рост, см")]
    public int? Height { get; set; }

    [Column("weight")]
    [Required(ErrorMessage = "Введите вес")]
    [Range(30, 150, ErrorMessage = "Вес должен быть от 30 до 150 кг")]
    [Display(Name = "Вес, кг")]
    public int? Weight { get; set; }

    [Column("birthcity")]
    [Required(ErrorMessage = "Введите город рождения")]
    [StringLength(50)]
    [Display(Name = "Город рождения")]
    public string BirthCity { get; set; } = "";

    [Column("birthstate")]
    [Required(ErrorMessage = "Выберите штат рождения")]
    [Display(Name = "Штат рождения")]
    public string BirthState { get; set; } = "";
}
