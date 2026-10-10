using CrittersApp.Models;
using CrittersGrpc;
using Microsoft.Extensions.Options;

namespace CrittersApp.Services.Grpc;

public class GrpcRosterValidationClient : IRosterValidationClient
{
    private readonly RosterValidation.RosterValidationClient _client;
    private readonly ValidationOptions _options;

    public GrpcRosterValidationClient(RosterValidation.RosterValidationClient client,
                                      IOptions<ValidationOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<string>> ValidateAsync(Roster m)
    {
        var reply = await _client.ValidateAsync(m.ToGrpcRequest(),
            deadline: DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds));
        return reply.Errors.ToList();
    }
}
