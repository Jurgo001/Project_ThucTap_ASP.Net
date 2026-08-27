using ProductCrud.DataServices;
using ProductCrud.DataServices.Models.Audit;

namespace ProductCrud.DataServices.Repositories;

public interface IAuditLogRepository
{
    Task<(List<AuditLogDTO> Items, int TotalRecords)> GetAllAsync(AuditLogFilterDTO filter);
}
