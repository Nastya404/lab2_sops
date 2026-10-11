using CrittersApp.Models;

namespace CrittersApp.Services;

public interface IRosterClient
{
    Task<IReadOnlyList<Player>> GetPlayersAsync();
    Task<IReadOnlyList<FieldError>> AddPlayerAsync(Player player);
    Task<PlayerOptions> GetOptionsAsync();
}
