namespace WebApplication1.Models.Forum
{
    public class ForumTopicModel
    {
        public Boolean UserCanCreate { get; set; }
        public String TopicId { get; set; } = null!;
        public String Title { get; set; } = null!;
        public String Description { get; set; } = null!;
        public List<ForumPostViewModel> Posts { get; set; } = null!;

        public String? CreateMessage { get; set; }
        public bool? IsMessagePositive { get; set; }
        public ForumPostFormModel FormModel { get; set; } = null!;
    }
}
