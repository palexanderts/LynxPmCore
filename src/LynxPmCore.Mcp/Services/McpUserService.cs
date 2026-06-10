using LynxPmCore.Application.Common.Interfaces;

namespace LynxPmCore.Mcp.Services;

internal sealed class McpUserService : ICurrentUserService
{
    public string? UserId => "mcp-agent";
    public string? UserCode => "MCP_AGENT";
    public string? UserName => "MCP Agent";
    public bool IsAuthenticated => true;
}
