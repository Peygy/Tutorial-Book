using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    // Controller to manage tutorial pages
    public class PageController : Controller
    {
        public IActionResult Welcome()
            => View();

        public IActionResult Study()
            => View();

        public IActionResult ViewProfile()
            => View();

        [Route("/error/{statusCode}")]
        public IActionResult Error(int statusCode)
            => View(statusCode);
    }
}
