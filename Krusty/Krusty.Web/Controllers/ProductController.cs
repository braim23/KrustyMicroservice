using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Krusty.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> ProductIndex()
        {
            List<ProductDto>? list = new();

            ResponseDto response = await _productService.GetAllProductsAsync();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDto>>
                    (Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(list);
        }
        [HttpGet]
        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductDto productDto)
        {
            if(ModelState.IsValid)
            {
                ResponseDto response = await _productService.CreateProductAsync(productDto);
                if(response != null && response.IsSuccess) 
                {
                    TempData["success"] = "Product created successfully!";
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            return View(productDto);
        }
        [HttpGet]
        public async Task<IActionResult> ProductDelete(int productId)
        {
			ResponseDto respone = await _productService.GetProductByIdAsync(productId);
			if (respone != null && respone.IsSuccess)
			{
				ProductDto? productDto = JsonConvert.DeserializeObject<ProductDto>
					(Convert.ToString(respone.Result));
                return View(productDto);
			}
			return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDto productDto)
        {
            ResponseDto response = await _productService.DeleteProductAsync(productDto.ProductId);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product deleted!";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(productDto);
        }

		// edit

		[HttpGet]
		public async Task<IActionResult> ProductEdit(int productId)
		{
			ResponseDto respone = await _productService.GetProductByIdAsync(productId);
			if (respone != null && respone.IsSuccess)
			{
				ProductDto? productDto = JsonConvert.DeserializeObject<ProductDto>
					(Convert.ToString(respone.Result));
				return View(productDto);
			}
			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> ProductEdit(ProductDto productDto)
		{
			ResponseDto response = await _productService.UpdateProductAsync(productDto);
			if (response != null && response.IsSuccess)
			{
				TempData["success"] = "Product updated successfully!";
				return RedirectToAction(nameof(ProductIndex));
			}
			else
			{
				TempData["error"] = response?.Message;
			}
			return View(productDto);
		}
	}
}
