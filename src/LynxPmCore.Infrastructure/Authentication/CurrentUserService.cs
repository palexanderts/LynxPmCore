using System.Security.Claims;
using LynxPmCore.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LynxPmCore.Infrastructure.Authentication;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserCode => User?.FindFirstValue("userCode");
    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
