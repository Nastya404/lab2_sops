using CrittersGrpc.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
    o.ListenAnyIP(5200, l => l.Protocols = HttpProtocols.Http2));

builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<RosterValidationService>();
app.MapGet("/", () => "gRPC service. Use a gRPC client.");
app.Run();