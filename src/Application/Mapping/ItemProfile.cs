using Application.DTOs.Item;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Item, ItemResponse>();
        CreateMap<Item, ItemSummaryResponse>();
        CreateMap<CreateItemRequest, Item>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ProductId, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore());
        CreateMap<UpdateItemRequest, Item>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ProductId, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore());
    }
}
