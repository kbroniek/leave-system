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
    [Migration("20220621100229_CreateLeaveType")]
    partial class CreateLeaveType
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

                    b.Property<LeaveType.LeaveTypeProperties>("Properties")
                        .HasColumnType("jsonb");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LeaveTypeId");

                    b.HasIndex("BaseLeaveTypeId");

                    b.ToTable("LeaveTypes");
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
