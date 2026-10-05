using System.ServiceModel;
using CrittersApp.Models;
using CrittersApp.ValidationServiceReference;

namespace CrittersApp.Services;

public class WcfRosterValidationClient : IRosterValidationClient
{
    private readonly string _url;

    public WcfRosterValidationClient(IConfiguration cfg)
        => _url = cfg["Services:WcfValidationUrl"] ?? "http://localhost:5100/ValidationService.svc";

    public async Task<IReadOnlyList<string>> ValidateAsync(Roster m)
    {
        var client = new ValidationServiceClient(
            ValidationServiceClient.EndpointConfiguration.BasicHttpBinding_IValidationService, _url);
        try
        {
            var dto = new RosterDto
            {
                PlayerId = m.PlayerId,
                Jersey = m.Jersey,
                Position = m.Position,
                Birthday = m.Birthday,
                Height = m.Height,
                Weight = m.Weight,
                BirthState = m.BirthState
            };
            string[] errors = await client.ValidateAsync(dto);
            return errors;
        }
        finally
        {
            if (client.State == CommunicationState.Faulted) client.Abort();
            else client.Close();
        }
    }
}