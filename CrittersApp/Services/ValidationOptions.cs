namespace CrittersApp.Services;

public enum ValidationTransport { Wcf, Grpc }

// Секция "Validation" в appsettings.json
public class ValidationOptions
{
    public const string Section = "Validation";

    // Каким способом вызывать службу проверки правил: Wcf (вариант 2.1) или Grpc (вариант 2.2)
    public ValidationTransport Transport { get; set; } = ValidationTransport.Wcf;
    public string WcfUrl { get; set; } = "http://localhost:5100/ValidationService.svc";
    public string GrpcUrl { get; set; } = "http://localhost:5200";
    public int TimeoutSeconds { get; set; } = 10;

    public string CurrentUrl => Transport == ValidationTransport.Grpc ? GrpcUrl : WcfUrl;
}
