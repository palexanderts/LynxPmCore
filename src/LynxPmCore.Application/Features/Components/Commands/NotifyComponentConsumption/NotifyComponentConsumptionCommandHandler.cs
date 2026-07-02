using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Components;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Components.Commands.NotifyComponentConsumption;

internal sealed class NotifyComponentConsumptionCommandHandler(
    IComponentNotificationRepository repo,
    IUnitOfWork uow) : ICommandHandler<NotifyComponentConsumptionCommand, ComponentNotificationDto>
{
    public async Task<Result<ComponentNotificationDto>> Handle(NotifyComponentConsumptionCommand request, CancellationToken ct)
    {
        var nextId = await repo.GetNextIdAsync(ct);
        var notification = ComponentNotification.Create(
            nextId,
            request.AvisoId,
            request.OperationPosition,
            request.ComponentId,
            request.Quantity,
            request.ConsumedBy,
            request.Observations);

        await repo.AddAsync(notification, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(MapToDto(notification));
    }

    private static ComponentNotificationDto MapToDto(ComponentNotification n) => new()
    {
        Id = n.Id,
        AvisoId = n.AvisoId,
        OperationPosition = n.OperationPosition,
        ProductCode = n.ProductCode,
        Quantity = n.Quantity,
        UserCode = n.UserCode,
        Fecha = n.Fecha,
        Comments = n.Comments
    };
}
