using Krusty.Web.Models;
using Krusty.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Krusty.Web.Controllers
{
    public class HomeController : Controller
    {
		private readonly IProductService _productService;
		private readonly ILogger _logger;
		public HomeController(IProductService productService)
		{
			_productService = productService;
		}

		public async Task<IActionResult> Index()
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
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDto? productDtoModel = new();

            ResponseDto response = await _productService.GetProductByIdAsync(productId);
            if (response != null && response.IsSuccess)
            {
                productDtoModel = JsonConvert.DeserializeObject<ProductDto>
                    (Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(productDtoModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
