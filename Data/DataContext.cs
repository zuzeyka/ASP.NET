using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entity.User> Users { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
        }
    }
}
