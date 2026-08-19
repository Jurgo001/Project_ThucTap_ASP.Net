using ProductCrud.Api.Models;
using ProductCrud.Api.Models.Files;

namespace ProductCrud.Api.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public FileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<ResultModel<FileUploadResultDTO>> UploadAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Vui lòng chọn file cần tải lên.");
        }

        var maxSizeBytes = _configuration.GetValue<long?>("FileUpload:MaxSizeBytes")
            ?? 5 * 1024 * 1024;

        if (file.Length > maxSizeBytes)
        {
            throw new ArgumentException("File vượt quá kích thước cho phép 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = _configuration
            .GetSection("FileUpload:AllowedExtensions")
            .Get<string[]>()
            ?? new[] { ".jpg", ".jpeg", ".png", ".pdf" };

        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Chỉ cho phép file JPG, JPEG, PNG hoặc PDF.");
        }

        if (!await HasValidFileSignatureAsync(file, extension))
        {
            throw new ArgumentException("Nội dung file không đúng định dạng cho phép.");
        }

        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadDirectory = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadDirectory, storedFileName);

        await using (var fileStream = new FileStream(
            physicalPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None))
        {
            await file.CopyToAsync(fileStream);
        }

        var result = new FileUploadResultDTO
        {
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            Url = $"/uploads/{storedFileName}",
            Size = file.Length
        };

        return ResultModel<FileUploadResultDTO>.Ok(result, "Tải file lên thành công.");
    }

    private static async Task<bool> HasValidFileSignatureAsync(
        IFormFile file,
        string extension)
    {
        var header = new byte[8];

        await using var readStream = file.OpenReadStream();
        var bytesRead = await readStream.ReadAsync(header.AsMemory(0, header.Length));

        return extension switch
        {
            ".jpg" or ".jpeg" =>
                bytesRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,
            ".png" =>
                bytesRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A,
            ".pdf" =>
                bytesRead >= 4 &&
                header[0] == 0x25 &&
                header[1] == 0x50 &&
                header[2] == 0x44 &&
                header[3] == 0x46,
            _ => false
        };
    }
}
