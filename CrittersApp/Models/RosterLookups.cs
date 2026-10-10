namespace CrittersApp.Models;

// Допустимые значения для выпадающих списков формы и серверной проверки
public static class RosterLookups
{
    public static readonly string[] Positions = ["C", "LW", "RW", "D", "G"];

    public static readonly string[] States =
    [
        "MA", "CT", "OH", "NJ", "NY", "IL", "MN", "MI", "CO", "ME", "DE",
        "BC", "ON", "QU", "SW", "RU", "UK", "CZ", "FI"
    ];
}
