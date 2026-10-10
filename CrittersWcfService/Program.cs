using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CrittersWcfService;

var builder = WebApplication.CreateBuilder(args);

// Адрес прослушивания берётся из appsettings.json ("Urls"), по умолчанию все интерфейсы,
// чтобы к службе можно было обратиться с другого компьютера.
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://0.0.0.0:5100");

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
// В WSDL подставляется хост из запроса (IP сервера), а не localhost
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<ValidationService>();
    // BasicHttpBinding — SOAP 1.1 поверх HTTP
    serviceBuilder.AddServiceEndpoint<ValidationService, IValidationService>(
        new BasicHttpBinding(), "/ValidationService.svc");

    var metadata = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadata.HttpGetEnabled = true;
});

app.Run();
