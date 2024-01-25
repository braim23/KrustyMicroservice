using AutoMapper;
using Krusty.Services.ProductAPI.Models;
using Krusty.Services.ProductAPI.Models.Dto;


namespace Krusty.Services.ProductAPI;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var mappingConfig = new MapperConfiguration(config =>
        {
            config.CreateMap<ProductDto, Product>();
            config.CreateMap<Product, ProductDto>();
        });
        return mappingConfig;
    }  
}
