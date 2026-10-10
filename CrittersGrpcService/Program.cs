using CrittersGrpc.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// gRPC без TLS требует HTTP/2 (h2c). Слушаем все интерфейсы, чтобы служба была доступна по сети.
var port = builder.Configuration.GetValue("GrpcPort", 5200);
builder.WebHost.ConfigureKestrel(o =>
    o.ListenAnyIP(port, l => l.Protocols = HttpProtocols.Http2));

builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<RosterValidationService>();
app.MapGet("/", () => "gRPC service. Use a gRPC client.");
app.Run();
