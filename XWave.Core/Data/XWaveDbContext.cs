using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XWave.Core.Models;

namespace XWave.Core.Data;

internal class XWaveDbContext : IdentityDbContext<ApplicationUser>
{
    public XWaveDbContext(DbContextOptions<XWaveDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Category { get; set; } = null!;
    public DbSet<Discount> Discount { get; set; } = null!;
    public DbSet<Product> Product { get; set; } = null!;
    public DbSet<PaymentAccount> PaymentAccount { get; set; } = null!;
    public DbSet<PaymentAccountDetails> PaymentAccountDetails { get; set; } = null!;
    public DbSet<Order> Order { get; set; } = null!;
    public DbSet<OrderDetails> OrderDetails { get; set; } = null!;
    public DbSet<CustomerAccount> CustomerAccount { get; set; } = null!;
    public DbSet<StaffAccount> StaffAccount { get; set; } = null!;
    public DbSet<Activity> Activity { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OrderDetails>(entity =>
        {
            entity.HasKey(od => new { od.OrderId, od.ProductId });
        });

        builder.Entity<PaymentAccount>(entity =>
        {
            entity
                .HasIndex(p => new { p.AccountNumber, p.Provider })
                .IsUnique();
        });

        builder.Entity<PaymentAccountDetails>(entity =>
        {
            entity.HasKey(pd => new { pd.CustomerId, pd.PaymentAccountId });
            entity.HasQueryFilter(p => !p.Payment.IsDeleted);
        });

        builder.Entity<Product>(entity =>
        {
            entity
                .HasOne(x => x.Discount)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.DiscountId)
                 .HasPrincipalKey(x => x.Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        builder.Entity<Activity>(entity =>
        {
            entity
                .HasOne(activityLog => activityLog.AppUser)
                .WithMany()
                .HasForeignKey(activityLog => activityLog.UserId);

            entity
                .Property(a => a.OperationType)
                .HasConversion(new EnumToStringConverter<OperationType>());
        });

        builder.Entity<StaffAccount>(entity =>
        {
            entity
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<StaffAccount>(s => s.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            entity
                .HasOne(sa => sa.ImmediateManager)
                .WithMany()
                .HasForeignKey(s => s.ImmediateManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasQueryFilter(sa => !sa.IsDeleted);
        });

        builder.Entity<CustomerAccount>(entity =>
        {
            entity
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<CustomerAccount>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}