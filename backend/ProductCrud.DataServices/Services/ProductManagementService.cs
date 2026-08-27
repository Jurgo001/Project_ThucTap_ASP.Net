using ProductCrud.DataServices.Infrastructure;
using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Repositories;

namespace ProductCrud.DataServices.Services;

public class ProductManagementService : IProductManagementService
{
    private readonly IProductManagementRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ProductManagementService(
        IProductManagementRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<ResultModel<List<ProductDTO>>> GetAllAsync(ProductFilterDTO filter)
    {
        filter.PageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
        filter.PageSize = filter.PageSize < 1 ? 5 : Math.Min(filter.PageSize, 100);

        if (string.IsNullOrWhiteSpace(filter.SortField))
        {
            filter.SortField = "Id";
        }

        filter.SortDirection = string.Equals(
            filter.SortDirection,
            "asc",
            StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        var (items, totalRecords) = await _repository.GetAllAsync(filter);

        return ResultModel<List<ProductDTO>>.Ok(
            items,
            "Lấy danh sách thành công.",
            totalRecords);
    }

    public async Task<ResultModel<ProductDTO>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id sản phẩm không hợp lệ.");
        }

        var product = await _repository.GetByIdAsync(id);

        if (product is null)
        {
            throw new KeyNotFoundException("Không tìm thấy sản phẩm.");
        }

        return ResultModel<ProductDTO>.Ok(product);
    }

    public async Task<ResultModel<int>> CreateAsync(ProductModel model)
    {
        Validate(model);

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được người dùng đăng nhập.");

        var newId = await _repository.CreateAsync(model, userId);

        return ResultModel<int>.Ok(newId, "Thêm sản phẩm thành công.");
    }

    public async Task<ResultModel<bool>> UpdateAsync(ProductModel model)
    {
        if (model.Id <= 0)
        {
            throw new ArgumentException("Id sản phẩm không hợp lệ.");
        }

        Validate(model);

        if (!await _repository.UpdateAsync(model))
        {
            throw new KeyNotFoundException("Không tìm thấy sản phẩm cần sửa.");
        }

        return ResultModel<bool>.Ok(true, "Cập nhật sản phẩm thành công.");
    }

    public async Task<ResultModel<bool>> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id sản phẩm không hợp lệ.");
        }

        if (!await _repository.DeleteAsync(id))
        {
            throw new KeyNotFoundException("Không tìm thấy sản phẩm cần xóa.");
        }

        return ResultModel<bool>.Ok(true, "Xóa sản phẩm thành công.");
    }

    private static void Validate(ProductModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ProductCode))
        {
            throw new ArgumentException("Mã sản phẩm không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(model.ProductName))
        {
            throw new ArgumentException("Tên sản phẩm không được để trống.");
        }

        if (model.Price < 0)
        {
            throw new ArgumentException("Giá sản phẩm không được âm.");
        }

        if (model.Quantity < 0)
        {
            throw new ArgumentException("Số lượng không được âm.");
        }
    }
}
