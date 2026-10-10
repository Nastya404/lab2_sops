using CrittersApp.Services;
using CrittersApp.Services.Grpc;
using CrittersApp.Services.Wcf;
using CrittersGrpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Клиент не хранит данные: всё читается и сохраняется через службу на сервере.
// Способ обращения задаётся RosterService:Transport = Wcf | Grpc.
var serviceSection = builder.Configuration.GetSection(RosterServiceOptions.Section);
builder.Services.Configure<RosterServiceOptions>(serviceSection);
var service = serviceSection.Get<RosterServiceOptions>() ?? new RosterServiceOptions();

if (service.Transport == ServiceTransport.Grpc)
{
    // Вариант 2.2: gRPC (HTTP/2 без TLS)
    builder.Services.AddGrpcClient<Roster.RosterClient>(o => o.Address = new Uri(service.GrpcUrl));
    builder.Services.AddScoped<IRosterClient, GrpcRosterClient>();
}
else
{
    // Вариант 2.1: WCF, SOAP (BasicHttpBinding) через прокси-класс
    builder.Services.AddScoped<IRosterClient, WcfRosterClient>();
}

var app = builder.Build();

app.Logger.LogInformation("Сервер: {Transport} ({Url})", service.Transport, service.CurrentUrl);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Roster}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
