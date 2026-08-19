namespace ProductCrud.Api.Models;

public class ResultModel<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int? TotalRecords { get; set; }

    public static ResultModel<T> Ok(T data, string message = "Thành công", int? totalRecords = null)
        => new() { Success = true, Message = message, Data = data, TotalRecords = totalRecords };

    public static ResultModel<T> Fail(string message)
        => new() { Success = false, Message = message };
}
