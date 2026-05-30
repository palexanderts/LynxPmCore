using LynxPmCore.Shared.Context;

namespace LynxPmCore.Infrastructure.Context;

public sealed class LynxClientContext : ILynxClientContext
{
    public LynxClientContext(string client, string environment)
    {
        Client = client;
        Environment = environment;
    }

    public string Client { get; }
    public string Environment { get; }
    public bool IsDevelopment => Environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
    public bool IsProduction => Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
}
