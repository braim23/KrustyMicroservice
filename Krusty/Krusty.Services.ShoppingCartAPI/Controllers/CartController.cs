using AutoMapper;
using Krusty.MessageBus;
using Krusty.Services.ShoppingCartAPI.Data;
using Krusty.Services.ShoppingCartAPI.Models;
using Krusty.Services.ShoppingCartAPI.Models.Dto;
using Krusty.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Krusty.Services.ShoppingCartAPI.Controllers;

[Route("api/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ResponseDto _responseDto;
    private IMapper _mapper;
    private readonly AppDbContext _dbContext;
    private readonly IProductService _productService;
    private readonly ICouponService _couponService;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;

    public CartController(IMapper mapper, AppDbContext dbContext, IProductService productService, ICouponService couponService, IMessageBus messageBus, IConfiguration configuration)
    {
        _responseDto = new ResponseDto();
        _mapper = mapper;
        _dbContext = dbContext;
        _productService = productService;
        _couponService = couponService;
        _messageBus = messageBus;
        _configuration = configuration;
    }


    [HttpGet("GetCart/{userId}")]
    public async Task<ResponseDto> GetCart(string userId)
    {
        try
        {
            CartDto cart = new()
            {
                CartHeader = _mapper.Map<CartHeaderDto>(_dbContext.CartHeaders.First(u => u.UserId == userId)),
            };
            cart.CartDetailsDto = _mapper.Map<IEnumerable<CartDetailsDto>>(_dbContext.CartDetails
                .Where(u=>u.CartHeaderId==cart.CartHeader.CartHeaderId));

            IEnumerable<ProductDto> productDtos = await _productService.GetProductsAsync();

            foreach(var item in cart.CartDetailsDto)
            {
                item.ProductDto = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                cart.CartHeader.CartTotal += (item.Count * item.ProductDto.Price);
            }


            // Apply coupon logic
            if(!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
            {
                CouponDto couponDto = await _couponService.GetCouponAsync(cart.CartHeader.CouponCode);
                if(couponDto != null && cart.CartHeader.CartTotal >= couponDto.MinAmount)
                {
                    cart.CartHeader.CartTotal -= couponDto.DiscountAmount;
                    cart.CartHeader.Discount = couponDto.DiscountAmount;
                }
            }


            _responseDto.Result = cart;
        }
        catch (Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.Message = ex.Message;
        }
        return _responseDto;
    }

    [HttpPost("ApplyCoupon")]
    public async Task<Object> ApplyCoupon([FromBody] CartDto cartDto)
    {
        try
        {
            var cartFromDb = await _dbContext.CartHeaders.FirstAsync(u=>u.UserId == cartDto.CartHeader.UserId);
            cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
            _dbContext.CartHeaders.Update(cartFromDb);
            _dbContext.SaveChangesAsync();
            _responseDto.Result = true;
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.Message = ex.Message;
        }
        return _responseDto;
    }

    [HttpPost("RemoveCoupon")]
    public async Task<Object> RemoveCoupon([FromBody] CartDto cartDto)
    {
        try
        {
            var cartFromDb = await _dbContext.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
            cartFromDb.CouponCode = "";
            _dbContext.CartHeaders.Update(cartFromDb);
            _dbContext.SaveChangesAsync();
            _responseDto.Result = true;
        }
        catch (Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.Message = ex.Message;
        }
        return _responseDto;
    }

    [HttpPost("EmailCartRequest")]
    public async Task<Object> EmailCartRequest([FromBody] CartDto cartDto)
    {
        try
        {
            await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCart"));
            _responseDto.Result = true;
        }
        catch (Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.Message = ex.Message;
        }
        return _responseDto;
    }


    [HttpPost("CartUpsert")]
    public async Task<ResponseDto> CartUpsert(CartDto cartDto)
    {
        try
        {
            var cartHeaderFromDb = await _dbContext.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u=>u.UserId == cartDto.CartHeader.UserId);
            if (cartHeaderFromDb == null)
            {
                // create a new cart = new header and details
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                _dbContext.CartHeaders.Add(cartHeader);
                await _dbContext.SaveChangesAsync();

                cartDto.CartDetailsDto.First().CartHeaderId = cartHeader.CartHeaderId;
                _dbContext.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetailsDto.First()));
                await _dbContext.SaveChangesAsync();

            }
            else
            {
                // if header is not null = check if details has the same product
                var cartDetailsFromDb = await _dbContext.CartDetails.AsNoTracking().FirstOrDefaultAsync
                    (u => u.ProductId == cartDto.CartDetailsDto.First()
                    .ProductId && u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                if (cartDetailsFromDb == null)
                {
                    // create cart details
                    cartDto.CartDetailsDto.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    _dbContext.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetailsDto.First()));
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // update count in cart details
                    cartDto.CartDetailsDto.First().Count += cartDetailsFromDb.Count;
                    cartDto.CartDetailsDto.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    cartDto.CartDetailsDto.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                    _dbContext.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetailsDto.First()));
                    await _dbContext.SaveChangesAsync();
                }
            }
            _responseDto.Result = cartDto;
        }
        catch (Exception ex)
        {
            _responseDto.Message = ex.Message.ToString();
            _responseDto.IsSuccess = false;
        }
        return _responseDto;

    }

    [HttpPost("RemoveCart")]
    public async Task<ResponseDto> RemoveCart([FromBody]int cartDetailsId)
    {
        try
        {
            CartDetails cartDetails = _dbContext.CartDetails.First(u=>u.CartDetailsId == cartDetailsId);
            int totalCountOfCartItems = _dbContext.CartDetails.Where(u=>u.CartHeaderId == cartDetails.CartHeaderId).Count();
            _dbContext.CartDetails.Remove(cartDetails);
            if(totalCountOfCartItems == 1)
            {
                var cartHeaderToRemove = await _dbContext.CartHeaders
                    .FirstOrDefaultAsync(u=>u.CartHeaderId==cartDetails.CartHeaderId);

                _dbContext.CartHeaders.Remove(cartHeaderToRemove);
            }
            await _dbContext.SaveChangesAsync();
            _responseDto.Result = true;
        }
        catch (Exception ex)
        {
            _responseDto.Message = ex.Message.ToString();
            _responseDto.IsSuccess = false;
        }
        return _responseDto;

    }


}
