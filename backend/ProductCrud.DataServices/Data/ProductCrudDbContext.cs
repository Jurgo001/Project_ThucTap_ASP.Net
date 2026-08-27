using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductCrud.DataServices.Entities;

namespace ProductCrud.DataServices.Data;

public class ProductCrudDbContext : DbContext
{
    public ProductCrudDbContext(DbContextOptions<ProductCrudDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<AppUserEntity> AppUsers => Set<AppUserEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var utcDateTimeConverter = new ValueConverter<DateTime, DateTime>(
            value => value,
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        var nullableUtcDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            value => value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value);

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("ProductList");
            entity.HasKey(product => product.Id);
            entity.HasIndex(product => product.ProductCode)
                  .IsUnique()
                  .HasFilter("[IsDeleted] = 0");

            entity.Property(product => product.ProductCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(product => product.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(product => product.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(product => product.IsActive)
                .HasDefaultValue(true);

            entity.Property(product => product.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(product => product.CreatedDate)
                .HasConversion(utcDateTimeConverter)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(product => product.ModifiedDate)
                .HasConversion(nullableUtcDateTimeConverter);
        });

        modelBuilder.Entity<AppUserEntity>(entity =>
        {
            entity.ToTable("AppUsers");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Username).IsUnique();

            entity.Property(user => user.Username)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(user => user.IsActive)
                .HasDefaultValue(true);

            entity.Property(user => user.CreatedDate)
                .HasConversion(utcDateTimeConverter)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<AuditLogEntity>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(auditLog => auditLog.Id);

            entity.Property(auditLog => auditLog.Username)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(auditLog => auditLog.Action)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(auditLog => auditLog.EntityName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(auditLog => auditLog.EntityId)
                .HasMaxLength(100);

            entity.Property(auditLog => auditLog.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(auditLog => auditLog.CreatedDate)
                .HasConversion(utcDateTimeConverter)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories");

            entity.HasKey(category => category.Id);

            entity.Property(category => category.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(category => category.IsActive)
                .HasDefaultValue(true);

            entity.Property(category => category.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(category => category.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
