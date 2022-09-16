using LeaveSystem.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveSystem.Db;
public class LeaveSystemDbContext : DbContext
{
    public LeaveSystemDbContext(DbContextOptions<LeaveSystemDbContext> options) : base(options) { }

    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<UserLeaveLimit> UserLeaveLimits { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnLeaveTypeCreating(modelBuilder);
        OnUserLeaveLimitCreating(modelBuilder);
        OnRoleCreating(modelBuilder);
    }

    private void OnRoleCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
             .HasKey(e => e.Id);
        modelBuilder.Entity<Role>()
            .Property(b => b.RoleType)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<RoleType>()); ;
        modelBuilder.Entity<Role>()
            .Property(b => b.Email)
            .IsRequired();
        modelBuilder.Entity<Role>()
            .HasIndex(p => new { p.RoleType, p.Email }).IsUnique();
    }

    private void OnUserLeaveLimitCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLeaveLimit>()
             .HasKey(e => e.Id);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.AssignedToUserEmail)
            .IsRequired(false);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.Limit)
            .IsRequired(false);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.ValidSince)
            .IsRequired(false);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.ValidUntil)
            .IsRequired(false);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.LeaveTypeId)
            .IsRequired(true);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.Property)
            .IsRequired(false)
            .HasColumnType("jsonb");
        modelBuilder.Entity<UserLeaveLimit>()
            .HasOne(l => l.LeaveType)
            .WithMany(t => t.UserLeaveLimits)
            .HasForeignKey(l => l.LeaveTypeId)
            .HasPrincipalKey(t => t.Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<UserLeaveLimit>()
            .HasIndex(p => new { p.LeaveTypeId, p.AssignedToUserEmail, p.ValidSince }).IsUnique();
        modelBuilder.Entity<UserLeaveLimit>()
            .HasIndex(p => new { p.LeaveTypeId, p.AssignedToUserEmail, p.ValidUntil }).IsUnique();
    }

    private static void OnLeaveTypeCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaveType>()
             .HasKey(e => e.Id);
        modelBuilder.Entity<LeaveType>()
            .Property(b => b.Name)
            .IsRequired();
        modelBuilder.Entity<LeaveType>()
            .Property(b => b.Properties)
            .IsRequired(false)
            .HasColumnType("jsonb");
        modelBuilder.Entity<LeaveType>()
            .HasOne(t => t.BaseLeaveType)
            .WithMany(t => t.ConstraintedLeaveTypes)
            .HasForeignKey(t => t.BaseLeaveTypeId)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LeaveType>()
            .HasIndex(p => new { p.Name }).IsUnique();
    }
}
