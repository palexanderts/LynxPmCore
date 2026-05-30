using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Notices;

namespace LynxPmCore.Application.Mappings;

public sealed class NoticeProfile : Profile
{
    public NoticeProfile()
    {
        CreateMap<Notice, NoticeDto>()
            .ForMember(d => d.Operations, o => o.MapFrom(s => s.Operations));
        CreateMap<Notice, NoticeListDto>();
        CreateMap<Operation, OperationDto>();
    }
}
