    using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FPL_Showcase_WD.Controllers
{
    [AllowAnonymous]
    public class FantasyTeamController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}