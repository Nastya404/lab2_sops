using Critters.Shared;

namespace CrittersWcfService;

public class ValidationService : IValidationService
{
    public string[] Validate(RosterDto d)
    {
        var data = new RosterData(d.PlayerId, d.Jersey, d.Position, d.Birthday,
                                  d.Height, d.Weight, d.BirthState);
        return RosterRules.Validate(data).ToArray();
    }
}
