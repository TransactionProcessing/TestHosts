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
    [Migration("20240205104654_storeprepaytransactions")]
    partial class storeprepaytransactions
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

            modelBuilder.Entity("TestHosts.Database.PataPawa.Transaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TransactionId"));

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Messaage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MeterNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Reference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResultCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("StandardTokenAmt")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("StandardTokenRctNum")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("StandardTokenTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Units")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Vendor")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TransactionId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("TestHosts.Database.PataPawa.TransactionCharge", b =>
                {
                    b.Property<int>("TransactionChargeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TransactionChargeId"));

                    b.Property<decimal>("ERCCharge")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ForexCharge")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("FuelIndexCharge")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("InflationAdjustment")
                        .HasColumnType("int");

                    b.Property<decimal>("MonthlyFC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("REPCharge")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("TransactionId")
                        .HasColumnType("int");

                    b.HasKey("TransactionChargeId");

                    b.HasIndex("TransactionId");

                    b.ToTable("TransactionCharges");
                });

            modelBuilder.Entity("TestHosts.Database.PataPawa.TransactionCharge", b =>
                {
                    b.HasOne("TestHosts.Database.PataPawa.Transaction", null)
                        .WithMany("Charges")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TestHosts.Database.PataPawa.Transaction", b =>
                {
                    b.Navigation("Charges");
                });
#pragma warning restore 612, 618
        }
    }
}
