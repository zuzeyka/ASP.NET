using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;
using WebApplication1.Data;
using WebApplication1.Data.Entity;
using WebApplication1.Models.Home.User;
using WebApplication1.Servises.Hash;
using WebApplication1.Servises.KDF;
using WebApplication1.Servises.Random;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        private readonly IHashServise _hashService;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dataContext;
        private readonly IRandomServise _randomServise;
        private readonly IKdfServise _kdfServise;

        public UserController(IHashServise hashService, ILogger<UserController> logger, DataContext dataContext = null, IRandomServise randomServise = null, IKdfServise kdfServise = null)
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomServise = randomServise;
            _kdfServise = kdfServise;
        }

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
            String savedName = null;
            if (registrationModel.Avatar is not null)
            {
                if (registrationModel.Avatar.Length < 1024)
                {
                    registerValidation.AvatarMessage = "You need image more than 1kb";
                    isModelValid = false;
                }
                else
                {
                    savedName = _randomServise.RandomAvatarName(registrationModel.Avatar.FileName, 16);
                    String path = "wwwroot/avatars/" + savedName;
                    FileInfo fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        do
                        {
                            savedName = _randomServise.RandomAvatarName(registrationModel.Avatar.FileName, 16);
                            fileInfo = new FileInfo(path);
                        } while (fileInfo.Exists);
                    }
                    using FileStream fs = new(path, FileMode.Create);
                    registrationModel.Avatar.CopyTo(fs);
                    ViewData["savedName"] = savedName;
                }
            }
            
            if (isModelValid)
            {
                String salt = _randomServise.RandomString(16);
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    RealName = registrationModel.RealName,
                    Email = registrationModel.Email,
                    EmailCode = _randomServise.ConfirmCode(6),
                    PasswordSalt = salt,
                    PasswordHash = _kdfServise.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegisterDt = DateTime.Now,
                    LastEnterDt = null
                };
                _dataContext.Users.Add(user);
                _dataContext.SaveChangesAsync();

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
        [HttpPost] // метод доступный только для POST запросов
        public String AuthUser()
        {
            // альтернативный (для моделей) способ получения параметров запроса\
            StringValues lovinValues = Request.Form["user-login"];
            // коллекция loginValues формируется при любом ключе, но для
            // неправильных (отсутсвующих) ключей она пустая
            if (lovinValues.Count == 0)
            {
                // нет логина в составе полей
                return "Missed required parameter: user-login";
            }
            String login = lovinValues[0] ?? "";

            StringValues passValues = Request.Form["user-password"];
            if (passValues.Count == 0)
            {
                // нет логина в составе полей
                return "Missed required parameter: user-password";
            }
            String password = passValues[0] ?? "";

            // ищемпользователя по логину
            User? user = _dataContext.Users.Where(u => u.Login == login).FirstOrDefault();
            if (user is not null)
            {
                // если нашли - проверяем пароль (derived key)
                if (user.PasswordHash == _kdfServise.GetDerivedKey(password, user.PasswordSalt))
                {
                    // данные проверены - пользователь аутентифицирован - сохраняем в сессии
                    HttpContext.Session.SetString("authUserId", user.Id.ToString());
                    return "OK";
                }
            }
            return $"Error Auth User: Login:{login}, Password:{password} check again";
        }
    }
}
