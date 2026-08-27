using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Models.Audit;

namespace ProductCrud.DataServices.Services;

public interface IAuditLogService
{
    Task<ResultModel<List<AuditLogDTO>>> GetAllAsync(AuditLogFilterDTO filter);
}
