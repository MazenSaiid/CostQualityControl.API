using CostQualityControl.API.DTOs.Products;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IProductService svc) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Products:view")]
    public async Task<ActionResult<List<ProductDto>>> GetAll() => Ok(await svc.GetAllAsync());

    [HttpGet("{id}")]
    [Authorize(Policy = "Products:view")]
    public async Task<ActionResult<ProductDto>> GetById(int id) { var p = await svc.GetByIdAsync(id); return p is null ? NotFound() : Ok(p); }

    [HttpPost]
    [Authorize(Policy = "Products:write")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest req) { var p = await svc.CreateAsync(req); return CreatedAtAction(nameof(GetById), new { id = p.Id }, p); }

    [HttpPut("{id}")]
    [Authorize(Policy = "Products:write")]
    public async Task<ActionResult<ProductDto>> Update(int id, UpdateProductRequest req) { var p = await svc.UpdateAsync(id, req); return p is null ? NotFound() : Ok(p); }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Products:delete")]
    public async Task<IActionResult> Delete(int id) => await svc.DeleteAsync(id) ? NoContent() : NotFound();
}
