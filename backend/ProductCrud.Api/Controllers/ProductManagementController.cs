using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCrud.Api.Models;
using ProductCrud.Api.Services;

namespace ProductCrud.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductManagementController : ControllerBase
{
    private readonly IProductManagementService _service;

    public ProductManagementController(IProductManagementService service)
    {
        _service = service;
    }

    [HttpGet("GetAll")]
    [Authorize(Roles = "Admin,Editor,Viewer")]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilterDTO filter)
    {
        return Ok(await _service.GetAllAsync(filter));
    }

    [HttpGet("GetById/{id:int}")]
    [Authorize(Roles = "Admin,Editor,Viewer")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost("Create")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Create([FromBody] ProductModel model)
    {
        return Ok(await _service.CreateAsync(model));
    }

    [HttpPut("Update")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Update([FromBody] ProductModel model)
    {
        return Ok(await _service.UpdateAsync(model));
    }

    [HttpDelete("Delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await _service.DeleteAsync(id));
    }
}
