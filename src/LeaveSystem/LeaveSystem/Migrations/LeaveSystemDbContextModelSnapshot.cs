﻿// <auto-generated />
using System;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveSystem.Migrations
{
    [DbContext(typeof(LeaveSystemDbContext))]
    partial class LeaveSystemDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LeaveSystem.Db.Entities.LeaveType", b =>
                {
                    b.Property<Guid>("LeaveTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BaseLeaveTypeId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<LeaveType.LeaveTypeProperties>("Properties")
                        .HasColumnType("jsonb");

                    b.HasKey("LeaveTypeId");

                    b.HasIndex("BaseLeaveTypeId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("LeaveTypes");
                });

            modelBuilder.Entity("LeaveSystem.Db.Entities.Role", b =>
                {
                    b.Property<Guid>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RoleType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("RoleId");

                    b.HasIndex("RoleType", "Email")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("LeaveSystem.Db.Entities.UserLeaveLimit", b =>
                {
                    b.Property<Guid>("UserLeaveLimitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AssignedToUserEmail")
                        .HasColumnType("text");

                    b.Property<Guid>("LeaveTypeId")
                        .HasColumnType("uuid");

                    b.Property<TimeSpan?>("Limit")
                        .HasColumnType("interval");

                    b.Property<TimeSpan?>("OverdueLimit")
                        .HasColumnType("interval");

                    b.Property<UserLeaveLimit.UserLeaveLimitProperties>("Property")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("ValidSince")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("ValidUntil")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("UserLeaveLimitId");

                    b.HasIndex("LeaveTypeId", "AssignedToUserEmail", "ValidSince")
                        .IsUnique();

                    b.HasIndex("LeaveTypeId", "AssignedToUserEmail", "ValidUntil")
                        .IsUnique();

                    b.ToTable("UserLeaveLimits");
                });

            modelBuilder.Entity("LeaveSystem.Db.Entities.LeaveType", b =>
                {
                    b.HasOne("LeaveSystem.Db.Entities.LeaveType", "BaseLeaveType")
                        .WithMany("ConstraintedLeaveTypes")
                        .HasForeignKey("BaseLeaveTypeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("BaseLeaveType");
                });

            modelBuilder.Entity("LeaveSystem.Db.Entities.UserLeaveLimit", b =>
                {
                    b.HasOne("LeaveSystem.Db.Entities.LeaveType", "LeaveType")
                        .WithMany("UserLeaveLimits")
                        .HasForeignKey("LeaveTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("LeaveType");
                });

            modelBuilder.Entity("LeaveSystem.Db.Entities.LeaveType", b =>
                {
                    b.Navigation("ConstraintedLeaveTypes");

                    b.Navigation("UserLeaveLimits");
                });
#pragma warning restore 612, 618
        }
    }
}
