namespace WebApplication1.Data.Entity
{
    public class Topic
    {
        public Guid Id { get; set; }
        public Guid ThemeId { get; set; }
        public String Title { get; set; } = null!;
        public String Description { get; set; } = null!;
        public Guid AutorId { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime? DeleteDt { get; set; }
    }
}
