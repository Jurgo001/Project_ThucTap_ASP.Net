using ProductCrud.Api.Models;
using ProductCrud.Api.Models.Files;

namespace ProductCrud.Api.Services;

public interface IFileStorageService
{
    Task<ResultModel<FileUploadResultDTO>> UploadAsync(IFormFile file);
}
