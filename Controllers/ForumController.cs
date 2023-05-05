using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models.Forum;
using WebApplication1.Servises.Validation;

namespace WebApplication1.Controllers
{
    public class ForumController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ForumController> _logger;
        private readonly IValidationService _validationService;

        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _validationService = validationService;
        }

        private int _counter = 0;
        private int Counter { get => _counter++; set => _counter = value; }

        public IActionResult Index()
        {
            Counter = 0;
            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = _dataContext
                    .Sections
                    .Include(s => s.Author)   // включити навігаційну властивість Author
                    .Where(s => s.DeletedDt == null)
                    .OrderBy(s => s.CreatedDt)
                    .AsEnumerable()  // IQueriable -> IEnumerable
                    .Select(s => new ForumSectionViewModel()
                    {
                        Title = s.Title,
                        Description = s.Description,
                        LogoUrl = $"/img/logos/section{Counter}.png",
                        CreatedDtString = DateTime.Today == s.CreatedDt.Date
                            ? "Сьогодні " + s.CreatedDt.ToString("HH:mm")
                            : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = s.Id.ToString(),
                        // AuthorName - RealName або Login в залежності від налагоджень 
                        AuthorName = s.Author.IsRealNamePublic
                            ? s.Author.RealName
                            : s.Author.Login,
                        AuthorAvatarUrl = s.Author.Avatar == null
                            ? "/avatars/no-avatar.png"
                            : $"/avatars/{s.Author.Avatar}"
                    })
                    .ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
            {
                HttpContext.Session.Remove("CreateSectionMessage");
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle")!,
                        Description = HttpContext.Session.GetString("SavedDescription")!
                    };
                    HttpContext.Session.Remove("SaveTitle");
                    HttpContext.Session.Remove("SaveDescription");
                }
                HttpContext.Session.Remove("CreateSectionMessage");
                HttpContext.Session.Remove("IsMessagePositive");
            }

            return View(model);
        }

        public ViewResult Sections([FromRoute] String id)
        {
            ForumSectionModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = id,
                Themes = _dataContext
                    .Themes
                    .Where(t => t.DeleteDt == null && t.SectionId == Guid.Parse(id))
                    .Select(t => new ForumThemeViewModel()
                    {
                        Title = t.Title,
                        Description = t.Description,
                        CreatedDtString = DateTime.Today == t.CreateDt.Date
                            ? "Today " + t.CreateDt.ToString("HH:mm")
                            : t.CreateDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = t.Id.ToString(),
                        SectionId = t.SectionId.ToString(),
                    })
                    .ToList()
            };

            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
            {
                _logger.LogInformation(id);
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle")!,
                        Description = HttpContext.Session.GetString("SavedDescription")!
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateSectionMessage");
                HttpContext.Session.Remove("IsMessagePositive");
            }

            return View(model);
        }

        private void HandleInvalidForm(string errorMessage, string savedTitle, string savedDescription)
        {
            HttpContext.Session.SetInt32("IsMessagePositive", 0);
            HttpContext.Session.SetString("CreateSectionMessage", errorMessage);
            HttpContext.Session.SetString("SavedTitle", savedTitle ?? String.Empty);
            HttpContext.Session.SetString("SavedDescription", savedDescription ?? String.Empty);
        }

        [HttpPost]
        public RedirectToActionResult CreateSection(ForumSectionFormModel formModel)
        {
            _logger.LogInformation("Title: {t}, Description: {d}",
                formModel.Title, formModel.Description);

            if(!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty))
            {
                HandleInvalidForm("Name can't be empty", formModel.Title, formModel.Description);
            }
            else if (!_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HandleInvalidForm("Description can't be empty", formModel.Title, formModel.Description);
            }
            else
            {
                try
                {
                    Guid userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);
                    HttpContext.Session.SetString("CreateSectionMessage",
                        "Added successfully");
                }
                catch
                {
                    HandleInvalidForm("Authorization denied", formModel.Title, formModel.Description);
                }
                
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public RedirectToActionResult CreateTheme(ForumThemeFormModel formModel)
        {
            if (!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty))
            {
                HandleInvalidForm("Name can't be empty", formModel.Title, formModel.Description);
            }
            else if (!_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                HandleInvalidForm("Description can't be empty", formModel.Title, formModel.Description);
            }
            else
            {
                try
                {
                    Guid userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Themes.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreateDt = DateTime.Now,
                        AutorId = userId,
                        SectionId = Guid.Parse(formModel.SectionId)
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);
                    HttpContext.Session.SetString("CreateSectionMessage",
                        "Added successfully");
                }
                catch
                {
                    HandleInvalidForm("Authorization denied", formModel.Title, formModel.Description);
                }

            }
            return RedirectToAction(nameof(Sections), new {id = formModel.SectionId});
        }
    }
}
