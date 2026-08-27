using ProductCrud.DataServices.Models;

namespace ProductCrud.DataServices.Services;

public interface IProductManagementService
{
    Task<ResultModel<List<ProductDTO>>> GetAllAsync(ProductFilterDTO filter);
    Task<ResultModel<ProductDTO>> GetByIdAsync(int id);
    Task<ResultModel<int>> CreateAsync(ProductModel model);
    Task<ResultModel<bool>> UpdateAsync(ProductModel model);
    Task<ResultModel<bool>> DeleteAsync(int id);
}
