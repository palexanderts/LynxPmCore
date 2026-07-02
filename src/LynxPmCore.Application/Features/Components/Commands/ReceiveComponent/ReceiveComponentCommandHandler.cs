using LynxPmCore.Application.Common.Interfaces;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Components;
using LynxPmCore.Domain.Repositories;
using LynxPmCore.Shared.Abstractions;
using LynxPmCore.Shared.Common;

namespace LynxPmCore.Application.Features.Components.Commands.ReceiveComponent;

internal sealed class ReceiveComponentCommandHandler : ICommandHandler<ReceiveComponentCommand, ComponentReceiptDto>
{
    private readonly IComponentReceiptRepository _repo;
    private readonly IUnitOfWork _uow;

    public ReceiveComponentCommandHandler(IComponentReceiptRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<ComponentReceiptDto>> Handle(ReceiveComponentCommand request, CancellationToken ct)
    {
        var existing = await _repo.FindDuplicateAsync(request.ComponentId, request.ReceivedAt, request.ReceivedBy, ct);
        if (existing is not null)
            return Result.Success(MapToDto(existing));

        var receipt = ComponentReceipt.Create(
            request.ComponentId,
            request.Quantity,
            request.Observations,
            request.ReceivedBy,
            request.ReceivedAt);

        await _repo.AddAsync(receipt, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Success(MapToDto(receipt));
    }

    private static ComponentReceiptDto MapToDto(ComponentReceipt r) => new()
    {
        ReceiptId = r.ReceiptId,
        ComponentId = r.ComponentId,
        Quantity = r.Quantity,
        Observations = r.Observations,
        ReceivedBy = r.ReceivedBy,
        ReceivedAt = r.ReceivedAt
    };
}
