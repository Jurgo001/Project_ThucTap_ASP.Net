using Microsoft.EntityFrameworkCore;
using ProductCrud.Api.Data;
using ProductCrud.Api.Models;
using ProductCrud.Api.Models.Entities;

namespace ProductCrud.Api.Repositories;

public class ProductManagementRepository : IProductManagementRepository
{
    private readonly ProductCrudDbContext _dbContext;

    public ProductManagementRepository(ProductCrudDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<ProductDTO> Items, int TotalRecords)> GetAllAsync(ProductFilterDTO filter)
    {
        IQueryable<ProductEntity> query = _dbContext.Products
            .AsNoTracking()
            .Where(product => !product.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(product =>
                product.ProductCode.Contains(keyword) ||
                product.ProductName.Contains(keyword));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(product => product.IsActive == filter.IsActive.Value);
        }

        var totalRecords = await query.CountAsync();

        query = ApplySorting(query, filter.SortField, filter.SortDirection);

        var items = await query
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(product => new ProductDTO
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = product.Quantity,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                ModifiedDate = product.ModifiedDate
            })
            .ToListAsync();

        return (items, totalRecords);
    }

    public async Task<ProductDTO?> GetByIdAsync(int id)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id && !product.IsDeleted)
            .Select(product => new ProductDTO
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = product.Quantity,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                ModifiedDate = product.ModifiedDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(ProductModel model, int createdByUserId)
    {
        if (await IsDuplicateProductCodeAsync(model.ProductCode))
        {
            throw new InvalidOperationException("Mã sản phẩm đã tồn tại.");
        }

        var product = new ProductEntity
        {
            ProductCode = model.ProductCode.Trim(),
            ProductName = model.ProductName.Trim(),
            Price = model.Price,
            Quantity = model.Quantity,
            IsActive = model.IsActive,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdByUserId
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product.Id;
    }

    public async Task<bool> UpdateAsync(ProductModel model)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(item => item.Id == model.Id && !item.IsDeleted);

        if (product is null)
        {
            return false;
        }

        if (await IsDuplicateProductCodeAsync(model.ProductCode, model.Id))
        {
            throw new InvalidOperationException("Mã sản phẩm đã tồn tại.");
        }

        product.ProductCode = model.ProductCode.Trim();
        product.ProductName = model.ProductName.Trim();
        product.Price = model.Price;
        product.Quantity = model.Quantity;
        product.IsActive = model.IsActive;
        product.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);

        if (product is null)
        {
            return false;
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<bool> IsDuplicateProductCodeAsync(string productCode, int? excludedId = null)
    {
        var normalizedProductCode = productCode.Trim();

        return await _dbContext.Products.AnyAsync(product =>
            product.ProductCode == normalizedProductCode &&
            !product.IsDeleted &&
            (!excludedId.HasValue || product.Id != excludedId.Value));
    }

    private static IQueryable<ProductEntity> ApplySorting(
        IQueryable<ProductEntity> query,
        string sortField,
        string sortDirection)
    {
        var isDescending = string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        return sortField.Trim().ToLowerInvariant() switch
        {
            "productcode" => isDescending
                ? query.OrderByDescending(product => product.ProductCode)
                : query.OrderBy(product => product.ProductCode),
            "productname" => isDescending
                ? query.OrderByDescending(product => product.ProductName)
                : query.OrderBy(product => product.ProductName),
            "price" => isDescending
                ? query.OrderByDescending(product => product.Price)
                : query.OrderBy(product => product.Price),
            "quantity" => isDescending
                ? query.OrderByDescending(product => product.Quantity)
                : query.OrderBy(product => product.Quantity),
            "isactive" => isDescending
                ? query.OrderByDescending(product => product.IsActive)
                : query.OrderBy(product => product.IsActive),
            "createddate" => isDescending
                ? query.OrderByDescending(product => product.CreatedDate)
                : query.OrderBy(product => product.CreatedDate),
            _ => isDescending
                ? query.OrderByDescending(product => product.Id)
                : query.OrderBy(product => product.Id)
        };
    }
}
