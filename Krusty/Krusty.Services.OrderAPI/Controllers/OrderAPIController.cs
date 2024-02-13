using AutoMapper;
using Krusty.Services.OrderAPI.Data;
using Krusty.Services.OrderAPI.Models.Dto;
using Krusty.Services.ShoppingCartAPI.Service.IService;
using Krusty.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Krusty.Services.OrderAPI.Models;

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
            orderHeaderDto.OrderTotal = Math.Round(orderHeaderDto.OrderTotal, 2);
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

    //[HttpGet("Meow")]
    //public ResponseDto Meow()
    //{
    //    Console.WriteLine("Meow");
    //    return _response;
    //}

}
