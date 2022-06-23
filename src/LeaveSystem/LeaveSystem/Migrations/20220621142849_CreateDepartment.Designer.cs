﻿// <auto-generated />
using System;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveSystem.Migrations
{
    [DbContext(typeof(LeaveSystemDbContext))]
    [Migration("20220621142849_CreateDepartment")]
    partial class CreateDepartment
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LeaveSystem.Db.Domains.Department", b =>
                {
                    b.Property<Guid>("DepartmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<FederatedUser[]>("Users")
                        .HasColumnType("jsonb");

                    b.HasKey("DepartmentId");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("LeaveSystem.Db.Domains.LeaveType", b =>
                {
                    b.Property<Guid>("LeaveTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BaseLeaveTypeId")
                        .HasColumnType("uuid");

                    b.Property<LeaveType.LeaveTypeProperties>("Properties")
                        .HasColumnType("jsonb");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LeaveTypeId");

                    b.HasIndex("BaseLeaveTypeId");

                    b.ToTable("LeaveTypes");
                });

            modelBuilder.Entity("LeaveSystem.Db.Domains.LeaveType", b =>
                {
                    b.HasOne("LeaveSystem.Db.Domains.LeaveType", "BaseLeaveType")
                        .WithMany("ConstraintedLeaveTypes")
                        .HasForeignKey("BaseLeaveTypeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("BaseLeaveType");
                });

            modelBuilder.Entity("LeaveSystem.Db.Domains.LeaveType", b =>
                {
                    b.Navigation("ConstraintedLeaveTypes");
                });
#pragma warning restore 612, 618
        }
    }
}
