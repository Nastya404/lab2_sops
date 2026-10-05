using System.Globalization;
using CrittersApp.Models;
using CrittersGrpc;

namespace CrittersApp.Services;

public class GrpcRosterValidationClient : IRosterValidationClient
{
    private readonly RosterValidation.RosterValidationClient _client;

    public GrpcRosterValidationClient(RosterValidation.RosterValidationClient client)
        => _client = client;

    public async Task<IReadOnlyList<string>> ValidateAsync(Roster m)
    {
        var request = new RosterRequest
        {
            PlayerId = m.PlayerId ?? "",
            Jersey = m.Jersey,
            Position = m.Position,
            Birthday = m.Birthday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Height = m.Height,
            Weight = m.Weight,
            BirthState = m.BirthState
        };

        var reply = await _client.ValidateAsync(request);
        return reply.Errors.ToList();
    }
}