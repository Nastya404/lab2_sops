using Critters.Shared;
using Critters.Shared.Data;
using CrittersGrpc.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// gRPC без TLS требует HTTP/2 (h2c). Слушаем все интерфейсы, чтобы служба была доступна по сети.
var port = builder.Configuration.GetValue("GrpcPort", 5200);
builder.WebHost.ConfigureKestrel(o =>
    o.ListenAnyIP(port, l => l.Protocols = HttpProtocols.Http2));

// БД на сервере (путь — "Database" в appsettings.json)
builder.Services.AddCrittersData(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<RosterGrpcService>();
app.MapGet("/", () => "gRPC service. Use a gRPC client.");

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("БД: {Path}",
        CrittersDataExtensions.GetDatabasePath(builder.Configuration, builder.Environment.ContentRootPath));
    foreach (var ip in NetworkInfo.GetLocalIPv4())
        app.Logger.LogInformation("gRPC-служба доступна по адресу: http://{Ip}:{Port}", ip, port);
});

app.Run();
