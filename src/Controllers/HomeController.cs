using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
