﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestHosts.Database.TestBank;

namespace TestHosts.Migrations
{
    [DbContext(typeof(TestBankContext))]
    [Migration("20211014160720_InitialTestBankDb")]
    partial class InitialTestBankDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TestHosts.Database.TestBank.Deposit", b =>
                {
                    b.Property<Guid>("HostIdentifier")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("DepositId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccountNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Reference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("SentToHost")
                        .HasColumnType("bit");

                    b.Property<string>("SortCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("HostIdentifier", "DepositId");

                    b.ToTable("deposit");
                });

            modelBuilder.Entity("TestHosts.Database.TestBank.HostConfiguration", b =>
                {
                    b.Property<Guid>("HostIdentifier")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccountNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CallbackUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SortCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("HostIdentifier");

                    b.ToTable("hostconfiguration");
                });
#pragma warning restore 612, 618
        }
    }
}
