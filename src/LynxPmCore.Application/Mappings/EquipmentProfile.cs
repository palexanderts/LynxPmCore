using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Equipments;

namespace LynxPmCore.Application.Mappings;

public sealed class EquipmentProfile : Profile
{
    public EquipmentProfile()
    {
        CreateMap<Equipment, EquipmentDto>();
    }
}
