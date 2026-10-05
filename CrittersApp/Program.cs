using Microsoft.EntityFrameworkCore;
using CrittersApp.Data;
using CrittersApp.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<CrittersContext>(options =>
    options.UseSqlite("Data Source=CRITTERS.db"));
builder.Services.AddScoped<IRosterValidationClient, WcfRosterValidationClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
