﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestHosts.Database.PataPawa;

#nullable disable

namespace TestHosts.Migrations.PataPawa
{
    [DbContext(typeof(PataPawaContext))]
    [Migration("20240204113648_AddPrePayUsersAndMeters")]
    partial class AddPrePayUsersAndMeters
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TestHosts.Database.PataPawa.PostPaidAccount", b =>
                {
                    b.Property<Guid>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ApiKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AccountId");

                    b.ToTable("postpaidaccounts");
                });

            modelBuilder.Entity("TestHosts.Database.PataPawa.PostPaidBill", b =>
                {
                    b.Property<Guid>("PostPaidBillId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccountName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AccountNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsFullyPaid")
                        .HasColumnType("bit");

                    b.HasKey("PostPaidBillId");

                    b.ToTable("postpaidbill");
                });

            modelBuilder.Entity("TestHosts.Database.PataPawa.PrePayMeter", b =>
                {
                    b.Property<string>("MeterNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("MeterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("MeterNumber");

                    b.ToTable("PrePayMeters");
                });

            modelBuilder.Entity("TestHosts.Database.PataPawa.PrePayUser", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("PrePayUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
