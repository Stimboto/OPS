using Microsoft.EntityFrameworkCore;
using OPS.Domain.Entities;

namespace OPS.Infrastructure.Data;

public class OpsDbContext : DbContext
{
    public OpsDbContext(DbContextOptions<OpsDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentHistory> IncidentHistories => Set<IncidentHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();
    public DbSet<IncidentComment> IncidentComments => Set<IncidentComment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserTeam> UserTeams => Set<UserTeam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<UserTeam>(entity =>
        {
            entity.HasIndex(ut => new { ut.UserId, ut.TeamId }).IsUnique();
            
            entity.HasOne(ut => ut.User)
                .WithMany(u => u.UserTeams)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ut => ut.Team)
                .WithMany(t => t.UserTeams)
                .HasForeignKey(ut => ut.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasIndex(i => i.TrackingId).IsUnique();

            entity.HasOne(i => i.ReportedByUser)
                .WithMany()
                .HasForeignKey(i => i.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.AssignedToUser)
                .WithMany()
                .HasForeignKey(i => i.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.Team)
                .WithMany(t => t.Incidents)
                .HasForeignKey(i => i.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // Analytics Indexes
            entity.HasIndex(i => i.CreatedAt);
            entity.HasIndex(i => new { i.TeamId, i.CreatedAt });
            entity.HasIndex(i => new { i.AssignedToUserId, i.CreatedAt });
            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.ResolutionDueAt);
        });

        modelBuilder.Entity<IncidentHistory>(entity =>
        {
            entity.HasOne(h => h.Incident)
                .WithMany(i => i.History)
                .HasForeignKey(h => h.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IncidentAttachment>(entity =>
        {
            entity.HasOne(a => a.Incident)
                .WithMany(i => i.Attachments)
                .HasForeignKey(a => a.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(a => a.CreatedAt);
        });

        modelBuilder.Entity<IncidentComment>(entity =>
        {
            entity.HasOne(c => c.Incident)
                .WithMany(i => i.Comments)
                .HasForeignKey(c => c.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(c => c.CreatedAt);
            entity.HasIndex(c => c.IsDeleted);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "Manager", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 3, Name = "Responder", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 4, Name = "Reporter", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Team>().HasData(
            new Team { Id = 1, Name = "Platform Engineering", Description = "Core platform team", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Team { Id = 2, Name = "Infrastructure", Description = "Infrastructure and ops", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Team { Id = 3, Name = "Security", Description = "Security operations", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Team { Id = 4, Name = "Customer Operations", Description = "Customer support", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Team { Id = 5, Name = "Payments", Description = "Payments processing", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
