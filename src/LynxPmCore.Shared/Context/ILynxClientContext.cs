namespace LynxPmCore.Shared.Context;

public interface ILynxClientContext
{
    string Client { get; }
    string Environment { get; }
    bool IsDevelopment { get; }
    bool IsProduction { get; }
}
