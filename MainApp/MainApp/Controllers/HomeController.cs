using Microsoft.AspNetCore.Mvc;
using MainApp.Models;

namespace MainApp.Controllers
{
    public class HomeController : Controller
    {
        public string Welcome()
        {
            return "Welcome";
        }
    }
}
