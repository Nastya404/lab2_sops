using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Critters.Shared;
using Critters.Shared.Data;
using CrittersWcfService;

var builder = WebApplication.CreateBuilder(args);

// Адрес прослушивания берётся из appsettings.json ("Urls"), по умолчанию все интерфейсы,
// чтобы к службе можно было обратиться с другого компьютера.
var urls = builder.Configuration["Urls"] ?? "http://0.0.0.0:5100";
builder.WebHost.UseUrls(urls);

// БД на сервере (путь — "Database" в appsettings.json)
builder.Services.AddCrittersData(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddHttpContextAccessor();
// Служба создаётся через DI, чтобы получить хранилище и ILogger
builder.Services.AddTransient<RosterService>();
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
// В WSDL подставляется хост из запроса (IP сервера), а не localhost
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

// Журнал всех HTTP-запросов к службе (WSDL, SOAP): кто, что, с каким результатом
app.Use(async (context, next) =>
{
    await next();
    app.Logger.LogInformation("HTTP {Method} {Path}{Query} от {Client} -> {Status}",
        context.Request.Method, context.Request.Path, context.Request.QueryString,
        NetworkInfo.Format(context.Connection.RemoteIpAddress), context.Response.StatusCode);
});

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<RosterService>();
    // BasicHttpBinding — SOAP 1.1 поверх HTTP
    serviceBuilder.AddServiceEndpoint<RosterService, IRosterService>(
        new BasicHttpBinding(), "/RosterService.svc");

    var metadata = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadata.HttpGetEnabled = true;
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("БД: {Path}",
        CrittersDataExtensions.GetDatabasePath(builder.Configuration, builder.Environment.ContentRootPath));
    var port = new Uri(urls.Replace("*", "localhost").Replace("+", "localhost")).Port;
    foreach (var ip in NetworkInfo.GetLocalIPv4())
        app.Logger.LogInformation("WCF-служба доступна по адресу: http://{Ip}:{Port}/RosterService.svc", ip, port);
});

app.Run();
