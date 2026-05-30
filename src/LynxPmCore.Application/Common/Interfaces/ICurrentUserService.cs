namespace LynxPmCore.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserCode { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
