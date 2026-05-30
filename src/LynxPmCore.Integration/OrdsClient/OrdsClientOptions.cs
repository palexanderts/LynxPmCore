namespace LynxPmCore.Integration.OrdsClient;

public sealed class OrdsClientOptions
{
    public const string SectionName = "Ords";
    public string BaseUrl { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 30;
}
