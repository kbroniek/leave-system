﻿using System.Text.Json;
using LeaveSystem.Converters;
using LeaveSystem.Db.Entities;
using Marten.Services.Json;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveSystem.Db;
public class LeaveSystemDbContext : DbContext
{
    public LeaveSystemDbContext(DbContextOptions<LeaveSystemDbContext> options) : base(options) { }
    public LeaveSystemDbContext() { }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<UserLeaveLimit> UserLeaveLimits { get; set; }
    public DbSet<Setting> Settings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnLeaveTypeCreating(modelBuilder);
        OnUserLeaveLimitCreating(modelBuilder);
        OnSettingsCreating(modelBuilder);
    }

    private void OnSettingsCreating(ModelBuilder modelBuilder)
    {
        var userLeaveLimitProperties = new TypeToJsonConverter<JsonDocument>();
        modelBuilder.Entity<Setting>()
             .HasKey(e => e.Id);
        modelBuilder.Entity<Setting>()
            .Property(b => b.Category)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<SettingCategoryType>());
        modelBuilder.Entity<Setting>()
            .Property(b => b.Value)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasConversion(userLeaveLimitProperties);
    }

    private void OnUserLeaveLimitCreating(ModelBuilder modelBuilder)
    {
        var userLeaveLimitProperties = new TypeToJsonConverter<UserLeaveLimit.UserLeaveLimitProperties>();
        modelBuilder.Entity<UserLeaveLimit>()
             .HasKey(e => e.Id);
        modelBuilder.Entity<UserLeaveLimit>()
            .Property(b => b.AssignedToUserId)
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
            .HasColumnType("jsonb")
            .HasConversion(userLeaveLimitProperties);
        modelBuilder.Entity<UserLeaveLimit>()
            .HasOne(l => l.LeaveType)
            .WithMany(t => t.UserLeaveLimits)
            .HasForeignKey(l => l.LeaveTypeId)
            .HasPrincipalKey(t => t.Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<UserLeaveLimit>()
            .HasIndex(p => new { p.LeaveTypeId, p.AssignedToUserId, p.ValidSince }).IsUnique();
        modelBuilder.Entity<UserLeaveLimit>()
            .HasIndex(p => new { p.LeaveTypeId, p.AssignedToUserId, p.ValidUntil }).IsUnique();
    }

    private static void OnLeaveTypeCreating(ModelBuilder modelBuilder)
    {
        var leaveTypePropertiesToJsonConverter = new TypeToJsonConverter<LeaveType.LeaveTypeProperties>();
        modelBuilder.Entity<LeaveType>()
             .HasKey(e => e.Id);
        modelBuilder.Entity<LeaveType>()
            .Property(b => b.Name)
            .IsRequired();
        modelBuilder.Entity<LeaveType>()
            .Property(b => b.Properties)
            .IsRequired(false)
            .HasColumnType("jsonb")
            .HasConversion(leaveTypePropertiesToJsonConverter);
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
