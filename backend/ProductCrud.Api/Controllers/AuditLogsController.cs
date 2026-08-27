using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCrud.DataServices.Models.Audit;
using ProductCrud.DataServices.Services;

namespace ProductCrud.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogFilterDTO filter)
    {
        return Ok(await _auditLogService.GetAllAsync(filter));
    }
}
