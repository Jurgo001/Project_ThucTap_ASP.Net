using ProductCrud.Api.Models;

namespace ProductCrud.Api.Repositories;

public interface IProductManagementRepository
{
    Task<(List<ProductDTO> Items, int TotalRecords)> GetAllAsync(ProductFilterDTO filter);
    Task<ProductDTO?> GetByIdAsync(int id);
    Task<int> CreateAsync(ProductModel model, int createdByUserId);
    Task<bool> UpdateAsync(ProductModel model);
    Task<bool> DeleteAsync(int id);
}
