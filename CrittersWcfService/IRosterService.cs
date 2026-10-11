using CoreWCF;
using System.Runtime.Serialization;

namespace CrittersWcfService;

public static class Ns { public const string Value = "http://critters.example/roster"; }

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

[DataContract(Namespace = Ns.Value)]
public class FieldErrorDto
{
    [DataMember] public string Field { get; set; } = "";
    [DataMember] public string Message { get; set; } = "";
}

[DataContract(Namespace = Ns.Value)]
public class OptionsDto
{
    [DataMember] public string[] Positions { get; set; } = [];
    [DataMember] public string[] States { get; set; } = [];
}

[ServiceContract(Namespace = Ns.Value)]
public interface IRosterService
{
    [OperationContract]
    Task<PlayerDto[]> GetPlayers();

    [OperationContract]
    Task<FieldErrorDto[]> AddPlayer(PlayerDto player);

    [OperationContract]
    OptionsDto GetOptions();
}
