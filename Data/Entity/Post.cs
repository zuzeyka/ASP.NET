namespace WebApplication1.Data.Entity
{
    public class Post
    {
        public Guid Id { get; set; }
        public Guid TopicId { get; set; }
        public Guid AutorId { get; set; }
        public Guid? ReplyId { get; set; }
        public String Content { get; set; } = null!;
        public DateTime CreateDt { get; set; }
        public DateTime? DeleteDt { get; set; }
    }
}
