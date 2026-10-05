using CoreWCF;
using System.Runtime.Serialization;

namespace CrittersWcfService;

public static class Ns { public const string Value = "http://critters.example/validation"; }

[DataContract(Namespace = Ns.Value)]
public class RosterDto
{
    [DataMember] public string? PlayerId { get; set; }
    [DataMember] public int? Jersey { get; set; }
    [DataMember] public string Position { get; set; } = "";
    [DataMember] public DateTime Birthday { get; set; }
    [DataMember] public int? Height { get; set; }
    [DataMember] public int? Weight { get; set; }
    [DataMember] public string BirthState { get; set; } = "";
}

[ServiceContract(Namespace = Ns.Value)]
public interface IValidationService
{
    [OperationContract]
    string[] Validate(RosterDto roster);
}