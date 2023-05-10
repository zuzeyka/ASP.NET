﻿using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entity.User> Users { get; set; }
        public DbSet<Entity.EmailConfirmToken> EmailConfirmTokens { get; set; }
        public DbSet<Entity.Section> Sections { get; set; }
        public DbSet<Entity.Theme> Themes { get; set; }
        public DbSet<Entity.Topic> Topics { get; set; }
        public DbSet<Entity.Post> Posts { get; set; }
        public DbSet<Entity.Rate> Rates { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity.Rate>()
                .HasKey(
                nameof(Entity.Rate.ItemId), // Встановлення композитного 
                nameof(Entity.Rate.UserId)); // ключа

            // One-to-many Author(User)-Section
            modelBuilder.Entity<Entity.Section>()
                .HasOne(s => s.Author)  // NavyProp
                .WithMany()             // Empty - ref by type (User Author)
                .HasForeignKey(s => s.AuthorId);

            modelBuilder.Entity<Entity.Theme>()
                .HasOne(s => s.Author)
                .WithMany()
                .HasForeignKey(s => s.AuthorId);
        }
    }
}
