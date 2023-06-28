﻿// <auto-generated />
using System;
using System.Text.Json;
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
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BaseLeaveTypeId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<LeaveType.LeaveTypeProperties>("Properties")
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.HasIndex("BaseLeaveTypeId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("LeaveTypes");
                });

            modelBuilder.Entity("LeaveSystem.Db.Entities.Setting", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<JsonDocument>("Value")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("LeaveSystem.Db.Entities.UserLeaveLimit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AssignedToUserId")
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

                    b.HasKey("Id");

                    b.HasIndex("LeaveTypeId", "AssignedToUserId", "ValidSince")
                        .IsUnique();

                    b.HasIndex("LeaveTypeId", "AssignedToUserId", "ValidUntil")
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
