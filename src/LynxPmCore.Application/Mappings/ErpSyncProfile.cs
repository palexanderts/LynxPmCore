using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.ErpSync;

namespace LynxPmCore.Application.Mappings;

public sealed class ErpSyncProfile : Profile
{
    public ErpSyncProfile()
    {
        CreateMap<ErpSyncConfig, ErpSyncConfigDto>();
        CreateMap<ErpSyncOutboxEntry, ErpSyncOutboxDto>();
    }
}
