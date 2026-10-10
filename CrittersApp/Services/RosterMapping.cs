using System.Globalization;
using CrittersApp.Models;
using CrittersApp.ValidationServiceReference;
using CrittersGrpc;

namespace CrittersApp.Services;

// Маппинг модели EF (таблица roster) в контракты служб.
// Передаются только поля, нужные для проверки правил (имя и город службе не нужны).
public static class RosterMapping
{
    // WCF: DataContract из прокси-класса (Reference.cs) -> SOAP XML
    public static RosterDto ToWcfDto(this Roster m) => new()
    {
        PlayerId = m.PlayerId,
        Jersey = m.Jersey,
        Position = m.Position,
        Birthday = DateTime.SpecifyKind(m.Birthday.Date, DateTimeKind.Unspecified),
        Height = m.Height,
        Weight = m.Weight,
        BirthState = m.BirthState
    };

    // gRPC: protobuf-сообщение. В proto3 строки не бывают null,
    // int? передаётся через google.protobuf.Int32Value, дата — строкой yyyy-MM-dd.
    public static RosterRequest ToGrpcRequest(this Roster m) => new()
    {
        PlayerId = m.PlayerId ?? "",
        Jersey = m.Jersey,
        Position = m.Position ?? "",
        Birthday = m.Birthday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        Height = m.Height,
        Weight = m.Weight,
        BirthState = m.BirthState ?? ""
    };
}
