using System.Globalization;
using Critters.Shared;
using Grpc.Core;

namespace CrittersGrpc.Services;

public class RosterValidationService : RosterValidation.RosterValidationBase
{
    public override Task<ValidationReply> Validate(RosterRequest request, ServerCallContext context)
    {
        var reply = new ValidationReply();

        if (!DateTime.TryParseExact(request.Birthday, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthday))
        {
            reply.Errors.Add("Некорректный формат даты рождения.");
            return Task.FromResult(reply);
        }

        var data = new RosterData(request.PlayerId, request.Jersey, request.Position,
                                  birthday, request.Height, request.Weight, request.BirthState);

        reply.Errors.AddRange(RosterRules.Validate(data));
        return Task.FromResult(reply);
    }
}