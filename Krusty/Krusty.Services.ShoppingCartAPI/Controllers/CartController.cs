using AutoMapper;
using Krusty.Services.ShoppingCartAPI.Data;
using Krusty.Services.ShoppingCartAPI.Models;
using Krusty.Services.ShoppingCartAPI.Models.Dto;
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

    public CartController(IMapper mapper, AppDbContext dbContext)
    {
        _responseDto = new ResponseDto();
        _mapper = mapper;
        _dbContext = dbContext;
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
