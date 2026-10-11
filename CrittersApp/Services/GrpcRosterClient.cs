using CrittersApp.Models;
using CrittersGrpc;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;

namespace CrittersApp.Services;

public class GrpcRosterClient : IRosterClient
{
    private readonly Roster.RosterClient _client;
    private readonly RosterServiceOptions _options;

    public GrpcRosterClient(Roster.RosterClient client, IOptions<RosterServiceOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    private DateTime Deadline => DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds);

    public async Task<IReadOnlyList<Models.Player>> GetPlayersAsync()
    {
        var reply = await _client.GetPlayersAsync(new Empty(), deadline: Deadline);
        return reply.Players.Select(p => p.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<Models.FieldError>> AddPlayerAsync(Models.Player player)
    {
        var reply = await _client.AddPlayerAsync(player.ToGrpcMessage(), deadline: Deadline);
        return reply.Errors.Select(e => e.ToModel()).ToList();
    }

    public async Task<PlayerOptions> GetOptionsAsync()
    {
        var reply = await _client.GetOptionsAsync(new Empty(), deadline: Deadline);
        return new PlayerOptions(reply.Positions.ToList(), reply.States.ToList());
    }
}
