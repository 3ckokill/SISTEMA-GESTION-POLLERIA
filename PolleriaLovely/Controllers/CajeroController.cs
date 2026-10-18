using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CajeroController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}