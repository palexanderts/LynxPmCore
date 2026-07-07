using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Notices;

namespace LynxPmCore.Application.Mappings;

public sealed class NoticeProfile : Profile
{
    public NoticeProfile()
    {
        CreateMap<Notice, NoticeDto>()
            .ForMember(d => d.Operations, o => o.MapFrom(s => s.Operations))
            .ForMember(d => d.Causes, o => o.MapFrom(s => s.Causes));
        CreateMap<Notice, NoticeListDto>();
        CreateMap<Operation, OperationDto>()
            .ForMember(d => d.Parts, o => o.MapFrom(s => s.Parts));
        CreateMap<NoticeCause, NoticeCauseDto>();
        CreateMap<OperationPart, OperationPartDto>();
    }
}
