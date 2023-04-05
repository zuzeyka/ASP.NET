using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Home.User;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        public IActionResult Register(RegistrationModel registrationModel)
        {
            bool isModelValid = true;
            RegisterValidationModel registerValidation = new();
            if (String.IsNullOrEmpty(registrationModel.Login))
            {
                registerValidation.LoginMessage = "Login can't be empty";
                isModelValid = false;
            }
            ViewData["registrationModel"] = registrationModel;
            // способ перейти на View под другим именем
            return View("Registration");
        }
    }
}
