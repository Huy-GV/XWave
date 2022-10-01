﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XWave.Web.Data;
using XWave.Core.Data;

namespace XWave.Web.Data.Migrations;

[DbContext(typeof(XWaveDbContext))]
[Migration("20211211102117_OrderDetail")]
partial class OrderDetail
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("Relational:MaxIdentifierLength", 128)
            .HasAnnotation("ProductVersion", "5.0.9")
            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Name")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("NormalizedName")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.HasKey("Id");

                b.HasIndex("NormalizedName")
                    .IsUnique()
                    .HasDatabaseName("RoleNameIndex")
                    .HasFilter("[NormalizedName] IS NOT NULL");

                b.ToTable("AspNetRoles");
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<string>("ClaimType")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("ClaimValue")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("RoleId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.HasKey("Id");

                b.HasIndex("RoleId");

                b.ToTable("AspNetRoleClaims");
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<string>("ClaimType")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("ClaimValue")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("AspNetUserClaims");
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.Property<string>("LoginProvider")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ProviderKey")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ProviderDisplayName")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.HasKey("LoginProvider", "ProviderKey");

                b.HasIndex("UserId");

                b.ToTable("AspNetUserLogins");
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.Property<string>("UserId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("RoleId")
                    .HasColumnType("nvarchar(450)");

                b.HasKey("UserId", "RoleId");

                b.HasIndex("RoleId");

                b.ToTable("AspNetUserRoles");
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.Property<string>("UserId")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("LoginProvider")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Name")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Value")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("UserId", "LoginProvider", "Name");

                b.ToTable("AspNetUserTokens");
            });

        modelBuilder.Entity("XWave.Models.ApplicationUser", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("ConcurrencyStamp")
                    .IsConcurrencyToken()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Email")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<bool>("EmailConfirmed")
                    .HasColumnType("bit");

                b.Property<string>("FirstName")
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnType("nvarchar(15)");

                b.Property<string>("LastName")
                    .IsRequired()
                    .HasMaxLength(25)
                    .HasColumnType("nvarchar(25)");

                b.Property<string>("NormalizedEmail")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("NormalizedUserName")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("PasswordHash")
                    .HasColumnType("nvarchar(max)");

                b.Property<int?>("PaymentId")
                    .HasColumnType("int");

                b.Property<DateTime>("RegistrationDate")
                    .HasColumnType("datetime2");

                b.Property<string>("UserName")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.HasKey("Id");

                b.HasIndex("NormalizedEmail")
                    .HasDatabaseName("EmailIndex");

                b.HasIndex("NormalizedUserName")
                    .IsUnique()
                    .HasDatabaseName("UserNameIndex")
                    .HasFilter("[NormalizedUserName] IS NOT NULL");

                b.ToTable("AspNetUsers");
            });

        modelBuilder.Entity("XWave.Models.Category", b =>
            {
                b.Property<int>("ID")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Description")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnType("nvarchar(10)");

                b.HasKey("ID");

                b.ToTable("Category");
            });

        modelBuilder.Entity("XWave.Models.Customer", b =>
            {
                b.Property<string>("CustomerID")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Address")
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                b.Property<string>("Country")
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("nvarchar(20)");

                b.Property<int>("PhoneNumber")
                    .HasColumnType("int");

                b.HasKey("CustomerID");

                b.ToTable("Customer");
            });

        modelBuilder.Entity("XWave.Models.Discount", b =>
            {
                b.Property<int>("ID")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<DateTime>("EndDate")
                    .HasColumnType("datetime2");

                b.Property<long>("Percentage")
                    .HasColumnType("bigint");

                b.Property<DateTime>("StartDate")
                    .HasColumnType("datetime2");

                b.HasKey("ID");

                b.ToTable("Discount");
            });

        modelBuilder.Entity("XWave.Models.Order", b =>
            {
                b.Property<int>("ID")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<string>("CustomerID")
                    .HasColumnType("nvarchar(450)");

                b.Property<DateTime>("Date")
                    .HasColumnType("datetime2");

                b.Property<int>("PaymentID")
                    .HasColumnType("int");

                b.HasKey("ID");

                b.HasIndex("CustomerID");

                b.HasIndex("PaymentID");

                b.ToTable("Order");
            });

        modelBuilder.Entity("XWave.Models.OrderDetail", b =>
            {
                b.Property<int>("OrderID")
                    .HasColumnType("int");

                b.Property<int>("ProductID")
                    .HasColumnType("int");

                b.Property<decimal>("PriceAtOrder")
                    .HasColumnType("decimal(18,4)");

                b.Property<long>("Quantity")
                    .HasColumnType("bigint");

                b.HasKey("OrderID", "ProductID");

                b.HasIndex("OrderID")
                    .IsUnique();

                b.HasIndex("ProductID");

                b.ToTable("OrderDetail");
            });

        modelBuilder.Entity("XWave.Models.Payment", b =>
            {
                b.Property<int>("ID")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<long>("AccountNo")
                    .HasColumnType("bigint");

                b.Property<DateTime>("ExpiryDate")
                    .HasColumnType("datetime2");

                b.Property<string>("Provider")
                    .HasColumnType("nvarchar(450)");

                b.HasKey("ID");

                b.HasIndex("AccountNo", "Provider")
                    .IsUnique()
                    .HasFilter("[Provider] IS NOT NULL");

                b.ToTable("Payment");
            });

        modelBuilder.Entity("XWave.Models.PaymentDetail", b =>
            {
                b.Property<string>("CustomerID")
                    .HasColumnType("nvarchar(450)");

                b.Property<int>("PaymentID")
                    .HasColumnType("int");

                b.Property<DateTime>("LatestPurchase")
                    .HasColumnType("datetime2");

                b.Property<long>("PurchaseCount")
                    .HasColumnType("bigint");

                b.Property<DateTime>("Registration")
                    .HasColumnType("datetime2");

                b.HasKey("CustomerID", "PaymentID");

                b.HasIndex("PaymentID");

                b.ToTable("PaymentDetail");
            });

        modelBuilder.Entity("XWave.Models.Product", b =>
            {
                b.Property<int>("ID")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<int>("CategoryID")
                    .HasColumnType("int");

                b.Property<int?>("DiscountID")
                    .HasColumnType("int");

                b.Property<string>("ImagePath")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTime>("LastRestock")
                    .HasColumnType("datetime2");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnType("nvarchar(15)");

                b.Property<decimal>("Price")
                    .HasColumnType("decimal(18,4)");

                b.Property<long>("Quantity")
                    .HasColumnType("bigint");

                b.HasKey("ID");

                b.HasIndex("CategoryID");

                b.HasIndex("DiscountID");

                b.ToTable("Product");
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.HasOne("XWave.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.HasOne("XWave.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("XWave.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.HasOne("XWave.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("XWave.Models.Order", b =>
            {
                b.HasOne("XWave.Models.Customer", "Customer")
                    .WithMany()
                    .HasForeignKey("CustomerID");

                b.HasOne("XWave.Models.Payment", "Payment")
                    .WithMany()
                    .HasForeignKey("PaymentID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Customer");

                b.Navigation("Payment");
            });

        modelBuilder.Entity("XWave.Models.OrderDetail", b =>
            {
                b.HasOne("XWave.Models.Order", "Order")
                    .WithOne("Detail")
                    .HasForeignKey("XWave.Models.OrderDetail", "OrderID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("XWave.Models.Product", "Product")
                    .WithMany()
                    .HasForeignKey("ProductID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Order");

                b.Navigation("Product");
            });

        modelBuilder.Entity("XWave.Models.PaymentDetail", b =>
            {
                b.HasOne("XWave.Models.Customer", "Customer")
                    .WithMany()
                    .HasForeignKey("CustomerID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("XWave.Models.Payment", "Payment")
                    .WithMany()
                    .HasForeignKey("PaymentID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Customer");

                b.Navigation("Payment");
            });

        modelBuilder.Entity("XWave.Models.Product", b =>
            {
                b.HasOne("XWave.Models.Category", "Category")
                    .WithMany()
                    .HasForeignKey("CategoryID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("XWave.Models.Discount", "Discount")
                    .WithMany()
                    .HasForeignKey("DiscountID");

                b.Navigation("Category");

                b.Navigation("Discount");
            });

        modelBuilder.Entity("XWave.Models.Order", b =>
            {
                b.Navigation("Detail");
            });
#pragma warning restore 612, 618
    }
}
