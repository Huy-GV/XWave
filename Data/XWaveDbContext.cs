using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Models;

namespace XWave.Data
{
    public class XWaveDbContext : IdentityDbContext<ApplicationUser>
    {
        public XWaveDbContext(DbContextOptions<XWaveDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Discount> Discount { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentDetail> PaymentDetail { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<ActivityLog> StaffActivityLog { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<OrderDetail>()
                .HasKey(od => new { od.OrderID, od.ProductID });

            builder.Entity<OrderDetail>()
                .HasIndex(od => od.OrderID)
                .IsUnique(false);
                
            builder.Entity<Payment>()
                .HasIndex(p => new { p.AccountNo, p.Provider })
                .IsUnique();

            builder.Entity<PaymentDetail>()
                .HasKey(pd => new { pd.CustomerID, pd.PaymentID });

            builder.Entity<Product>()
                .HasOne(p => p.Discount)
                .WithMany(d => d.Products)
                .HasForeignKey(p => p.DiscountID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Discount>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerID);

            builder.Entity<ActivityLog>()
                .HasOne(activityLog => activityLog.StaffUser)
                .WithMany()
                .HasForeignKey(activityLog => activityLog.StaffID);

            builder.Entity<ActivityLog>()
                .Property(a => a.OperationType)
                .HasConversion(new EnumToStringConverter<OperationType>());
        }
    }
}
