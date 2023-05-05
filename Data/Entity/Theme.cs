namespace WebApplication1.Data.Entity
{
    public class Theme
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public String Title { get; set; } = null!;
        public String Description { get; set; } = null!;
        public Guid AutorId { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime? DeleteDt { get; set; }
    }
}
