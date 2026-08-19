namespace ProductCrud.Api.Models.Audit;

public class AuditLogFilterDTO
{
    public string? Keyword { get; set; }
    public string? Action { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
