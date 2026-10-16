using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Repositories;
using ProductCrud.DataServices.Infrastructure.Caching;
using Microsoft.Extensions.Logging;

namespace ProductCrud.DataServices.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository repository,
        ICacheService cache,
        ILogger<CategoryService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ResultModel<List<CategoryDTO>>> GetAllAsync()
    {
        var cachedCategories =
            await _cache.GetAsync<List<CategoryDTO>>(
                CacheKeys.Categories);

        if (cachedCategories is not null)
        {
            _logger.LogInformation(
                "REDIS HIT - Categories");

            return ResultModel<List<CategoryDTO>>.Ok(
                cachedCategories,
                "Lấy danh sách danh mục thành công.");
        }

        _logger.LogInformation(
            "REDIS MISS - Query Categories from database");

        var categories =
            await _repository.GetAllAsync();

        await _cache.SetAsync(
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

        var id = await _repository.CreateAsync(model);

        await _cache.RemoveAsync(CacheKeys.Categories);

        _logger.LogInformation(
            "REDIS INVALIDATED - Categories");

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


        await _cache.RemoveAsync(CacheKeys.Categories);

        _logger.LogInformation(
            "REDIS INVALIDATED - Categories");

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


        await _cache.RemoveAsync(CacheKeys.Categories);

        _logger.LogInformation(
            "REDIS INVALIDATED - Categories");

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