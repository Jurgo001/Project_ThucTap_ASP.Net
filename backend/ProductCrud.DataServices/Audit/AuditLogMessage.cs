using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCrud.DataServices.Audit;

public class AuditLogMessage
{
    public int? UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}