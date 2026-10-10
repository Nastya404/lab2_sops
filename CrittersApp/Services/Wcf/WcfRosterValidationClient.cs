using CrittersApp.Models;
using CrittersApp.ValidationServiceReference;
using Microsoft.Extensions.Options;

namespace CrittersApp.Services.Wcf;

public class WcfRosterValidationClient : IRosterValidationClient
{
    private readonly ValidationOptions _options;

    public WcfRosterValidationClient(IOptions<ValidationOptions> options)
        => _options = options.Value;

    public async Task<IReadOnlyList<string>> ValidateAsync(Roster m)
    {
        // Прокси-класс, сгенерированный dotnet-svcutil по WSDL; адрес службы — из конфигурации
        var client = new ValidationServiceClient(
            ValidationServiceClient.EndpointConfiguration.BasicHttpBinding_IValidationService, _options.WcfUrl);
        var timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        client.Endpoint.Binding.OpenTimeout = timeout;
        client.Endpoint.Binding.SendTimeout = timeout;
        try
        {
            string[] errors = await client.ValidateAsync(m.ToWcfDto());
            client.Close();
            return errors ?? [];
        }
        catch
        {
            client.Abort();
            throw;
        }
    }
}
