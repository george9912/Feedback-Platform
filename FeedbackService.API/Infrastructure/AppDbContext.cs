using FeedbackService.API.Features.Feedback;
using FeedbackService.API.Features.Feedback.Campaign;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FeedbackService.API.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<FeedbackCampaign> Campaigns => Set<FeedbackCampaign>();
        public DbSet<CampaignAudienceRule> CampaignAudienceRules => Set<CampaignAudienceRule>();
        public DbSet<CampaignParticipant> CampaignParticipants => Set<CampaignParticipant>();
        public DbSet<CampaignFinalReport> CampaignFinalReports => Set<CampaignFinalReport>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feedback>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Comment).HasMaxLength(2000);
                b.Property(x => x.SubmittedByUserId);
                b.Property(x => x.CampaignId);
                b.Property(x => x.Visibility)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(Features.Feedback.FeedbackVisibility.Public);
                b.Property(x => x.Tags)
                    .HasMaxLength(500)
                    .HasDefaultValue(string.Empty);
                b.HasIndex(x => x.CampaignId);
                b.HasIndex(x => new { x.CampaignId, x.SubmittedByUserId });
            });

            modelBuilder.Entity<FeedbackCampaign>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(200).IsRequired();
                b.Property(x => x.Description).HasMaxLength(2000);
                b.Property(x => x.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(CampaignStatus.Draft);
                b.Property(x => x.MinimumRequiredSubmissions).IsRequired();
                b.HasMany(x => x.AudienceRules)
                    .WithOne()
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasMany(x => x.Participants)
                    .WithOne()
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.Status);
            });

            modelBuilder.Entity<CampaignAudienceRule>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.AudienceType)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                b.Property(x => x.RuleValue).HasMaxLength(200).IsRequired();
                b.HasIndex(x => new { x.CampaignId, x.AudienceType });
            });

            modelBuilder.Entity<CampaignParticipant>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.RequiredSubmissions).IsRequired();
                b.Property(x => x.SubmittedSubmissions).IsRequired();
                b.HasIndex(x => new { x.CampaignId, x.UserId }).IsUnique();
                b.HasIndex(x => x.UserId);
            });

            modelBuilder.Entity<CampaignFinalReport>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.CampaignId).IsUnique();
            });
        }
    }
}
