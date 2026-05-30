using Asp.Versioning;
using Hangfire;
using LynxPmCore.Api.Configuration;
using LynxPmCore.Api.Middleware;
using LynxPmCore.Application;
using LynxPmCore.BackgroundJobs;
using LynxPmCore.Infrastructure;
using LynxPmCore.Integration;
using LynxPmCore.Persistence;
using LynxPmCore.Shared.Context;
using LynxPmCore.SignalR;
using LynxPmCore.SignalR.Hubs;
using Serilog;

// ── 1. Resolve client and environment before builder ──────────────────────────
var (lynxClient, lynxEnv) = LynxConfigurationBootstrap.ReadFromEnv();

// Align ASPNETCORE_ENVIRONMENT so IsDevelopment() / IProduction() work
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", lynxEnv);

var builder = WebApplication.CreateBuilder(args);

// ── 2. Load layered configuration ─────────────────────────────────────────────
builder.AddLynxConfiguration(lynxClient, lynxEnv);

// ── 3. Serilog (reads Serilog section from merged config) ─────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Client", lynxClient)
    .Enrich.WithProperty("LynxEnvironment", lynxEnv)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{Client}:{LynxEnvironment}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File($"logs/{lynxClient}/lynxpm-.log", rollingInterval: RollingInterval.Day));

// ── 4. Client context service ─────────────────────────────────────────────────
builder.Services.AddLynxClientContext(lynxClient, lynxEnv);

// ── 5. Application layers ─────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIntegration(builder.Configuration);
builder.Services.AddBackgroundJobs();
builder.Services.AddSignalRServices();

// ── 6. API versioning ─────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(opts =>
{
    opts.DefaultApiVersion = new ApiVersion(1);
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.ReportApiVersions = true;
}).AddApiExplorer(opts =>
{
    opts.GroupNameFormat = "'v'VVV";
    opts.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new()
    {
        Title = $"LynxPmCore API — {lynxClient.ToUpperInvariant()}",
        Version = "v1",
        Description = $"Client: **{lynxClient}** | Environment: **{lynxEnv}**"
    });
    opts.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    opts.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            []
        }
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(opts =>
    opts.AddPolicy("fixed", _ =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            "global",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10)
            })));

// ── 7. Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── 8. Startup banner ─────────────────────────────────────────────────────────
var startupLog = app.Services.GetRequiredService<ILogger<Program>>();
startupLog.LogInformation("╔══════════════════════════════════════╗");
startupLog.LogInformation("║       LynxPmCore API starting        ║");
startupLog.LogInformation("║  Client : {Client,-26} ║", lynxClient.ToUpperInvariant());
startupLog.LogInformation("║  Env    : {Env,-26} ║", lynxEnv);
startupLog.LogInformation("╚══════════════════════════════════════╝");

// ── 9. Middleware pipeline ────────────────────────────────────────────────────
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(opts =>
{
    opts.SwaggerEndpoint("/swagger/v1/swagger.json", $"LynxPmCore v1 [{lynxClient}]");
    opts.RoutePrefix = string.Empty;
    opts.DocumentTitle = $"LynxPmCore — {lynxClient.ToUpperInvariant()}";
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/info", (ILynxClientContext ctx) => new
{
    client = ctx.Client,
    environment = ctx.Environment,
    isDevelopment = ctx.IsDevelopment,
    version = "1.0.0"
});
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SynchronizationHub>("/hubs/sync");

LynxPmCore.BackgroundJobs.DependencyInjection.RegisterRecurringJobs();

app.Run();
