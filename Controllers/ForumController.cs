using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Data.Entity;
using WebApplication1.Models.Forum;
using WebApplication1.Servises.Transliterate;
using WebApplication1.Servises.Validation;

namespace WebApplication1.Controllers
{
    public class ForumController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ForumController> _logger;
        private readonly IValidationService _validationService;
        private readonly ITransliterationService _transliterationService;

        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService, ITransliterationService transliterationService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _validationService = validationService;
            _transliterationService = transliterationService;
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
                            ? "Today " + s.CreatedDt.ToString("HH:mm")
                            : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = s.UrlId ?? s.Id.ToString(),
                        // AuthorName - RealName або Login в залежності від налагоджень 
                        AuthorName = s.Author.IsRealNamePublic
                            ? s.Author.RealName
                            : s.Author.Login,
                        AuthorAvatarUrl = s.Author.Avatar == null
                            ? "/avatars/no-avatar.jpg"
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
            Guid sectionId;
            try
            {
                sectionId = Guid.Parse(id);
            }
            catch
            {
                sectionId = _dataContext.Sections.First(s => s.UrlId == id).Id;
            }
            ForumSectionModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = sectionId.ToString(),
                Themes = _dataContext
                    .Themes
                    .Include(t => t.Author)
                    .Where(t => t.DeletedDt == null && t.SectionId == sectionId)
                    .Select(t => new ForumThemeViewModel()
                    {
                        Title = t.Title,
                        Description = t.Description,
                        CreatedDtString = DateTime.Today == t.CreatedDt.Date
                            ? "Today " + t.CreatedDt.ToString("HH:mm")
                            : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                        UrlIdString = t.Id.ToString(),
                        SectionId = t.SectionId.ToString(),
                        AuthorName = t.Author.IsRealNamePublic
                                        ? t.Author.RealName
                                        : t.Author.Login,
                        AuthorAvatarUrl = $"/avatars/{t.Author.Avatar ?? "no-avatar.jpg"}",
                        ProfileCreatedDt = t.CreatedDt
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

        public IActionResult Themes([FromRoute] String id)
        {
            Guid themeId;
            try
            {
                themeId = Guid.Parse(id);
            }
            catch
            {
                themeId = Guid.Empty; // _dataContext.Themes.First(s => s.UrlId == id).Id;
            }
            var theme = _dataContext.Themes.Find(themeId);
            var user = _dataContext.Users.Find(theme.AuthorId);
            if (theme == null)
            {
                return NotFound();
            }
            ForumThemesModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Title = theme.Title,
                ThemeId = id,
                Topics = _dataContext
                .Topics
                .Where(t => t.DeletedDt == null && t.ThemeId == themeId)
                .Select(t => new ForumTopicViewModel()
                {
                    Title = t.Title,
                    Description = t.Description,
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date
                            ? "Today " + t.CreatedDt.ToString("HH:mm")
                            : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    UrlIdString = t.Id.ToString(),
                    AuthorAvatarUrl = $"/avatars/{user.Avatar ?? "no-avatar.jpg"}",
                    AuthorName = user.IsRealNamePublic
                                        ? user.RealName
                                        : user.Login
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

        public IActionResult Topics([FromRoute] string id)
        {
            // ForumTopicsModel ForumPostFormModel ForumPostViewModel
            Guid topicId;
            try
            {
                topicId = Guid.Parse(id);
            }
            catch
            {
                topicId = Guid.Empty; // _dataContext.Themes.First(s => s.UrlId == id).Id;
            }
            var topic = _dataContext.Topics.Find(topicId);
            if (topic == null)
            {
                return NotFound();
            }
            ForumTopicModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Title = topic.Title,
                Description = topic.Description,
                TopicId = id,
                Posts = _dataContext
                .Posts
                .Include(t => t.Author)
                .Include(t => t.Reply)
                .Where(t => t.DeletedDt == null && t.TopicId == topicId)
                .Select(t => new ForumPostViewModel()
                {
                    Content = t.Content,
                    AuthorName = t.Author.IsRealNamePublic ? t.Author.RealName : t.Author.Login,
                    AuthorAvatarUrl = $"/avatars/{t.Author.Avatar ?? "no-avatar.jpg"}"
                    
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
                        Content = HttpContext.Session.GetString("SavedTitle")!,
                        ReplyId = HttpContext.Session.GetString("SavedDescription")!
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
            _logger.LogInformation("Title: {t}, Description: {d}, Photo: {p}",
                formModel.Title, formModel.Description, formModel.Photo);

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
                    String trans = _transliterationService.Transliterate(formModel.Title);
                    String urlId = trans;
                    int n = 2;
                    while (_dataContext.Sections.Any(s => s.UrlId == urlId))
                    {
                        urlId = $"{trans}{n++}";
                    }
                    _dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        UrlId = urlId
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
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
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
            return RedirectToAction(
                nameof(Sections),
                new { id = formModel.SectionId });  // TODO: id = UrlId ?? SectionId
        }

        [HttpPost]
        public RedirectToActionResult CreateTopic(ForumTopicFormModel formModel)
        {
            Guid userId;
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
                    userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Topics.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        ThemeId = Guid.Parse(formModel.ThemeId)
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
            return RedirectToAction(nameof(Themes), new {id = formModel.ThemeId});
        }

        [HttpPost]
        public RedirectToActionResult CreatePost(ForumPostFormModel formModel)
        {
            Guid userId;
            if (!_validationService.Validate(formModel.Content, ValidationTerms.NotEmpty))
            {
                HandleInvalidForm("Content can't be empty", formModel.Content, formModel.ReplyId);
            }
            else
            {
                try
                {
                    userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Posts.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Content = formModel.Content,
                        ReplyId = String.IsNullOrEmpty(formModel.ReplyId)
                        ? null
                        : Guid.Parse(formModel.ReplyId),
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        TopicId = Guid.Parse(formModel.TopicId)
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);
                    HttpContext.Session.SetString("CreateSectionMessage",
                        "Added successfully");
                }
                catch
                {
                    HandleInvalidForm("Authorization denied", formModel.Content, formModel.ReplyId);
                }
            }
            return RedirectToAction(nameof(Topics), new { id = formModel.TopicId });
        }
    }
}
