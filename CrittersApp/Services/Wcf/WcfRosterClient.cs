using CrittersApp.Models;
using CrittersApp.RosterServiceReference;
using Microsoft.Extensions.Options;

namespace CrittersApp.Services.Wcf;

// Вариант 2.1: SOAP через прокси-класс RosterServiceClient, сгенерированный dotnet-svcutil по WSDL
public class WcfRosterClient : IRosterClient
{
    private readonly RosterServiceOptions _options;

    public WcfRosterClient(IOptions<RosterServiceOptions> options) => _options = options.Value;

    public async Task<IReadOnlyList<Player>> GetPlayersAsync()
    {
        var players = await CallAsync(c => c.GetPlayersAsync());
        return (players ?? []).Select(p => p.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<FieldError>> AddPlayerAsync(Player player)
    {
        var errors = await CallAsync(c => c.AddPlayerAsync(player.ToWcfDto()));
        return (errors ?? []).Select(e => e.ToModel()).ToList();
    }

    public async Task<PlayerOptions> GetOptionsAsync()
    {
        var o = await CallAsync(c => c.GetOptionsAsync());
        return new PlayerOptions(o.Positions ?? [], o.States ?? []);
    }

    // Один вызов = один прокси: открыть, вызвать, закрыть (при ошибке — Abort)
    private async Task<T> CallAsync<T>(Func<RosterServiceClient, Task<T>> call)
    {
        var client = new RosterServiceClient(
            RosterServiceClient.EndpointConfiguration.BasicHttpBinding_IRosterService, _options.WcfUrl);
        var timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        client.Endpoint.Binding.OpenTimeout = timeout;
        client.Endpoint.Binding.SendTimeout = timeout;
        try
        {
            var result = await call(client);
            client.Close();
            return result;
        }
        catch
        {
            client.Abort();
            throw;
        }
    }
}
