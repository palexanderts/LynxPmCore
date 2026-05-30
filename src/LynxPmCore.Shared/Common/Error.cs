namespace LynxPmCore.Shared.Common;

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("General.Null", "A null value was provided.");
    public static Error NotFound(string resource) => new($"{resource}.NotFound", $"{resource} was not found.");
    public static Error Conflict(string resource) => new($"{resource}.Conflict", $"{resource} conflict.");
    public static Error Validation(string field, string message) => new($"Validation.{field}", message);
}
