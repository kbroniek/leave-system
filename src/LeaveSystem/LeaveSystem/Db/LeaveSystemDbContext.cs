using LeaveSystem.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Db;
public class LeaveSystemDbContext : DbContext
{
    public LeaveSystemDbContext(DbContextOptions<LeaveSystemDbContext> options) : base(options) { }

    public DbSet<LeaveType>? LeaveTypes { get; set; }
    public DbSet<Department>? Departments { get; set; }
    public DbSet<UserLeaveLimit>? UserLeaveLimits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnLeaveTypeCreating(modelBuilder);
        OnDepartmentCreating(modelBuilder);
        OnUserLeaveLimitCreating(modelBuilder);
    }

    private void OnUserLeaveLimitCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLeaveLimit>()
             .HasKey(e => e.UserLeaveLimitId);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.User)
            .IsRequired()
            .HasColumnType("jsonb");
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.Limit)
            .IsRequired();
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.ValidSince)
            .IsRequired();
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.ValidUntil)
            .IsRequired();
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.Property)
            .IsRequired(false)
            .HasColumnType("jsonb");
        modelBuilder.Entity<UserLeaveLimit>()
            .Ignore(t => t.Id);
        modelBuilder.Entity<UserLeaveLimit>()
            .HasOne(l => l.LeaveType)
            .WithMany(t => t.UserLeaveLimits)
            .HasForeignKey(l => l.LeaveTypeId)
            .HasPrincipalKey(t => t.LeaveTypeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void OnDepartmentCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>()
             .HasKey(e => e.DepartmentId);
        modelBuilder.Entity<Department>()
            .Property(b => b.Title)
            .IsRequired();
        modelBuilder.Entity<Department>()
            .Property(b => b.Users)
            .IsRequired()
            .HasColumnType("jsonb");
        modelBuilder.Entity<Department>()
            .Ignore(t => t.Id);
    }

    private static void OnLeaveTypeCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeaveType>()
             .HasKey(e => e.LeaveTypeId);
        modelBuilder.Entity<LeaveType>()
            .Property(b => b.Title)
            .IsRequired();
        modelBuilder.Entity<LeaveType>()
            .Property(b => b.Properties)
            .IsRequired(false)
            .HasColumnType("jsonb");
        modelBuilder.Entity<LeaveType>()
            .HasOne(t => t.BaseLeaveType)
            .WithMany(t => t.ConstraintedLeaveTypes)
            .HasForeignKey(t => t.BaseLeaveTypeId)
            .HasPrincipalKey(t => t.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LeaveType>()
            .Ignore(t => t.Id);
    }
}
