using LynxPmCore.Application.DTOs;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentMedia;
using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LynxPmCore.Api.Controllers;

/// <summary>
/// Endpoint de compatibilidad ORDS — permite llamar al Core con el mismo patrón
/// que usa la app móvil: POST /Index?Service=GET_EQUIPMENT_HISTORY
/// </summary>
[ApiController]
[Route("Index")]
[AllowAnonymous]
public sealed class IndexController(ISender sender, INoticeRepository noticeRepo) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(
        [FromQuery(Name = "Service")] string? service,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonElement? body,
        CancellationToken ct)
    {
        return service?.ToUpperInvariant() switch
        {
            "GET_EQUIPMENT_HISTORY" => await HandleEquipmentHistory(body, ct),
            "GET_EQUIPMENT_MEDIA"   => await HandleEquipmentMedia(body, ct),
            _ => NotFound(new { error = $"Service not found: {service}" })
        };
    }

    // ── Handlers ────────────────────────────────────────────────────────────────

    private async Task<IActionResult> HandleEquipmentHistory(JsonElement? body, CancellationToken ct)
    {
        var code = GetStringProp(body, "equipment_code");
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new { error = "equipment_code is required" });

        var (notices, _) = await noticeRepo.GetPagedAsync(1, 500, null, code, null, ct);
        return Ok(notices.Select(MapToHistoryResponse).ToArray());
    }

    private async Task<IActionResult> HandleEquipmentMedia(JsonElement? body, CancellationToken ct)
    {
        var code = GetStringProp(body, "equipment_code");
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new { error = "equipment_code is required" });

        var result = await sender.Send(new GetEquipmentMediaQuery(code), ct);

        // Si el equipo no tiene media, devolver array vacío (no error)
        if (!result.IsSuccess)
            return result.Error.Code.EndsWith(".NotFound")
                ? Ok(Array.Empty<EquipmentMediaOrdsResponse>())
                : BadRequest(result.Error);

        return Ok(result.Value.Select(MapToMediaResponse).ToArray());
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static string? GetStringProp(JsonElement? body, string key)
    {
        if (body is null) return null;
        return body.Value.TryGetProperty(key, out var el) ? el.GetString() : null;
    }

    private static EquipmentHistoryOrdsResponse MapToHistoryResponse(Notice n) => new(
        Id:          long.TryParse(n.ApexId, out var id) ? id : 0,
        AvisoId:     n.Number,
        Description: n.Description ?? "",
        Status:      MapStatus(n.Status),
        CreatedDate: n.CreatedAt.ToString("yyyyMMdd"),
        CreatedTime: n.CreatedAt.ToString("HHmmss"),
        CloseDate:   n.Status == NoticeStatus.Closed ? n.UpdatedAt.ToString("yyyyMMdd") : "00000000",
        CloseTime:   n.Status == NoticeStatus.Closed ? n.UpdatedAt.ToString("HHmmss") : "000000",
        Usuario:     n.CreatedBy,
        CheckBy:     n.ApprovedBy ?? "",
        Supervisor:  n.ApprovedBy ?? "",
        Tipo:        "",
        Comments:    n.Description ?? "",
        Operations:  n.Operations.Count,
        Executor:    n.ApprovedBy ?? n.CreatedBy
    );

    private static EquipmentMediaOrdsResponse MapToMediaResponse(EquipmentMediaDto m) => new(
        Id:            m.Id.ToString(),
        EquipmentCode: m.EquipmentCode,
        MediaType:     m.MediaType,
        Url:           m.Url,
        ThumbnailUrl:  m.ThumbnailUrl,
        Title:         m.Title,
        Position:      m.Position,
        CreatedBy:     m.CreatedBy
    );

    private static int MapStatus(NoticeStatus status) => status switch
    {
        NoticeStatus.Open       => 0,
        NoticeStatus.InProgress => 3,
        NoticeStatus.Paused     => 3,
        NoticeStatus.Completed  => 5,
        NoticeStatus.Closed     => 4,
        NoticeStatus.Cancelled  => 2,
        _                       => 0
    };
}

// ── Response types ORDS-compatible (snake_case) ──────────────────────────────

public sealed record EquipmentHistoryOrdsResponse(
    [property: JsonPropertyName("id")]           long    Id,
    [property: JsonPropertyName("aviso_id")]     string  AvisoId,
    [property: JsonPropertyName("description")]  string? Description,
    [property: JsonPropertyName("status")]       int     Status,
    [property: JsonPropertyName("created_date")] string  CreatedDate,
    [property: JsonPropertyName("created_time")] string  CreatedTime,
    [property: JsonPropertyName("close_date")]   string  CloseDate,
    [property: JsonPropertyName("close_time")]   string  CloseTime,
    [property: JsonPropertyName("usuario")]      string  Usuario,
    [property: JsonPropertyName("checkby")]      string  CheckBy,
    [property: JsonPropertyName("supervisor")]   string  Supervisor,
    [property: JsonPropertyName("tipo")]         string  Tipo,
    [property: JsonPropertyName("comments")]     string  Comments,
    [property: JsonPropertyName("operations")]   int     Operations,
    [property: JsonPropertyName("executor")]     string  Executor
);

public sealed record EquipmentMediaOrdsResponse(
    [property: JsonPropertyName("id")]             string  Id,
    [property: JsonPropertyName("equipment_code")] string  EquipmentCode,
    [property: JsonPropertyName("media_type")]     string  MediaType,
    [property: JsonPropertyName("url")]            string  Url,
    [property: JsonPropertyName("thumbnail_url")]  string? ThumbnailUrl,
    [property: JsonPropertyName("title")]          string? Title,
    [property: JsonPropertyName("position")]       int     Position,
    [property: JsonPropertyName("created_by")]     string? CreatedBy
);
