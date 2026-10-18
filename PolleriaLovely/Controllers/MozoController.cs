using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PolleriaLovely.Controllers
{
    [Authorize(Roles = "Mozo")]
    public class MozoController : Controller
    {
        public IActionResult Index() => View();
    }
}