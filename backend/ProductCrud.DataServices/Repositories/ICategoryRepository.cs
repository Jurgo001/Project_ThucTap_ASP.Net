using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductCrud.DataServices.Models;

namespace ProductCrud.DataServices.Repositories;

public interface ICategoryRepository
{
    Task<List<CategoryDTO>> GetAllAsync();

    Task<CategoryDTO?> GetByIdAsync(int id);

    Task<int> CreateAsync(CategoryModel model);

    Task<bool> UpdateAsync(CategoryModel model);

    Task<bool> DeleteAsync(int id);
}
