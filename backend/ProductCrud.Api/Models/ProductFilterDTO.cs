namespace ProductCrud.Api.Models;

public class ProductFilterDTO
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string SortField { get; set; } = "Id";
    public string SortDirection { get; set; } = "desc";
}
