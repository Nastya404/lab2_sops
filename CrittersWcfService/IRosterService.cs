using CoreWCF;
using System.Runtime.Serialization;

namespace CrittersWcfService;

public static class Ns { public const string Value = "http://critters.example/roster"; }

// Игрок в SOAP-сообщениях
[DataContract(Namespace = Ns.Value)]
public class PlayerDto
{
    [DataMember] public string? PlayerId { get; set; }
    [DataMember] public int? Jersey { get; set; }
    [DataMember] public string? FName { get; set; }
    [DataMember] public string? SName { get; set; }
    [DataMember] public string? Position { get; set; }
    [DataMember] public DateTime? Birthday { get; set; }
    [DataMember] public int? Height { get; set; }
    [DataMember] public int? Weight { get; set; }
    [DataMember] public string? BirthCity { get; set; }
    [DataMember] public string? BirthState { get; set; }
}

// Ошибка проверки: Field — имя поля (пусто — ошибка всей записи)
[DataContract(Namespace = Ns.Value)]
public class FieldErrorDto
{
    [DataMember] public string Field { get; set; } = "";
    [DataMember] public string Message { get; set; } = "";
}

// Допустимые значения для выпадающих списков формы
[DataContract(Namespace = Ns.Value)]
public class OptionsDto
{
    [DataMember] public string[] Positions { get; set; } = [];
    [DataMember] public string[] States { get; set; } = [];
}

[ServiceContract(Namespace = Ns.Value)]
public interface IRosterService
{
    // Все игроки из БД сервера
    [OperationContract]
    Task<PlayerDto[]> GetPlayers();

    // Проверка и сохранение игрока на сервере. Пустой массив — запись сохранена.
    [OperationContract]
    Task<FieldErrorDto[]> AddPlayer(PlayerDto player);

    [OperationContract]
    OptionsDto GetOptions();
}
