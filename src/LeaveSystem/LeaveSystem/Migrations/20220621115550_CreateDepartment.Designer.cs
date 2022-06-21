﻿// <auto-generated />
using System;
using LeaveSystem.Api.Domains;
using LeaveSystem.Db;
using LeaveSystem.Db.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeaveSystem.Migrations
{
    [DbContext(typeof(LeaveSystemDbContext))]
    [Migration("20220621115550_CreateDepartment")]
    partial class CreateDepartment
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LeaveSystem.Api.Domains.LeaveType", b =>
                {
                    b.Property<Guid>("LeaveTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BaseLeaveTypeId")
                        .HasColumnType("uuid");

                    b.Property<LeaveTypeProperties>("Properties")
                        .HasColumnType("jsonb");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LeaveTypeId");

                    b.HasIndex("BaseLeaveTypeId");

                    b.ToTable("LeaveTypes");
                });

            modelBuilder.Entity("LeaveSystem.Db.Domains.Department", b =>
                {
                    b.Property<Guid>("DepartmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Department.UserDepartment>("Users")
                        .HasColumnType("jsonb");

                    b.HasKey("DepartmentId");

                    b.ToTable("Department");
                });

            modelBuilder.Entity("LeaveSystem.Api.Domains.LeaveType", b =>
                {
                    b.HasOne("LeaveSystem.Api.Domains.LeaveType", "BaseLeaveType")
                        .WithMany("ConstraintedLeaveTypes")
                        .HasForeignKey("BaseLeaveTypeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("BaseLeaveType");
                });

            modelBuilder.Entity("LeaveSystem.Api.Domains.LeaveType", b =>
                {
                    b.Navigation("ConstraintedLeaveTypes");
                });
#pragma warning restore 612, 618
        }
    }
}
