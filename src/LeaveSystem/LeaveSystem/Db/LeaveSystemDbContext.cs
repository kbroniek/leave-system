using LeaveSystem.Db.Domains;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Db;
public class LeaveSystemDbContext : DbContext
{
    public LeaveSystemDbContext(DbContextOptions<LeaveSystemDbContext> options) : base(options) { }

    public DbSet<LeaveType>? LeaveTypes { get; set; }
    public DbSet<Department>? Department { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnLeaveTypeCreating(modelBuilder);
        OnDepartmentCreating(modelBuilder);
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
            .IsRequired(false)
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
