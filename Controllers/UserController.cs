using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebApplication1.Data;
using WebApplication1.Data.Entity;
using WebApplication1.Models;
using WebApplication1.Models.Email;
using WebApplication1.Models.Home.User;
using WebApplication1.Servises.Email;
using WebApplication1.Servises.Hash;
using WebApplication1.Servises.KDF;
using WebApplication1.Servises.Random;
using WebApplication1.Servises.Validation;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        private readonly IHashServise _hashService;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dataContext;
        private readonly IRandomServise _randomServise;
        private readonly IKdfServise _kdfServise;
        private readonly IValidationService _validationService;
        private readonly IEmailService _emailService;

        public UserController(IHashServise hashService, ILogger<UserController> logger, DataContext dataContext = null, IRandomServise randomServise = null, IKdfServise kdfServise = null, IValidationService validationService = null, IEmailService emailService = null)
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomServise = randomServise;
            _kdfServise = kdfServise;
            _validationService = validationService;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult EmailConfirmation()
        {
            return View();
        }

        public IActionResult Profile([FromRoute]String id)
        {
            _logger.LogInformation(id);
            User? user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
            if (user is not null)
            {
                Models.Home.User.ProfileModel model = new(user);
                // достаем ведомости про аутентификацию
                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)
                    .Value;
                    if (userLogin == user.Login) // Профиль - свой (персональный)
                    {
                        model.IsPersonal = true;
                    }
                }
                return View(model);
            } 
            else return NotFound();
            /* Личная страничка / Профиль
             * 1. Будет ли эта страничка другим пользователям?
             * Да, пользователи могут просматривать профили других пользователей,
             * но только те данные которые разрешил владелец.
             * 2 Как должна формироваться адрес /User/Profile/???
             * a) Id
             * b) login
             * Выбираем логин, в силу удобности распрастронения ссылки на профиль
             * !! необходимо уникальность логина
             
             */

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
            if (_dataContext.Users.Any(u => u.Login == registrationModel.Login))
            {
                registerValidation.PasswordMessage = "Login field can't be not unique (this login is already used)";
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
            if (!_validationService.Validate(registrationModel.Email, ValidationTerms.NotEmpty))
            {
                registerValidation.EmailMessage = "Email field can't be empty";
                isModelValid = false;
            }
            else if (!_validationService.Validate(registrationModel.Email, ValidationTerms.Email))
            {
                registerValidation.EmailMessage = "Not valid email";
                isModelValid = false;
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
                String confirmEmailCode = _randomServise.ConfirmCode(6);
                // отправляем код подтверждения
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    RealName = registrationModel.RealName,
                    Email = registrationModel.Email,
                    EmailCode = confirmEmailCode,
                    PasswordSalt = salt,
                    PasswordHash = _kdfServise.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegisterDt = DateTime.Now,
                    LastEnterDt = null
                };
                _dataContext.Users.Add(user);

                // Если данные добавленны в БД, отправляем код подверждения на почту
                // генерируем токен автоматческого подтверждения
                var emailConfirmToken = _GenerateEmailConfirmCode(user);

                _dataContext.SaveChangesAsync();

                _SendConfirmEmail(user, emailConfirmToken, "confirm_email");

                _SendConfirmEmail(user, emailConfirmToken, "welcome_email");

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

        private bool _SendConfirmEmail(Data.Entity.User user, 
                                       Data.Entity.EmailConfirmToken emailConfirmToken,
                                       String emailName)
        {
            // формируем ссылку: схема://домен/User/ConfirmToken?token=...
            // схема - http или https домен(хост) - localhost:7572
            String confirmLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/User/ConfirmToken?token={emailConfirmToken.Id}";
            return _emailService.Send(
                emailName,
                new Models.Email.ConfirmEmailModel
                {
                    Email = user.Email,
                    RealName = user.RealName,
                    EmailCode = user.EmailCode!,
                    ConfirmLink = confirmLink
                });
        }

        private Data.Entity.EmailConfirmToken _GenerateEmailConfirmCode(Data.Entity.User user)
        {
            Data.Entity.EmailConfirmToken emailConfirmToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserEmail = user.Email,
                Moment = DateTime.Now,
                Used = 0
            };

            _dataContext.EmailConfirmTokens.Add(emailConfirmToken);
            return emailConfirmToken;
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
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("authUserId");
            return RedirectToAction("Index", "Home");
            /* Redirect и другие вопросы с перенаправлением
             * Browser             Server
             * GET /home --------> (routing) ->Home::Index()->View()
             * page <-------- 200 OK <!dictype html>...
             * 
             * <a Logout> --------> User::Logout()->Redirect(...)
             * follow <------- 302 (Redirect) Location: /home
             * GET /home ---------> (routing)->Home::Index()->View()
             * page <-------- 200 OK <!doctype html>...
             * 
             * 301 - Permanent Redirect - переносим на постоянной основе,
             * как правило, сайт изменил URL
             * Произвольный редирект следуется GET-запросом, если необходимо
             * сохранить начальный метод, то используется
             * Redirect...PreserveMethod
             * 
             * 30x Redirect называют внешним, потому что информация
             * доходит до браузера и изменяется URL в адресной строке
             * http://..../addr1 ---> 302 Location /addr
             * http://..../addr2 ----> 200 html
             *                            addr1.asp
             * http://..../addr1 (if..)\  addr2.asp
             *                          \ addr3.asp
             *                       forward - внутренее перенаправление
             *  (в браузере /addr1, но фактично отображается addr3.asp)
             */
        }
        [HttpPut]   // метод доступний тільки для PUT запитів
        public IActionResult Update([FromBody] UpdateRequestModel model)
        {
            UpdateResponseModel responseModel = new();
            try
            {
                if (model is null) throw new Exception("No or empty data");
                if (HttpContext.User.Identity?.IsAuthenticated == false)
                {
                    throw new Exception("UnAuthenticated");
                }
                User? user = _dataContext.Users.Find(
                    Guid.Parse(
                        HttpContext.User.Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value
                ));
                if (user is null) throw new Exception("UnAuthorized");
                switch (model.Field)
                {
                    case "realname":
                        if(_validationService.Validate(model.Value, ValidationTerms.RealName))
                        {
                            user.RealName = model.Value;
                            _dataContext.SaveChanges();
                        }
                        else throw new Exception(
                                $"Validation error: field '{model.Field}' with value '{model.Value}'");
                        break;
                    case "email":
                        if (_validationService.Validate(model.Value, ValidationTerms.Email))
                        {
                            user.Email = model.Value;
                            user.EmailCode = _randomServise.ConfirmCode(6);
                            _SendConfirmEmail(user, _GenerateEmailConfirmCode(user), "confirm_email");
                            _dataContext.SaveChanges();
                        }
                        else throw new Exception(
                                $"Validation error: field '{model.Field}' with value '{model.Value}'");
                        break;
                    default:
                        throw new Exception("Invalid 'Field' attribute");
                }
                responseModel.Status = "OK";
                responseModel.Data = $"Field '{model.Field}' updated by value '{model.Value}'";
            }
            catch (Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Data = ex.Message;
            }

            return Json(responseModel);

            /* Метод для оновлення даних про користувача
             * Приймає асинхронні запити з JSON даними, повертає JSON
             * із результатом роботи.
             * Приймає дані = описуємо модель цих даних
             * Повертає дані = описуємо модель
             */
        }

        [HttpPost]
        public JsonResult ConfirmEmail([FromBody] string emailCode)
        {
            StatusDataModel model = new();

            if (String.IsNullOrEmpty(emailCode))
            {
                model.Status = "406";
                model.Data = "Empty code not acceptable";
            }
            else if (HttpContext.User.Identity?.IsAuthenticated == false)
            {
                model.Status = "401";
                model.Data = "Unauthenticated";
            }
            else
            {
                User? user = _dataContext.Users.Find(
                Guid.Parse(
                HttpContext.User.Claims
                .First(c => c.Type == ClaimTypes.Sid)
                .Value
                ));
                if (user is null)
                {
                    model.Status = "403";
                    model.Data = "Forbidden (UnAthorized)";
                }
                else if (user.EmailCode is null)
                {
                    model.Status = "208";
                    model.Data = "Already confirmed";
                }
                else if (user.EmailCode != emailCode)
                {
                    model.Status = "406";
                    model.Data = "Code not Accepted";
                }
                else
                {
                    user.EmailCode = null;
                    _dataContext.SaveChanges();
                    model.Status = "200";
                    model.Data = "OK";
                }
            }
            return Json(model); 
        }

        [HttpGet]
        public ViewResult ConfirmToken([FromQuery] String token)
        {
            ViewData["result"] = token;
            try
            {
                var confirmToken = _dataContext.EmailConfirmTokens
                    .Find(Guid.Parse(token))
                    ?? throw new Exception();

                var user = _dataContext.Users.Find(
                    confirmToken.UserId)
                    ?? throw new Exception();

                if (user.Email != confirmToken.UserEmail)
                    throw new Exception();

                user.EmailCode = null;
                confirmToken.Used += 1;
                _dataContext.SaveChanges();
                ViewData["result"] = "Congratulations, email now is confirmed";
            }
            catch
            {
                ViewData["result"] = "Verification failed, do not change the link from the email";
            }
            return View();
        }

        [HttpPatch]
        public String ResendConfirmEmail()
        {
            if (HttpContext.User.Identity?.IsAuthenticated == false)
            {
                return "Unauthenticated";
            }
            try
            {
                User? user = _dataContext.Users.Find(
                Guid.Parse(
                HttpContext.User.Claims
                .First(c => c.Type == ClaimTypes.Sid)
                .Value
                )) ?? throw new Exception();
                // формируем новый код подтверждения
                user.EmailCode = _randomServise.ConfirmCode(6);

                // генерируем токен автоматического подтверждения
                var emailConfirmToken = _GenerateEmailConfirmCode(user);

                // сохраняем новый код и токен
                _dataContext.SaveChangesAsync();

                // отправляем письмо
                if (_SendConfirmEmail(user, emailConfirmToken, "confirm_email"))
                    return "OK";
                else
                    return "Send error";
            }
            catch
            {
                return "Unauthenticated";
            }
            
            return "OK";
        }
    }
}
