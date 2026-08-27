using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductCrud.DataServices.Models;

namespace ProductCrud.DataServices.Services;

public interface ICategoryService
{
    Task<ResultModel<List<CategoryDTO>>> GetAllAsync();

    Task<ResultModel<CategoryDTO>> GetByIdAsync(int id);

    Task<ResultModel<int>> CreateAsync(CategoryModel model);

    Task<ResultModel<bool>> UpdateAsync(CategoryModel model);

    Task<ResultModel<bool>> DeleteAsync(int id);
}