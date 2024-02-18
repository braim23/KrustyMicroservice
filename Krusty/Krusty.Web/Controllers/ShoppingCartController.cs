﻿using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Krusty.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Krusty.Web.Controllers;

public class ShoppingCartController : Controller
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IOrderService _orderService;

    public ShoppingCartController(IShoppingCartService shoppingCartService, IOrderService orderService)
    {
        _shoppingCartService = shoppingCartService;
        _orderService = orderService;
    }
    [Authorize]
    public async Task<IActionResult> CartIndex()
    {
        return View(await LoadCartDtoBasedOnLoggedInUser());
    }
    [Authorize]
    public async Task<IActionResult> Checkout()
    {
        return View(await LoadCartDtoBasedOnLoggedInUser());
    }

    [HttpPost]
    [ActionName("Checkout")]
    public async Task<IActionResult> Checkout(CartDto cartDto)
    {
        CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
        cart.CartHeaderDto.Phone = cartDto.CartHeaderDto.Phone;
        cart.CartHeaderDto.Email = cartDto.CartHeaderDto.Email;
        cart.CartHeaderDto.Name = cartDto.CartHeaderDto.Name;

        var response = await _orderService.CreateOrder(cart);
        OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

        if (response != null && response.IsSuccess)
        {
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            // get stripe session and redirecto to stripe
            StripeRequestDto stripeRequestDto = new()
            {
                ApprovedUrl = domain + "ShoppingCart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
                CancelUrl = domain + "ShoppingCart/Checkout",
                OrderHeaderDto = orderHeaderDto
            };

            var stripeResponse = await _orderService.CreateStripeSession(stripeRequestDto);
            StripeRequestDto stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));
            Response.Headers.Add("Location", stripeResponseResult.StripeSessionUrl);
            return new StatusCodeResult(303);

        }

        return View();
    }

    public async Task<IActionResult> Confirmation(int orderId)
    {
        ResponseDto? response = await _orderService.ValidateStripeSession(orderId);
        if (response != null && response.IsSuccess)
        {
            OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            if(orderHeaderDto.Status == SD.Status_Approved)
            {
                return View(orderId);
            }
            TempData["success"] = "Cart updated successfully!";
            return RedirectToAction(nameof(CartIndex));
        }

        // Redirect to some error page based on the status
        return View(orderId);
    }

    

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
    {
        ResponseDto? response = await _shoppingCartService.ApplyCouponAsync(cartDto);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully!";
            return RedirectToAction(nameof(CartIndex));
        }
        TempData["error"] = "Someting went wrong";
        return RedirectToAction(nameof(CartIndex));
    }
    [HttpPost]
    public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
    {
        cartDto.CartHeaderDto.CouponCode = "";
        ResponseDto? response = await _shoppingCartService.ApplyCouponAsync(cartDto);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully!";
            return RedirectToAction(nameof(CartIndex));
        }
        TempData["error"] = "Someting went wrong";
        return RedirectToAction(nameof(CartIndex));
    }

    public async Task<IActionResult> Remove(int cartDetailsId)
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        ResponseDto? response = await _shoppingCartService.RemoveFromCartAsync(cartDetailsId);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully!";
            return RedirectToAction(nameof(CartIndex));
        }
        TempData["error"] = "Someting went wrong";
        return RedirectToAction(nameof(CartIndex));
    }

    [HttpPost]
    public async Task<IActionResult> EmailCart(CartDto cartDto)
    {
        CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
        cart.CartHeaderDto.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;

        ResponseDto? response = await _shoppingCartService.EmailCart(cart);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Email will be processed and sent shortly.";
            return RedirectToAction(nameof(CartIndex));
        }
        TempData["error"] = "Someting went wrong";
        return RedirectToAction(nameof(CartIndex));
    }

    private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        ResponseDto? response = await _shoppingCartService.GetCartByUserIdAsync(userId);
        if(response != null && response.IsSuccess)
        {
            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
            return cartDto;
        }
        return new CartDto();
    }
}
