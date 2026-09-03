using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductCrud.DataServices.Entities;
using ProductCrud.DataServices.Audit;

namespace ProductCrud.DataServices.Infrastructure;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogQueue _auditLogQueue;

    private readonly List<AuditLogMessage> _pendingAudits = new();
    public AuditLogInterceptor(
     ICurrentUserService currentUserService,
     IAuditLogQueue auditLogQueue)
    {
        _currentUserService = currentUserService;
        _auditLogQueue = auditLogQueue;
    }

    public override InterceptionResult<int> SavingChanges(
     DbContextEventData eventData,
     InterceptionResult<int> result)
    {
        CaptureAuditLogs(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
     DbContextEventData eventData,
     InterceptionResult<int> result,
     CancellationToken cancellationToken = default)
    {
        CaptureAuditLogs(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);

    }
    public override async ValueTask<int> SavedChangesAsync(
    SaveChangesCompletedEventData eventData,
    int result,
    CancellationToken cancellationToken = default)
    {
        foreach (var audit in _pendingAudits)
        {
            await _auditLogQueue.EnqueueAsync(
                audit,
                cancellationToken);
        }

        _pendingAudits.Clear();

        return await base.SavedChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
    public override Task SaveChangesFailedAsync(
    DbContextErrorEventData eventData,
    CancellationToken cancellationToken = default)
    {
        _pendingAudits.Clear();

        return base.SaveChangesFailedAsync(
            eventData,
            cancellationToken);
    }

    private void CaptureAuditLogs(DbContext? dbContext)
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

        foreach (var entry in productEntries)
        {
            var action = GetAction(entry);

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.ModifiedDate = createdDate;
            }

            _pendingAudits.Add(new AuditLogMessage
            {
                UserId = _currentUserService.UserId,
                Username = _currentUserService.Username,
                Action = action,
                EntityName = "Product",
                EntityId = entry.Entity.Id > 0
                    ? entry.Entity.Id.ToString()
                    : null,
                Description = BuildDescription(entry.Entity, action),
                CreatedDate = createdDate
            });
        }
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
