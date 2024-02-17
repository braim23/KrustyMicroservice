﻿using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Krusty.Web.Utility;

namespace Krusty.Web.Service;

public class OrderService : IOrderService
{
    private readonly IBaseService _baseService;
    public OrderService(IBaseService baseService)
    {
        _baseService = baseService;
    }

    public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = cartDto,
            Url = SD.OrderAPIBase + "/api/order/CreateOrder"
        });
    }

    public async Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = stripeRequestDto,
            Url = SD.OrderAPIBase + "/api/order/CreateStripeSession"
        });
    }

    


}
