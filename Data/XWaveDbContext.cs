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
        public DbSet<PaymentAccount> PaymentAccount { get; set; }
        public DbSet<PaymentAccountDetails> PaymentAccountDetails { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<CustomerAccount> CustomerAccount { get; set; }
        public DbSet<StaffAccount> StaffAccount { get; set; }
        public DbSet<Activity> Activity { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<OrderDetails>()
                .HasKey(od => new { od.OrderId, od.ProductId });

            //builder.Entity<OrderDetails>()
            //    .HasIndex(od => od.OrderId)
            //    .IsUnique(false);

            builder.Entity<PaymentAccount>()
                .HasIndex(p => new { p.AccountNumber, p.Provider })
                .IsUnique();

            builder.Entity<PaymentAccount>()
                .HasQueryFilter(p => !p.IsDeleted);

            builder.Entity<PaymentAccountDetails>()
                .HasKey(pd => new { pd.CustomerId, pd.PaymentAccountId });

            builder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            builder.Entity<Discount>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId);

            builder.Entity<Discount>()
                .HasMany(d => d.Products)
                .WithOne(p => p.Discount)
                .HasForeignKey(p => p.DiscountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Activity>()
                .HasOne(activityLog => activityLog.AppUser)
                .WithMany()
                .HasForeignKey(activityLog => activityLog.UserId);

            builder.Entity<Activity>()
                .Property(a => a.OperationType)
                .HasConversion(new EnumToStringConverter<OperationType>());

            builder.Entity<StaffAccount>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<StaffAccount>(s => s.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StaffAccount>()
                .HasOne(sa => sa.ImmediateManager)
                .WithMany()
                .HasForeignKey(s => s.ImmediateManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StaffAccount>()
                .HasQueryFilter(sa => !sa.IsDeleted);

            builder.Entity<CustomerAccount>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<CustomerAccount>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}