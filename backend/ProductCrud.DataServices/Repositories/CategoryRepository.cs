using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductCrud.DataServices.Data;
using ProductCrud.DataServices.Entities;
using ProductCrud.DataServices.Models;

namespace ProductCrud.DataServices.Repositories;
public class CategoryRepository : ICategoryRepository
{
    private readonly ProductCrudDbContext _dbContext;

    public CategoryRepository(ProductCrudDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<CategoryDTO>> GetAllAsync()
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category =>
                !category.IsDeleted &&
                category.IsActive)
            .OrderBy(category => category.CategoryName)
            .Select(category => new CategoryDTO
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                IsActive = category.IsActive
            })
            .ToListAsync();

    }
    public async Task<CategoryDTO?> GetByIdAsync(int id)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category =>
                category.Id == id &&
                !category.IsDeleted)
            .Select(category => new CategoryDTO
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                IsActive = category.IsActive
            })
            .FirstOrDefaultAsync();
    }
    public async Task<int> CreateAsync(CategoryModel model)
    {
        var entity = new CategoryEntity
        {
            CategoryName = model.CategoryName.Trim(),
            IsActive = model.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        _dbContext.Categories.Add(entity);

        await _dbContext.SaveChangesAsync();

        return entity.Id;
    }
    public async Task<bool> UpdateAsync(CategoryModel model)
    {
        var entity = await _dbContext.Categories
            .FirstOrDefaultAsync(category =>
                category.Id == model.Id &&
                !category.IsDeleted);

        if (entity is null)
        {
            return false;
        }

        entity.CategoryName = model.CategoryName.Trim();
        entity.IsActive = model.IsActive;
        entity.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbContext.Categories
            .FirstOrDefaultAsync(category =>
                category.Id == id &&
                !category.IsDeleted);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }
}