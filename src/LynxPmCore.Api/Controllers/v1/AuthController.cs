using Asp.Versioning;
using LynxPmCore.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(JwtService jwtService) : ControllerBase
{
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] LoginRequest request)
    {
        // Simplified — in production, validate against Oracle APEX login endpoint
        if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("UserCode and Password are required.");

        var token = jwtService.GenerateToken(
            Guid.NewGuid().ToString(),
            request.UserCode,
            request.UserCode,
            ["User"]);

        return Ok(new { token, expiresIn = 3600 });
    }
}

public sealed record LoginRequest(string UserCode, string Password);
