using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public DbSet<PaymentAccount> Payment { get; set; }
        public DbSet<TransactionDetails> PaymentDetail { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<ActivityLog> StaffActivityLog { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<OrderDetail>()
                .HasKey(od => new { od.OrderId, od.ProductId });

            builder.Entity<OrderDetail>()
                .HasIndex(od => od.OrderId)
                .IsUnique(false);
                
            builder.Entity<PaymentAccount>()
                .HasIndex(p => new { p.AccountNo, p.Provider })
                .IsUnique();

            builder.Entity<TransactionDetails>()
                .HasKey(pd => new { pd.CustomerId, pd.PaymentAccountId });

            builder.Entity<TransactionDetails>()
                .Property(td => td.TransactionType)
                .HasConversion(new EnumToStringConverter<TransactionType>());

            builder.Entity<Product>()
                .HasOne(p => p.Discount)
                .WithMany(d => d.Products)
                .HasForeignKey(p => p.DiscountID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Discount>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId);

            builder.Entity<ActivityLog>()
                .HasOne(activityLog => activityLog.StaffUser)
                .WithMany()
                .HasForeignKey(activityLog => activityLog.StaffId);

            builder.Entity<ActivityLog>()
                .Property(a => a.OperationType)
                .HasConversion(new EnumToStringConverter<OperationType>());

            builder.Entity<Staff>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Staff>(s => s.StaffId);

            builder.Entity<Customer>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Customer>(c => c.CustomerId);
        }
    }
}
