using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CrittersWcfService;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5100");

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<ValidationService>();
    serviceBuilder.AddServiceEndpoint<ValidationService, IValidationService>(
        new BasicHttpBinding(), "/ValidationService.svc");

    var metadata = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadata.HttpGetEnabled = true;
});

app.Run();