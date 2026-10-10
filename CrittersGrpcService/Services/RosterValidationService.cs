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
            reply.Errors.Add("Некорректный формат даты рождения (ожидается yyyy-MM-dd).");
            return Task.FromResult(reply);
        }

        reply.Errors.AddRange(RosterRules.Validate(ToRosterData(request, birthday)));
        return Task.FromResult(reply);
    }

    // Маппинг protobuf-сообщения -> доменная модель правил.
    // Int32Value (wrappers.proto) генерируется как int?, пустая строка proto3 = «нет значения».
    private static RosterData ToRosterData(RosterRequest r, DateTime birthday) => new(
        PlayerId: string.IsNullOrWhiteSpace(r.PlayerId) ? null : r.PlayerId.Trim(),
        Jersey: r.Jersey,
        Position: r.Position.Trim().ToUpperInvariant(),
        Birthday: birthday,
        Height: r.Height,
        Weight: r.Weight,
        BirthState: r.BirthState.Trim().ToUpperInvariant());
}
