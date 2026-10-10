using System.Globalization;
using Critters.Shared;
using Critters.Shared.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PlayerMessage = CrittersGrpc.Player;

namespace CrittersGrpc.Services;

public class RosterGrpcService : Roster.RosterBase
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly PlayerStore _store;
    private readonly ILogger<RosterGrpcService> _logger;

    public RosterGrpcService(PlayerStore store, ILogger<RosterGrpcService> logger)
    {
        _store = store;
        _logger = logger;
    }

    private static string Client(ServerCallContext context) =>
        NetworkInfo.Format(context.GetHttpContext().Connection.RemoteIpAddress);

    public override async Task<PlayerList> GetPlayers(Empty request, ServerCallContext context)
    {
        var players = await _store.GetAllAsync();
        _logger.LogInformation("gRPC GetPlayers от {Client}: {Count} игроков", Client(context), players.Count);

        var reply = new PlayerList();
        reply.Players.AddRange(players.Select(ToMessage));
        return reply;
    }

    public override async Task<AddPlayerReply> AddPlayer(PlayerMessage request, ServerCallContext context)
    {
        var reply = new AddPlayerReply();

        DateTime? birthday = null;
        if (request.Birthday.Length > 0)
        {
            if (!DateTime.TryParseExact(request.Birthday, DateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed))
            {
                reply.Errors.Add(new FieldError { Field = nameof(Critters.Shared.Player.Birthday), Message = "Некорректная дата рождения" });
                return reply;
            }
            birthday = parsed;
        }

        var player = ToPlayer(request, birthday);
        var errors = await _store.AddAsync(player);

        _logger.LogInformation("gRPC AddPlayer от {Client}: {Player} -> {Result}", Client(context), player,
            errors.Count == 0 ? "СОХРАНЁН" : $"отклонён [{string.Join(" | ", errors.Select(e => e.Message))}]");

        reply.Errors.AddRange(errors.Select(e => new FieldError { Field = e.Field, Message = e.Message }));
        return reply;
    }

    public override Task<Options> GetOptions(Empty request, ServerCallContext context)
    {
        var reply = new Options();
        reply.Positions.AddRange(PlayerLookups.Positions);
        reply.States.AddRange(PlayerLookups.States);
        return Task.FromResult(reply);
    }

    // Маппинг protobuf-сообщения <-> сущность БД.
    // Int32Value (wrappers.proto) генерируется как int?, пустая строка proto3 = «нет значения».
    private static Critters.Shared.Player ToPlayer(PlayerMessage m, DateTime? birthday) => new()
    {
        PlayerId = m.PlayerId,
        Jersey = m.Jersey,
        FName = m.Fname,
        SName = m.Sname,
        Position = m.Position,
        Birthday = birthday,
        Height = m.Height,
        Weight = m.Weight,
        BirthCity = m.BirthCity,
        BirthState = m.BirthState
    };

    private static PlayerMessage ToMessage(Critters.Shared.Player p) => new()
    {
        PlayerId = p.PlayerId,
        Jersey = p.Jersey,
        Fname = p.FName,
        Sname = p.SName,
        Position = p.Position,
        Birthday = p.Birthday?.ToString(DateFormat, CultureInfo.InvariantCulture) ?? "",
        Height = p.Height,
        Weight = p.Weight,
        BirthCity = p.BirthCity,
        BirthState = p.BirthState
    };
}
