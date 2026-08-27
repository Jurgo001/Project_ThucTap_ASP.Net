using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Repositories;
using ProductCrud.DataServices.Infrastructure.Caching;

namespace ProductCrud.DataServices.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ICacheService _cache;

    public CategoryService(
        ICategoryRepository repository,
        ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ResultModel<List<CategoryDTO>>> GetAllAsync()
    {
        if (_cache.TryGetValue(
            CacheKeys.Categories,
            out List<CategoryDTO>? cachedCategories))
        {
            Console.WriteLine("CACHE HIT - Categories");

            return ResultModel<List<CategoryDTO>>.Ok(
                cachedCategories!,
                "Lấy danh sách danh mục thành công.");
        }

        Console.WriteLine(
            "CACHE MISS - Query Categories từ database");

        var categories =
            await _repository.GetAllAsync();

        _cache.Set(
            CacheKeys.Categories,
            categories,
            TimeSpan.FromMinutes(10));

        return ResultModel<List<CategoryDTO>>.Ok(
            categories,
            "Lấy danh sách danh mục thành công.");
    }

    public async Task<ResultModel<CategoryDTO>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id danh mục không hợp lệ.");
        }

        var category = await _repository.GetByIdAsync(id);

        if (category is null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy danh mục.");
        }

        return ResultModel<CategoryDTO>.Ok(category);
    }

    public async Task<ResultModel<int>> CreateAsync(CategoryModel model)
    {
        Validate(model);

        var id =await _repository.CreateAsync(model);

        _cache.Remove(CacheKeys.Categories);

        Console.WriteLine(
            "CACHE INVALIDATED - Categories");

        return ResultModel<int>.Ok(
            id,
            "Thêm danh mục thành công.");
    }

    public async Task<ResultModel<bool>> UpdateAsync(CategoryModel model)
    {
        if (model.Id <= 0)
        {
            throw new ArgumentException(
                "Id danh mục không hợp lệ.");
        }

        Validate(model);

        if (!await _repository.UpdateAsync(model))
        {
            throw new KeyNotFoundException(
                "Không tìm thấy danh mục cần sửa.");
        }

        _cache.Remove(CacheKeys.Categories);
        Console.WriteLine("CACHE INVALIDATED - Categories");

        return ResultModel<bool>.Ok(
            true,
            "Cập nhật danh mục thành công.");
    }

    public async Task<ResultModel<bool>> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Id danh mục không hợp lệ.");
        }

        if (!await _repository.DeleteAsync(id))
        {
            throw new KeyNotFoundException(
                "Không tìm thấy danh mục cần xóa.");
        }

        _cache.Remove(CacheKeys.Categories);
        Console.WriteLine("CACHE INVALIDATED - Categories");

        return ResultModel<bool>.Ok(
            true,
            "Xóa danh mục thành công.");
    }

    private static void Validate(CategoryModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CategoryName))
        {
            throw new ArgumentException(
                "Tên danh mục không được để trống.");
        }
    }
}