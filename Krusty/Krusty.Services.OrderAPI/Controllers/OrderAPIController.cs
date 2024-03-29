﻿using AutoMapper;
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
using Krusty.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace Krusty.Services.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderAPIController : ControllerBase
{
    protected ResponseDto _response;
    private IMapper _mapper;
    private readonly AppDbContext _dbContext;
    private IProductService _productService;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;

    public OrderAPIController(

        IMapper mapper,
        AppDbContext dbContext,
        IProductService productService,
        IMessageBus messageBus,
        IConfiguration configuration)
    {
        _response = new ResponseDto();
        _mapper = mapper;
        _dbContext = dbContext;
        _productService = productService;
        _messageBus = messageBus;
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet("GetOrders")]
    public ResponseDto? Get(string? userId = "")
    {
        try
        {
            IEnumerable<OrderHeader> objList;
            if (User.IsInRole(SD.RoleAdmin))
            {
                objList = _dbContext.OrderHeaders.Include(u => u.OrderDetailsDto).OrderByDescending(u => u.OrderHeaderId).ToList();
            }
            else
            {
                objList = _dbContext.OrderHeaders.Include(u => u.OrderDetailsDto).Where(u => u.UserId == userId).OrderByDescending(u => u.OrderHeaderId).ToList();
            }

            _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }
        return _response;
    }

    [Authorize]
    [HttpGet("GetOrder/{id:int}")]
    public ResponseDto? Get(int id)
    {
        try
        {
            OrderHeader orderHeader = _dbContext.OrderHeaders.Include(u => u.OrderDetailsDto).First(u => u.OrderHeaderId == id);
            _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }
        return _response;
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
    [HttpPost("ValidateStripeSession")]
    public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
    {
        try
        {
            OrderHeader orderHeader = _dbContext.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);

            var service = new SessionService();
            Session session = service.Get(orderHeader.StripeSessionId);

            var paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                // payment is successful so..
                orderHeader.PaymentIntentId = paymentIntent.Id;
                orderHeader.Status = SD.Status_Approved;
                _dbContext.SaveChanges();

                RewardsDto rewardsDto = new()
                {
                    OrderId = orderHeader.OrderHeaderId,
                    RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
                    UserId = orderHeader.UserId
                };
                string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatdTopic");
                await _messageBus.PublishMessage(rewardsDto, topicName);
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

    [Authorize]
    [HttpPost("UpdateOrderStatus/{orderId:int}")]
    public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
    {
        try
        {
            OrderHeader orderHeader = _dbContext.OrderHeaders.First(u => u.OrderHeaderId == orderId);
            if(orderHeader != null)
            {
                if(newStatus== SD.Status_Cancelled)
                {
                    // refund
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderHeader.PaymentIntentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);
                }
                orderHeader.Status = newStatus;
                _dbContext.SaveChanges();
            }


        }
        catch (Exception ex)
        {
            _response.IsSuccess=false;
            _response.Message = ex.Message;
        }
        return _response;
    }

}
