using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Models.Audit;
using ProductCrud.DataServices.Repositories;

namespace ProductCrud.DataServices.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<ResultModel<List<AuditLogDTO>>> GetAllAsync(AuditLogFilterDTO filter)
    {
        filter.PageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
        filter.PageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var (items, totalRecords) = await _auditLogRepository.GetAllAsync(filter);

        return ResultModel<List<AuditLogDTO>>.Ok(
            items,
            "Lấy lịch sử hoạt động thành công.",
            totalRecords);
    }
}
