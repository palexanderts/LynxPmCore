using LynxPmCore.Application;
using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Mcp.Services;
using LynxPmCore.Mcp.Tools;
using LynxPmCore.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Application & Persistence layers ─────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

// ── MCP-specific service implementations ─────────────────────────────────────
builder.Services.AddScoped<ICurrentUserService, McpUserService>();
builder.Services.AddScoped<INotificationService, NoopNotificationService>();

// ── MCP Server ────────────────────────────────────────────────────────────────
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<NoticeTools>()
    .WithTools<ApprovalTools>()
    .WithTools<EquipmentTools>()
    .WithTools<AnalyticsTools>();

var app = builder.Build();

app.MapMcp("/mcp");

app.MapGet("/", () => new
{
    name = "LynxPM MCP Server",
    version = "1.0.0",
    mcp_endpoint = "/mcp",
    tools = new[]
    {
        "list_notices", "get_notice", "create_notice", "change_notice_status",
        "approve_notice", "reject_notice",
        "list_equipment", "get_equipment_media",
        "get_dashboard_kpis", "get_top_failing_equipment"
    },
    connect = new
    {
        claude_desktop = "Add to claude_desktop_config.json: { \"lynxpm\": { \"url\": \"http://localhost:5010/mcp\", \"transport\": \"http\" } }",
        cursor = "Settings → MCP Servers → Add → URL: http://localhost:5010/mcp",
        vscode = "Add to .vscode/mcp.json: { \"servers\": { \"lynxpm\": { \"url\": \"http://localhost:5010/mcp\" } } }"
    }
});

app.Run();
