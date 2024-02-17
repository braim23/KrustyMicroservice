using AutoMapper;
using Krusty.Services.OrderAPI.Data;
using Krusty.Services.OrderAPI.Models.Dto;
using Krusty.Services.ShoppingCartAPI.Service.IService;
using Krusty.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Krusty.Services.OrderAPI.Models;
using Stripe;
using Stripe.Checkout;

namespace Krusty.Services.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderAPIController : ControllerBase
{
    protected ResponseDto _response;
    private IMapper _mapper;
    private readonly AppDbContext _dbContext;
    private IProductService _productService;

    public OrderAPIController(

        IMapper mapper,
        AppDbContext dbContext,
        IProductService productService)
    {
        _response = new ResponseDto();
        _mapper = mapper;
        _dbContext = dbContext;
        _productService = productService;
    }
    [Authorize]
    [HttpPost("CreateOrder")]
    public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
    {
        try
        {
            OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeaderDto);
            orderHeaderDto.OrderTime = DateTime.Now;
            orderHeaderDto.Status = SD.Status_Pending;
            orderHeaderDto.OrderDetailsDto = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetailsDto);
            OrderHeader orderCreated = _dbContext.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
            await _dbContext.SaveChangesAsync();

            orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
            _response.Result = orderHeaderDto;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }
        return _response;
    }

    [Authorize]
    [HttpPost("CreateStripeSession")]
    public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
    {
        try
        {

            var options = new SessionCreateOptions
            {
                SuccessUrl = stripeRequestDto.ApprovedUrl,
                CancelUrl = stripeRequestDto.CancelUrl,
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            var discountsObj = new List<SessionDiscountOptions>()
            {
                new SessionDiscountOptions
                {
                    Coupon = stripeRequestDto.OrderHeaderDto.CouponCode
                }
            };

            foreach (var item in stripeRequestDto.OrderHeaderDto.OrderDetailsDto)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $10.99 = 1099
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            if (stripeRequestDto.OrderHeaderDto.Discount > 0)
            {
                options.Discounts = discountsObj;
            }
            var service = new SessionService();
            Session session = service.Create(options);
            stripeRequestDto.StripeSessionUrl = session.Url;
            OrderHeader orderHeader = _dbContext.OrderHeaders.First(u => u.OrderHeaderId == stripeRequestDto.OrderHeaderDto.OrderHeaderId);
            orderHeader.StripeSessionId = session.Id;
            await _dbContext.SaveChangesAsync();
            _response.Result = stripeRequestDto;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }
        return _response;
    }
    [Authorize]
    [HttpPost("CreateStripeSession")]
    public async Task<ResponseDto> CreateStripeSession([FromBody] int orderHeaderId)
    {
        try
        {
            OrderHeader orderHeader = _dbContext.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);
            
            var service = new SessionService();
            Session session = service.Get(orderHeader.StripeSessionId);

            var paymentIntentService= new PaymentIntentService();
            PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

            if(paymentIntent.Status == "succeeded")
            {
                // payment is successful so..
                orderHeader.PaymentIntentId = paymentIntent.Id;
                orderHeader.Status = SD.Status_Approved;
                _dbContext.SaveChanges();

                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
            }
            
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }
        return _response;
    }

}
