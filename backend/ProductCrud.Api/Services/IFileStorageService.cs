using ProductCrud.Api.Models.Files;
using ProductCrud.DataServices.Models;

namespace ProductCrud.Api.Services;

public interface IFileStorageService
{
    Task<ResultModel<FileUploadResultDTO>> UploadAsync(IFormFile file);
}