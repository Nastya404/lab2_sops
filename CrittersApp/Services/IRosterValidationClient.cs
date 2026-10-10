using CrittersApp.Models;

namespace CrittersApp.Services;

// Общий интерфейс для обоих вариантов: контроллер не знает, WCF это или gRPC
public interface IRosterValidationClient
{
    Task<IReadOnlyList<string>> ValidateAsync(Roster model);
}
