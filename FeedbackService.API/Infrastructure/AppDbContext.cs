using FeedbackService.API.Features.Feedback;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FeedbackService.API.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feedback>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Comment).HasMaxLength(2000);
                b.Property(x => x.Visibility)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(Features.Feedback.FeedbackVisibility.Public);
                b.Property(x => x.Tags)
                    .HasMaxLength(500)
                    .HasDefaultValue(string.Empty);
            });
        }
    }
}
