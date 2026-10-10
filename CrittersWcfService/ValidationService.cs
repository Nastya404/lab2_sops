using CoreWCF;
using Critters.Shared;

namespace CrittersWcfService;

// Namespace задан явно, иначе WSDL службы публикуется в http://tempuri.org/
// и разбивается на два документа (wsdl + wsdl:import).
[ServiceBehavior(Namespace = Ns.Value)]
public class ValidationService : IValidationService
{
    public string[] Validate(RosterDto roster)
    {
        if (roster is null)
            throw new FaultException("Не переданы данные игрока.");

        return RosterRules.Validate(ToRosterData(roster)).ToArray();
    }

    // Маппинг SOAP DataContract -> доменная модель правил
    private static RosterData ToRosterData(RosterDto d) => new(
        PlayerId: d.PlayerId?.Trim(),
        Jersey: d.Jersey,
        Position: (d.Position ?? "").Trim().ToUpperInvariant(),
        Birthday: d.Birthday.Date,
        Height: d.Height,
        Weight: d.Weight,
        BirthState: (d.BirthState ?? "").Trim().ToUpperInvariant());
}
