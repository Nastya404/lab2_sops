namespace CrittersApp.Services;

public enum ServiceTransport { Wcf, Grpc }

// Секция "RosterService" в appsettings.json
public class RosterServiceOptions
{
    public const string Section = "RosterService";

    // Каким способом обращаться к серверу: Wcf (вариант 2.1) или Grpc (вариант 2.2)
    public ServiceTransport Transport { get; set; } = ServiceTransport.Wcf;
    public string WcfUrl { get; set; } = "http://localhost:5100/RosterService.svc";
    public string GrpcUrl { get; set; } = "http://localhost:5200";
    public int TimeoutSeconds { get; set; } = 10;

    public string CurrentUrl => Transport == ServiceTransport.Grpc ? GrpcUrl : WcfUrl;
}
