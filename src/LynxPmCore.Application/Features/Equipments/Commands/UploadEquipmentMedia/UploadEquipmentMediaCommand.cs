using LynxPmCore.Application.DTOs;
using LynxPmCore.Shared.Abstractions;

namespace LynxPmCore.Application.Features.Equipments.Commands.UploadEquipmentMedia;

public sealed record UploadEquipmentMediaCommand(
    string EquipmentCode,
    Stream FileStream,
    string FileName,
    string ContentType,
    string MediaType,
    string? Title = null,
    int Position = 0) : ICommand<EquipmentMediaDto>, ITransactional;
