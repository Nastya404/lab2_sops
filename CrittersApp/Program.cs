using Microsoft.EntityFrameworkCore;
using CrittersApp.Data;
using CrittersApp.Services;
using CrittersApp.Services.Grpc;
using CrittersApp.Services.Wcf;
using CrittersGrpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Путь к БД — относительно папки проекта, а не текущего каталога процесса
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "CRITTERS.db");
builder.Services.AddDbContext<CrittersContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Critters") ?? $"Data Source={dbPath}"));

// Выбор способа вызова службы проверки правил (Validation:Transport = Wcf | Grpc)
var validationSection = builder.Configuration.GetSection(ValidationOptions.Section);
builder.Services.Configure<ValidationOptions>(validationSection);
var validation = validationSection.Get<ValidationOptions>() ?? new ValidationOptions();

if (validation.Transport == ValidationTransport.Grpc)
{
    // Вариант 2.2: gRPC (HTTP/2 без TLS)
    builder.Services.AddGrpcClient<RosterValidation.RosterValidationClient>(o =>
        o.Address = new Uri(validation.GrpcUrl));
    builder.Services.AddScoped<IRosterValidationClient, GrpcRosterValidationClient>();
}
else
{
    // Вариант 2.1: WCF, SOAP (BasicHttpBinding) через прокси-класс
    builder.Services.AddScoped<IRosterValidationClient, WcfRosterValidationClient>();
}

var app = builder.Build();

app.Logger.LogInformation("Служба проверки правил: {Transport} ({Url})",
    validation.Transport, validation.CurrentUrl);

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
