using ProductCrud.Api.Models.Audit;

namespace ProductCrud.Api.Repositories;

public interface IAuditLogRepository
{
    Task<(List<AuditLogDTO> Items, int TotalRecords)> GetAllAsync(AuditLogFilterDTO filter);
}
