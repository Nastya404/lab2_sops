using CoreWCF;
using Critters.Shared;
using Critters.Shared.Data;

namespace CrittersWcfService;

// Namespace задан явно, иначе WSDL службы публикуется в http://tempuri.org/
// и разбивается на два документа (wsdl + wsdl:import).
[ServiceBehavior(Namespace = Ns.Value)]
public class RosterService : IRosterService
{
    private readonly PlayerStore _store;
    private readonly ILogger<RosterService> _logger;
    private readonly IHttpContextAccessor _http;

    public RosterService(PlayerStore store, ILogger<RosterService> logger, IHttpContextAccessor http)
    {
        _store = store;
        _logger = logger;
        _http = http;
    }

    private string Client => NetworkInfo.Format(_http.HttpContext?.Connection.RemoteIpAddress);

    public async Task<PlayerDto[]> GetPlayers()
    {
        var players = await _store.GetAllAsync();
        _logger.LogInformation("SOAP GetPlayers от {Client}: {Count} игроков", Client, players.Count);
        return players.Select(ToDto).ToArray();
    }

    public async Task<FieldErrorDto[]> AddPlayer(PlayerDto dto)
    {
        if (dto is null)
            throw new FaultException("Не переданы данные игрока.");

        var player = ToPlayer(dto);
        var errors = await _store.AddAsync(player);

        _logger.LogInformation("SOAP AddPlayer от {Client}: {Player} -> {Result}", Client, player,
            errors.Count == 0 ? "СОХРАНЁН" : $"отклонён [{string.Join(" | ", errors.Select(e => e.Message))}]");

        return errors.Select(e => new FieldErrorDto { Field = e.Field, Message = e.Message }).ToArray();
    }

    public OptionsDto GetOptions() =>
        new() { Positions = PlayerLookups.Positions, States = PlayerLookups.States };

    // Маппинг SOAP DataContract <-> сущность БД
    private static Player ToPlayer(PlayerDto d) => new()
    {
        PlayerId = d.PlayerId ?? "",
        Jersey = d.Jersey,
        FName = d.FName ?? "",
        SName = d.SName ?? "",
        Position = d.Position ?? "",
        Birthday = d.Birthday,
        Height = d.Height,
        Weight = d.Weight,
        BirthCity = d.BirthCity ?? "",
        BirthState = d.BirthState ?? ""
    };

    private static PlayerDto ToDto(Player p) => new()
    {
        PlayerId = p.PlayerId,
        Jersey = p.Jersey,
        FName = p.FName,
        SName = p.SName,
        Position = p.Position,
        Birthday = p.Birthday,
        Height = p.Height,
        Weight = p.Weight,
        BirthCity = p.BirthCity,
        BirthState = p.BirthState
    };
}
