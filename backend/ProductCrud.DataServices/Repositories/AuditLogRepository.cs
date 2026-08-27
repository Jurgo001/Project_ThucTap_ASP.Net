using Microsoft.EntityFrameworkCore;
using ProductCrud.DataServices.Data;
using ProductCrud.DataServices.Models.Audit;
using ProductCrud.DataServices.Entities;

namespace ProductCrud.DataServices.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ProductCrudDbContext _dbContext;

    public AuditLogRepository(ProductCrudDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<AuditLogDTO> Items, int TotalRecords)> GetAllAsync(
        AuditLogFilterDTO filter)
    {
        IQueryable<AuditLogEntity> query = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(auditLog =>
                auditLog.Username.Contains(keyword) ||
                auditLog.Description.Contains(keyword) ||
                auditLog.EntityName.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            var action = filter.Action.Trim().ToUpperInvariant();
            query = query.Where(auditLog => auditLog.Action == action);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderByDescending(auditLog => auditLog.CreatedDate)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(auditLog => new AuditLogDTO
            {
                Id = auditLog.Id,
                UserId = auditLog.UserId,
                Username = auditLog.Username,
                Action = auditLog.Action,
                EntityName = auditLog.EntityName,
                EntityId = auditLog.EntityId,
                Description = auditLog.Description,
                CreatedDate = auditLog.CreatedDate
            })
            .ToListAsync();

        return (items, totalRecords);
    }
}
