using AutoMapper;
using LynxPmCore.Application.DTOs;
using LynxPmCore.Domain.Aggregates.Customers;

namespace LynxPmCore.Application.Mappings;

public sealed class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>();
    }
}
