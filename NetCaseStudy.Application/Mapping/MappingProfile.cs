using AutoMapper;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductRequest, Product>();
    }
}