using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Application.Features.Equipments.Queries.GetEquipmentMedia;
using LynxPmCore.Domain.Aggregates.Notices;
using LynxPmCore.Domain.Enums;
using LynxPmCore.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LynxPmCore.Api.Controllers;

/// <summary>
/// Endpoint de compatibilidad ORDS — permite llamar al Core con el mismo patrón
/// que usa la app móvil: POST /Index?Service=GET_AVISOS_CHANGES
/// </summary>
[ApiController]
[Route("Index")]
[AllowAnonymous]
public sealed class IndexController(
    ISender sender,
    INoticeRepository noticeRepo,
    IUnitOfWork unitOfWork,
    ILogger<IndexController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(
        [FromQuery(Name = "Service")] string? service,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonElement? body,
        CancellationToken ct)
    {
        return service?.ToUpperInvariant() switch
        {
            // ── Equipment ──────────────────────────────────────────────────────
            "GET_EQUIPMENT_HISTORY"    => await HandleEquipmentHistory(body, ct),
            "GET_EQUIPMENT_MEDIA"      => await HandleEquipmentMedia(body, ct),

            // ── Connectivity / Auth ────────────────────────────────────────────
            "CONNECTION"               => HandleConnection(),
            "LICENCE"                  => HandleLicence(),
            "LOGIN"                    => HandleLogin(body),
            "GET_USERS"                => Ok(Array.Empty<object>()),
            "SET_STATUS_USER"          => Ok(new OkOrdsResponse()),

            // ── Avisos: lectura ────────────────────────────────────────────────
            "GET_AVISOS_CHANGES"       => await HandleGetAvisosChanges(body, ct),
            "GET_AVISOS_CHANGES_N"     => await HandleGetAvisosChanges(body, ct),
            "GET_AVISO"                => await HandleGetAviso(body, ct),
            "DOWN_LOAD_AVISO"          => await HandleGetAviso(body, ct),
            "GET_POOL_AVISOS"          => await HandleGetPoolAvisos(body, ct),

            // ── Avisos: escritura ─────────────────────────────────────────────
            "SET_AVISO"                => await HandleSetAviso(body, ct),
            "STATUS_AVISO"             => await HandleStatusAviso(body, ct),
            "CIERRE_TECNICO"           => await HandleCierreTecnico(body, ct),
            "POST_INSPECTION"          => await HandleSetAviso(body, ct),

            // ── Órdenes de servicio ───────────────────────────────────────────
            "POST_ORDEN"               => Ok(new OkOrdsResponse()),
            "GET_STATUS_ORDEN"         => Ok(new OkOrdsResponse()),
            "NOTIFICAR_ORDEN"          => Ok(new OkOrdsResponse()),
            "SUPERVISORPERMITREQUEST"  => Ok(new OkOrdsResponse()),

            // ── Componentes / Inventario ──────────────────────────────────────
            "GET_COMPONENTES"          => Ok(Array.Empty<object>()),
            "GET_COMP_STOCK"           => Ok(Array.Empty<object>()),
            "POST_COMP_CONSUMPTION"    => Ok(new OkOrdsResponse()),
            "POST_COMP_RECEIPT"        => Ok(new OkOrdsResponse()),
            "POST_STOCK"               => Ok(new OkOrdsResponse()),
            "MODIFY_COMPONENT"         => Ok(new OkOrdsResponse()),
            "NOTIFY_COMPONENT"         => Ok(new OkOrdsResponse()),

            // ── Traslados ──────────────────────────────────────────────────────
            "GET_SOL_TRASLADOS"        => Ok(Array.Empty<object>()),
            "POST_TRASPASOS"           => Ok(new OkOrdsResponse()),

            // ── Notificaciones / Logs ─────────────────────────────────────────
            "SET_NOTIFICAR"            => Ok(new OkOrdsResponse()),
            "SET_LOG"                  => HandleSetLog(body),
            "SEND_ERRORS"              => HandleSendErrors(body),

            // ── Catálogos ──────────────────────────────────────────────────────
            "GET_CENTERS"              => Ok(Array.Empty<object>()),
            "GET_ROLES"                => Ok(Array.Empty<object>()),
            "GET_ROLESPERMITS"         => Ok(Array.Empty<object>()),
            "GET_TPAVISOS"             => Ok(Array.Empty<object>()),
            "GET_UBICACIONES"          => Ok(Array.Empty<object>()),
            "GET_DEPARTMENTS"          => Ok(Array.Empty<object>()),
            "GET_PERSONS"              => Ok(Array.Empty<object>()),
            "GET_PRIORIDAD"            => Ok(Array.Empty<object>()),
            "GET_RAZONES"              => Ok(Array.Empty<object>()),

            // ── Citas ──────────────────────────────────────────────────────────
            "GET_CITAS"                => Ok(Array.Empty<object>()),
            "POST_CITA"                => Ok(new OkOrdsResponse()),

            _                          => NotFound(new { error = $"Service not found: {service}" })
        };
    }

    // ── Connectivity ─────────────────────────────────────────────────────────────

    private IActionResult HandleConnection() => Ok(new { isOk = true });

    // ── Auth ─────────────────────────────────────────────────────────────────────

    private IActionResult HandleLicence() =>
        Ok(new LicenceOrdsResponse(
            Licence:   "X",
            Type:      "U",
            StartDate: DateTime.UtcNow.ToString("yyyyMMdd"),
            EndDate:   DateTime.UtcNow.AddYears(1).ToString("yyyyMMdd"),
            Logueado:  "X",
            Undefined: ""));

    private IActionResult HandleLogin(JsonElement? body)
    {
        var logon = GetStringProp(body, "Logon") ?? GetStringProp(body, "logon") ?? "UNKNOWN";
        var code  = GetStringProp(body, "SellerCode") ?? logon;
        return Ok(new UsersOrdsResponse(
            Tipo:       "",
            Descripcion:"",
            Type:       "U",
            Logon:      logon,
            Password:   "",
            SellerName: code,
            SellerCode: code,
            Znorol:     1));
    }

    // ── Logs ─────────────────────────────────────────────────────────────────────

    private IActionResult HandleSetLog(JsonElement? body)
    {
        logger.LogInformation("SET_LOG from app: {Body}", body?.ToString());
        return Ok(new OkOrdsResponse());
    }

    private IActionResult HandleSendErrors(JsonElement? body)
    {
        logger.LogWarning("SEND_ERRORS from app: {Body}", body?.ToString());
        return Ok(new OkOrdsResponse());
    }

    // ── Avisos: lectura ───────────────────────────────────────────────────────────

    private async Task<IActionResult> HandleGetAvisosChanges(JsonElement? body, CancellationToken ct)
    {
        var usuario = GetStringProp(body, "USUARIO") ?? GetStringProp(body, "usuario");
        var (notices, _) = await noticeRepo.GetPagedAsync(1, 1000, null, null, usuario, ct);
        return Ok(MapToAvisoRequest(notices));
    }

    private async Task<IActionResult> HandleGetPoolAvisos(JsonElement? body, CancellationToken ct)
    {
        var (notices, _) = await noticeRepo.GetPagedAsync(1, 500, NoticeStatus.Open, null, null, ct);
        return Ok(MapToAvisoRequest(notices));
    }

    private async Task<IActionResult> HandleGetAviso(JsonElement? body, CancellationToken ct)
    {
        var avisoNum = GetStringProp(body, "AVISO") ?? GetStringProp(body, "aviso");
        if (string.IsNullOrWhiteSpace(avisoNum))
            return BadRequest(new ErrorOrdsResponse("AVISO is required"));

        var notice = await noticeRepo.GetByNumberAsync(avisoNum, ct);
        return Ok(MapToAvisoRequest(notice is null ? [] : [notice]));
    }

    // ── Avisos: escritura ─────────────────────────────────────────────────────────

    private async Task<IActionResult> HandleSetAviso(JsonElement? body, CancellationToken ct)
    {
        if (body is null)
            return BadRequest(new ErrorOrdsResponse("Body is required"));

        if (!body.Value.TryGetProperty("HEADER", out var headerEl) || headerEl.ValueKind != JsonValueKind.Array)
            return BadRequest(new ErrorOrdsResponse("HEADER array is required"));

        var responses = new List<AvisoOrdsResponse>();

        foreach (var item in headerEl.EnumerateArray())
        {
            var avisoNum    = GetStringFromEl(item, "AVISO") ?? Guid.NewGuid().ToString("N")[..12];
            var equipo      = GetStringFromEl(item, "EQUIPO") ?? "UNKNOWN";
            var descripcion = GetStringFromEl(item, "DESCRIPCION") ?? "";
            var location    = GetStringFromEl(item, "UBICACION");
            var customer    = GetStringFromEl(item, "CONJUNTO");
            var creador     = GetStringFromEl(item, "CREADOR") ?? "ORDS";
            var statusInt   = GetIntFromEl(item, "STATUS");

            var existing = await noticeRepo.GetByNumberAsync(avisoNum, ct);

            if (existing is null)
            {
                var result = Notice.Create(avisoNum, equipo, descripcion, creador, location, customer, 1);
                if (result.IsSuccess)
                {
                    if (statusInt.HasValue)
                        result.Value.ChangeStatus(MapStatusFromOrd(statusInt.Value));
                    await noticeRepo.AddAsync(result.Value, ct);
                }
                responses.Add(new AvisoOrdsResponse("", "", 0, 0, avisoNum, "", "", "SET"));
            }
            else
            {
                if (statusInt.HasValue)
                {
                    var newStatus = MapStatusFromOrd(statusInt.Value);
                    if (existing.Status != newStatus)
                        existing.ChangeStatus(newStatus);
                }
                await noticeRepo.UpdateAsync(existing, ct);
                responses.Add(new AvisoOrdsResponse("", "", 0, 0, avisoNum, "", existing.ApexId ?? "", "SET"));
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Ok(responses.ToArray());
    }

    private async Task<IActionResult> HandleStatusAviso(JsonElement? body, CancellationToken ct)
    {
        var avisoNum = GetStringProp(body, "AVISO");
        var estatStr = GetStringProp(body, "estat");

        if (string.IsNullOrWhiteSpace(avisoNum))
            return BadRequest(new ErrorOrdsResponse("AVISO is required"));

        var notice = await noticeRepo.GetByNumberAsync(avisoNum, ct);
        if (notice is null)
            return Ok(new[] { new AvisoOrdsResponse("E", "Notice not found", 0, 0, avisoNum, "", "", "STATUS") });

        if (int.TryParse(estatStr, out var estatInt))
        {
            notice.ChangeStatus(MapStatusFromOrd(estatInt));
            await noticeRepo.UpdateAsync(notice, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Ok(new[] { new AvisoOrdsResponse("", "", 0, 0, avisoNum, "", notice.ApexId ?? "", "STATUS") });
    }

    private async Task<IActionResult> HandleCierreTecnico(JsonElement? body, CancellationToken ct)
    {
        var avisoNum = GetStringProp(body, "AVISO");
        if (string.IsNullOrWhiteSpace(avisoNum))
            return BadRequest(new ErrorOrdsResponse("AVISO is required"));

        var notice = await noticeRepo.GetByNumberAsync(avisoNum, ct);
        if (notice is null)
            return Ok(new[] { new AvisoOrdsResponse("E", "Notice not found", 0, 0, avisoNum, "", "", "CLOSE") });

        notice.ChangeStatus(NoticeStatus.Closed);
        await noticeRepo.UpdateAsync(notice, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Ok(new[] { new AvisoOrdsResponse("", "", 0, 0, avisoNum, "", notice.ApexId ?? "", "CLOSE") });
    }

    // ── Equipment handlers ───────────────────────────────────────────────────────

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

        if (!result.IsSuccess)
            return result.Error.Code.EndsWith(".NotFound")
                ? Ok(Array.Empty<EquipmentMediaOrdsResponse>())
                : BadRequest(result.Error);

        return Ok(result.Value.Select(MapToMediaResponse).ToArray());
    }

    // ── Mapping ──────────────────────────────────────────────────────────────────

    private static AvisoRequestOrdsResponse MapToAvisoRequest(IEnumerable<Notice> notices) =>
        new(
            Header:        notices.Select(MapToHeader).ToArray(),
            Items:         [],
            Components:    [],
            Notifications: [],
            OrdStatus:     [],
            Logs:          [],
            Operations:    [],
            Texto:         "");

    private static AvisoHeaderOrdsResponse MapToHeader(Notice n) => new(
        Aviso:       n.Number,
        AvisoSap:    n.ApexId ?? "",
        OrdenSap:    "",
        Equipo:      n.EquipmentCode,
        Descripcion: n.Description ?? "",
        Fecha:       n.CreatedAt.ToString("yyyyMMdd"),
        Hora:        n.CreatedAt.ToString("HHmmss"),
        Creador:     n.CreatedBy,
        Aprobador:   n.ApprovedBy ?? "",
        Supervisor:  n.ApprovedBy ?? "",
        Ubicacion:   n.Location ?? "",
        Prioridad:   n.Priority.ToString(),
        Status:      MapStatus(n.Status),
        FechaCierre: n.Status == NoticeStatus.Closed ? n.UpdatedAt.ToString("yyyyMMdd") : "00000000",
        HoraCierre:  n.Status == NoticeStatus.Closed ? n.UpdatedAt.ToString("HHmmss") : "000000",
        CerradoPor:  n.Status == NoticeStatus.Closed ? (n.ApprovedBy ?? n.CreatedBy) : "");

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
        Executor:    n.ApprovedBy ?? n.CreatedBy);

    private static EquipmentMediaOrdsResponse MapToMediaResponse(EquipmentMediaDto m) => new(
        Id:            m.Id.ToString(),
        EquipmentCode: m.EquipmentCode,
        MediaType:     m.MediaType,
        Url:           m.Url,
        ThumbnailUrl:  m.ThumbnailUrl,
        Title:         m.Title,
        Position:      m.Position,
        CreatedBy:     m.CreatedBy);

    private static int MapStatus(NoticeStatus status) => (int)status;

    private static NoticeStatus MapStatusFromOrd(int status) =>
        Enum.IsDefined(typeof(NoticeStatus), status) ? (NoticeStatus)status : NoticeStatus.Open;

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static string? GetStringProp(JsonElement? body, string key)
    {
        if (body is null) return null;
        return body.Value.TryGetProperty(key, out var el) ? el.GetString() : null;
    }

    private static string? GetStringFromEl(JsonElement el, string key)
        => el.TryGetProperty(key, out var prop) ? prop.GetString() : null;

    private static int? GetIntFromEl(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var prop)) return null;
        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var n)) return n;
        if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var s)) return s;
        return null;
    }
}

// ── Response types ────────────────────────────────────────────────────────────

public sealed record OkOrdsResponse(
    [property: JsonPropertyName("TIPO")]        string Tipo        = "",
    [property: JsonPropertyName("DESCRIPCION")] string Descripcion = "");

public sealed record ErrorOrdsResponse(
    [property: JsonPropertyName("DESCRIPCION")] string Descripcion,
    [property: JsonPropertyName("TIPO")]        string Tipo = "E");

public sealed record LicenceOrdsResponse(
    [property: JsonPropertyName("licence")]    string Licence,
    [property: JsonPropertyName("type")]       string Type,
    [property: JsonPropertyName("start_date")] string StartDate,
    [property: JsonPropertyName("end_date")]   string EndDate,
    [property: JsonPropertyName("Logueado")]   string Logueado,
    [property: JsonPropertyName("undefined")]  string Undefined);

public sealed record UsersOrdsResponse(
    [property: JsonPropertyName("TIPO")]        string Tipo,
    [property: JsonPropertyName("DESCRIPCION")] string Descripcion,
    [property: JsonPropertyName("type")]        string Type,
    [property: JsonPropertyName("Logon")]       string Logon,
    [property: JsonPropertyName("PassWord")]    string Password,
    [property: JsonPropertyName("SellerName")]  string SellerName,
    [property: JsonPropertyName("SellerCode")]  string SellerCode,
    [property: JsonPropertyName("znorol")]      int    Znorol);

public sealed record AvisoOrdsResponse(
    [property: JsonPropertyName("TIPO")]        string Tipo,
    [property: JsonPropertyName("DESCRIPCION")] string Descripcion,
    [property: JsonPropertyName("local_id")]    long   LocalId,
    [property: JsonPropertyName("no")]          long   No,
    [property: JsonPropertyName("AVISOID")]     string AvisoId,
    [property: JsonPropertyName("ORDENID")]     string OrdenId,
    [property: JsonPropertyName("SAPAVISOID")]  string SapAvisoId,
    [property: JsonPropertyName("OPERATION")]   string Operation);

public sealed record AvisoRequestOrdsResponse(
    [property: JsonPropertyName("HEADER")]        AvisoHeaderOrdsResponse[] Header,
    [property: JsonPropertyName("ITEMS")]         object[]                  Items,
    [property: JsonPropertyName("COMPONENTS")]    object[]                  Components,
    [property: JsonPropertyName("NOTIFICATIONS")] object[]                  Notifications,
    [property: JsonPropertyName("ORDSTATUS")]     object[]                  OrdStatus,
    [property: JsonPropertyName("LOGS")]          object[]                  Logs,
    [property: JsonPropertyName("OPERATIONS")]    object[]                  Operations,
    [property: JsonPropertyName("TEXTO")]         string                    Texto);

public sealed record AvisoHeaderOrdsResponse(
    [property: JsonPropertyName("AVISO")]        string Aviso,
    [property: JsonPropertyName("AVISOSAP")]     string AvisoSap,
    [property: JsonPropertyName("ORDENSAP")]     string OrdenSap,
    [property: JsonPropertyName("EQUIPO")]       string Equipo,
    [property: JsonPropertyName("DESCRIPCION")]  string Descripcion,
    [property: JsonPropertyName("FECHA")]        string Fecha,
    [property: JsonPropertyName("HORA")]         string Hora,
    [property: JsonPropertyName("CREADOR")]      string Creador,
    [property: JsonPropertyName("APROBADOR")]    string Aprobador,
    [property: JsonPropertyName("SUPERVISOR")]   string Supervisor,
    [property: JsonPropertyName("UBICACION")]    string Ubicacion,
    [property: JsonPropertyName("PRIORIDAD")]    string Prioridad,
    [property: JsonPropertyName("STATUS")]       int    Status,
    [property: JsonPropertyName("FECHACIERRE")]  string FechaCierre,
    [property: JsonPropertyName("HORACIERRE")]   string HoraCierre,
    [property: JsonPropertyName("CERRADOPOR")]   string CerradoPor);

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
    [property: JsonPropertyName("executor")]     string  Executor);

public sealed record EquipmentMediaOrdsResponse(
    [property: JsonPropertyName("id")]             string  Id,
    [property: JsonPropertyName("equipment_code")] string  EquipmentCode,
    [property: JsonPropertyName("media_type")]     string  MediaType,
    [property: JsonPropertyName("url")]            string  Url,
    [property: JsonPropertyName("thumbnail_url")]  string? ThumbnailUrl,
    [property: JsonPropertyName("title")]          string? Title,
    [property: JsonPropertyName("position")]       int     Position,
    [property: JsonPropertyName("created_by")]     string? CreatedBy);
