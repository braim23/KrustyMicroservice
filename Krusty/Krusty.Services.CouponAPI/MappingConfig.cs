using AutoMapper;
using Krusty.Services.CouponAPI.Models;
using Krusty.Services.CouponAPI.Models.Dto;

namespace Krusty.Services.CouponAPI;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var mappingConfig = new MapperConfiguration(config =>
        {
            config.CreateMap<CouponDto, Coupon>();
            config.CreateMap<Coupon, CouponDto>();
        });
        return mappingConfig;
    }  
}
