using CrittersApp.Models;

namespace CrittersApp.Services;

// Клиент удалённой службы. Две реализации: WCF (вариант 2.1) и gRPC (вариант 2.2) —
// контроллер не знает, какая используется.
public interface IRosterClient
{
    Task<IReadOnlyList<Player>> GetPlayersAsync();

    // Пустой список — запись сохранена на сервере
    Task<IReadOnlyList<FieldError>> AddPlayerAsync(Player player);

    Task<PlayerOptions> GetOptionsAsync();
}
