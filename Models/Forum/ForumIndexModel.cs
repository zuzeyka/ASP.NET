using WebApplication1.Models.Forum;

namespace WebApplication1.Models.Forum
{
    public class ForumIndexModel
    {
        public List<ForumSectionViewModel> Sections { get; set; } = null!;
        public Boolean UserCanCreate { get; set; }
        public String? CreateMessage { get; set; }
        public bool? IsMessagePositive { get; set; }
        public ForumSectionFormModel? FormModel { get; set; }
    }
}
