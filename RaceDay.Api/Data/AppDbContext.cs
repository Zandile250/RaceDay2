using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        b.Entity<Event>().Property(e => e.EventType).HasConversion<string>().HasMaxLength(20);
        b.Entity<Event>().Property(e => e.DistanceKm).HasPrecision(6, 2);
        b.Entity<Event>().HasOne(e => e.Organiser).WithMany().HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Category>().HasOne(c => c.Event).WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Cascade);

        // One enrolment per participant per event
        b.Entity<Enrolment>().HasIndex(e => new { e.EventId, e.ParticipantId }).IsUnique();
        b.Entity<Enrolment>().HasOne(e => e.Event).WithMany().HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Enrolment>().HasOne(e => e.Participant).WithMany().HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<Enrolment>().HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // One result per enrolment
        b.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();
        b.Entity<Result>().HasOne(r => r.Enrolment).WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
