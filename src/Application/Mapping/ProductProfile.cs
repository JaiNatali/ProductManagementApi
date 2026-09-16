using Application.DTOs.Product;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponse>();
        CreateMap<Product, ProductDetailsResponse>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
        CreateMap<Item, ProductItemDto>();
        CreateMap<Product, ProductSummaryResponse>();
        CreateMap<CreateProductRequest, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.CreatedOn, opt => opt.Ignore())
            .ForMember(d => d.ModifiedBy, opt => opt.Ignore())
            .ForMember(d => d.ModifiedOn, opt => opt.Ignore())
            .ForMember(d => d.Items, opt => opt.Ignore());
        CreateMap<UpdateProductRequest, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.CreatedOn, opt => opt.Ignore())
            .ForMember(d => d.ModifiedBy, opt => opt.Ignore())
            .ForMember(d => d.ModifiedOn, opt => opt.Ignore())
            .ForMember(d => d.Items, opt => opt.Ignore());
    }
}
