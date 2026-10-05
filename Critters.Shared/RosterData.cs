namespace Critters.Shared;

public record RosterData(
    string? PlayerId, int? Jersey, string Position, DateTime Birthday,
    int? Height, int? Weight, string BirthState
    );