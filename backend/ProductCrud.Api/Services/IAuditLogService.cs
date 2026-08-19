using ProductCrud.Api.Models;
using ProductCrud.Api.Models.Audit;

namespace ProductCrud.Api.Services;

public interface IAuditLogService
{
    Task<ResultModel<List<AuditLogDTO>>> GetAllAsync(AuditLogFilterDTO filter);
}
