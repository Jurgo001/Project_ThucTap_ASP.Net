using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Services;

namespace ProductCrud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _categoryService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await _categoryService.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Create(CategoryModel model)
    {
        return Ok(await _categoryService.CreateAsync(model));
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Update(CategoryModel model)
    {
        return Ok(await _categoryService.UpdateAsync(model));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _categoryService.DeleteAsync(id));
    }
}