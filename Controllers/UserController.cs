using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
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
            byte minPasswordLenght = 3;
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
            else if (registrationModel.Password.Length < minPasswordLenght)
            {
                registerValidation.PasswordMessage = $"Password is too short. At least {minPasswordLenght} symbols";
                registerValidation.RepeatPasswordMessage = "Password field is empty";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.RepeatPassword))
            {
                registerValidation.RepeatPasswordMessage = "Repeat Password field can't be empty";
                isModelValid = false;
            }
            else if (!registrationModel.RepeatPassword.Equals(registrationModel.Password))
            {
                registerValidation.RepeatPasswordMessage = "Repeat Password field isn't match with Password field";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.Email))
            {
                registerValidation.EmailMessage = "Email field can't be empty";
                isModelValid = false;
            }
            else
            {
                String emailRegex = @"^[\w.%+-]+@[\w.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(registrationModel.Email, emailRegex))
                {
                    registerValidation.EmailMessage = "Not valid email";
                }
            }
            if (String.IsNullOrEmpty(registrationModel.RealName))
            {
                registerValidation.RealNameMessage = "Real Name field can't be empty";
                isModelValid = false;
            }
            if (registrationModel.Avatar is not null)
            {
                if (registrationModel.Avatar.Length < 1024)
                {
                    registerValidation.AvatarMessage = "You need image more than 1kb";
                }
                else
                {
                    String path = "wwwroot/avatars/" + registrationModel.Avatar.FileName;
                    using FileStream fs = new(path, FileMode.Create);
                    registrationModel.Avatar.CopyTo(fs);
                }
            }
            
            if (isModelValid)
            {
                return View(registrationModel);
            }
            else
            {
                ViewData["registrationModel"] = registrationModel;
                ViewData["registerValidation"] = registerValidation;
            }
            
            ViewData["isModelValid"] = isModelValid;
            // способ перейти на View под другим именем
            return View("Registration");
        }
    }
}
