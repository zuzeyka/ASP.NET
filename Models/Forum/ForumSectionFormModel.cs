using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Models.Forum
{
    public class ForumSectionFormModel
    {
        [FromForm(Name = "section-title")]
        public String Title { get; set; } = null!;
        [FromForm(Name = "section-description")]
        public String Description { get; set; } = null!;
        [FromForm(Name = "section-photo")]
        public String? Photo { get; set; } = null!;
    }
}
