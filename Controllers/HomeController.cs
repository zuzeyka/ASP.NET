using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;
using WebApplication1.Servises;
using WebApplication1.Servises.Hash;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DateServise _dateServise;
        private readonly TimeServise _timeServise;
        private readonly StampServise _stampServise;
        private readonly IHashServise _hashServise;

        public HomeController(ILogger<HomeController> logger, DateServise dateServise, TimeServise timeServise, StampServise stampServise, IHashServise hashServise)
        {
            _logger = logger;
            _dateServise = dateServise;
            _timeServise = timeServise;
            _stampServise = stampServise;
            _hashServise = hashServise;
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

        public IActionResult TagHelpers()
        {
            return View();
        }

        public IActionResult DisplayTemplates()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "DisplayTemplates",
                Title = "Data Display Templates",
                Products = new()
                {
                    new() { Name = "Зарядний кабель",       Image = "1.png", Price = 210 },
                    new() { Name = "Маніпулятор 'миша'",    Image = "2.png", Price = 399.50 },
                    new() { Name = "Наліпка 'Smiley'",      Image = "3.png", Price = 2.95 },
                    new() { Name = "Серветки для монітору", Image = "4.png", Price = 100 },
                    new() { Name = "USB ліхтарик",          Image = "5.png", Price = 49.50 },
                    new() { Name = "Аккумулятор ААА",       Image = "6.png", Price = 280 },
                    new() { Name = "ОС Windows Home",       Image = "7.png", Price = 1250 },
                }
            };
            return View(model);
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

        public ViewResult Servises()
        {
            ViewData["data_servise"] = _dateServise.GetMoment();
            ViewData["data_hashcode"] = _dateServise.GetHashCode();

            ViewData["time_servise"] = _timeServise.GetMoment();
            ViewData["time_hashcode"] = _timeServise.GetHashCode();

            ViewData["stamp_servise"] = _stampServise.GetMoment();
            ViewData["stamp_hashcode"] = _stampServise.GetHashCode();

            ViewData["hash_service"] = _hashServise.Hash("123");
            return View();
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