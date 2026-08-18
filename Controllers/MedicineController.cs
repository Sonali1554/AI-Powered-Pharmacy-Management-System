using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmacyManagmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MedicineController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}