using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }

        public IActionResult URL()
        {
            return View();
        }

        public IActionResult PassData()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Models",
                Title = "Data Transfer models",
                Products = new()
                {
                    new() { Name = "Зарядний кабель", Price = 210 },
                    new() { Name = "Маніпулятор 'миша'", Price = 399.50 },
                    new() { Name = "Наліпка 'Smiley'", Price = 2.95 },
                    new() { Name = "Серветки для монітору", Price = 100 },
                    new() { Name = "USB ліхтарик", Price = 49.50 },
                    new() { Name = "Аккумулятор ААА", Price = 280 },
                    new() { Name = "ОС Windows Home", Price = 1250 },
                }
            };
            return View(model);
        }

        public IActionResult Scheme()
        {
            ViewBag.bagdata = "Data in ViewBag"; // способы передачи данных
            ViewData["data"] = "Data in ViewData"; // к представлению
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Razor()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}