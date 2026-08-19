using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductCrud.Api.Models.Entities;

namespace ProductCrud.Api.Infrastructure;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditLogInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var productEntries = dbContext.ChangeTracker
            .Entries<ProductEntity>()
            .Where(entry =>
                entry.State == EntityState.Added ||
                entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            .ToList();

        if (productEntries.Count == 0)
        {
            return;
        }

        var createdDate = DateTime.UtcNow;
        var auditLogs = new List<AuditLogEntity>();

        foreach (var entry in productEntries)
        {
            var action = GetAction(entry);

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.ModifiedDate = createdDate;
            }

            auditLogs.Add(new AuditLogEntity
            {
                UserId = _currentUserService.UserId,
                Username = _currentUserService.Username,
                Action = action,
                EntityName = "Product",
                EntityId = entry.Entity.Id > 0 ? entry.Entity.Id.ToString() : null,
                Description = BuildDescription(entry.Entity, action),
                CreatedDate = createdDate
            });
        }

        dbContext.Set<AuditLogEntity>().AddRange(auditLogs);
    }

    private static string GetAction(EntityEntry<ProductEntity> entry)
    {
        return entry.State switch
        {
            EntityState.Added => "CREATE",
            EntityState.Deleted => "DELETE",
            _ => "UPDATE"
        };
    }

    private static string BuildDescription(ProductEntity product, string action)
    {
        return $"{action} sản phẩm {product.ProductCode} - {product.ProductName}";
    }
}
