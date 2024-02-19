using Microsoft.AspNetCore.Mvc;

namespace Krusty.Web.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult OrderIndex()
        {
            return View();
        }
    }
}
