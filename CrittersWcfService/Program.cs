using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Critters.Shared;
using Critters.Shared.Data;
using CrittersWcfService;

var builder = WebApplication.CreateBuilder(args);

var urls = builder.Configuration["Urls"] ?? "http://0.0.0.0:5100";
builder.WebHost.UseUrls(urls);

builder.Services.AddCrittersData(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<RosterService>();
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

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
