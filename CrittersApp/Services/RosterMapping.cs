using System.Globalization;
using CrittersApp.Models;
using CrittersApp.RosterServiceReference;
using PlayerMessage = CrittersGrpc.Player;

namespace CrittersApp.Services;

// Маппинг модели формы в контракты служб и обратно
public static class RosterMapping
{
    private const string DateFormat = "yyyy-MM-dd";

    // ---- WCF: DataContract из прокси-класса (Reference.cs) <-> SOAP XML ----

    public static PlayerDto ToWcfDto(this Player m) => new()
    {
        PlayerId = m.PlayerId,
        Jersey = m.Jersey,
        FName = m.FName,
        SName = m.SName,
        Position = m.Position,
        Birthday = m.Birthday is { } b ? DateTime.SpecifyKind(b.Date, DateTimeKind.Unspecified) : null,
        Height = m.Height,
        Weight = m.Weight,
        BirthCity = m.BirthCity,
        BirthState = m.BirthState
    };

    public static Player ToModel(this PlayerDto d) => new()
    {
        PlayerId = d.PlayerId,
        Jersey = d.Jersey,
        FName = d.FName,
        SName = d.SName,
        Position = d.Position,
        Birthday = d.Birthday,
        Height = d.Height,
        Weight = d.Weight,
        BirthCity = d.BirthCity,
        BirthState = d.BirthState
    };

    public static FieldError ToModel(this FieldErrorDto e) => new(e.Field, e.Message);

    // ---- gRPC: protobuf-сообщения. В proto3 строки не бывают null,
    // int? передаётся через google.protobuf.Int32Value, дата — строкой yyyy-MM-dd ----

    public static PlayerMessage ToGrpcMessage(this Player m) => new()
    {
        PlayerId = m.PlayerId ?? "",
        Jersey = m.Jersey,
        Fname = m.FName ?? "",
        Sname = m.SName ?? "",
        Position = m.Position ?? "",
        Birthday = m.Birthday?.ToString(DateFormat, CultureInfo.InvariantCulture) ?? "",
        Height = m.Height,
        Weight = m.Weight,
        BirthCity = m.BirthCity ?? "",
        BirthState = m.BirthState ?? ""
    };

    public static Player ToModel(this PlayerMessage p) => new()
    {
        PlayerId = p.PlayerId,
        Jersey = p.Jersey,
        FName = p.Fname,
        SName = p.Sname,
        Position = p.Position,
        Birthday = DateTime.TryParseExact(p.Birthday, DateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var b) ? b : null,
        Height = p.Height,
        Weight = p.Weight,
        BirthCity = p.BirthCity,
        BirthState = p.BirthState
    };

    public static FieldError ToModel(this CrittersGrpc.FieldError e) => new(e.Field, e.Message);
}
