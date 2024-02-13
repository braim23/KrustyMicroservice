using AutoMapper;
using Krusty.Services.OrderAPI.Models;
using Krusty.Services.OrderAPI.Models.Dto;



namespace Krusty.Services.OrderAPI;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var mappingConfig = new MapperConfiguration(config =>
        {
            config.CreateMap<OrderHeaderDto, CartHeaderDto>()
            .ForMember(dest => dest.CartTotal, u => u.MapFrom(src => src.OrderTotal)).ReverseMap();

            config.CreateMap<CartDetailsDto, OrderDetailsDto>()
            .ForMember(dest => dest.ProductName, u => u.MapFrom(src => src.ProductDto.Name))
            .ForMember(dest => dest.Price, u => u.MapFrom(src => src.ProductDto.Price));

            config.CreateMap<OrderDetailsDto, CartDetailsDto>();

            config.CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();
            config.CreateMap<OrderDetailsDto, OrderDetails>().ReverseMap();

        });
        return mappingConfig;
    }  
}
