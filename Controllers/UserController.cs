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
                registerValidation.LoginMessage = "Login field can't be empty";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.Password))
            {
                registerValidation.PasswordMessage = "Password field can't be empty";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.RepeatPassword))
            {
                registerValidation.RepeatPasswordMessage = "Repeat Password field can't be empty";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.Email))
            {
                registerValidation.EmailMessage = "Email field can't be empty";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.RealName))
            {
                registerValidation.RealNameMessage = "Real Name field can't be empty";
                isModelValid = false;
            }
            ViewData["registrationModel"] = registrationModel;
            ViewData["registerValidation"] = registerValidation;
            ViewData["isModelValid"] = isModelValid;
            // способ перейти на View под другим именем
            return View("Registration");
        }
    }
}
