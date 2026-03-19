using Microsoft.AspNetCore.Mvc;
using SecurityDashboard.Models;

namespace SecurityDashboard.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) {
        _logger = logger;
    }

    public IActionResult Index() {
        return View();
    }

    [HttpGet]
    public IActionResult Home() {
        return View();
    }

    [HttpPost]
    public IActionResult Home(string confirm) {
        if (bool.TryParse(confirm, out _))
            AlertSystemController.ToggleAlertSystem();
        return View();
    }

    public IActionResult AboutUs() {
        return View();
    }
}
