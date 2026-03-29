using Microsoft.AspNetCore.Mvc;

namespace PiscAtlas.WebApp.Controllers
{
    public class EventoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
