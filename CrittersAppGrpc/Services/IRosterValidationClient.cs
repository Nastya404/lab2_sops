using CrittersApp.Models;

namespace CrittersApp.Services;

public interface IRosterValidationClient
{
    Task<IReadOnlyList<string>> ValidateAsync(Roster model);
}