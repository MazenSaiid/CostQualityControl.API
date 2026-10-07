using CostQualityControl.API.DTOs.Suppliers;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController(ISupplierService svc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetAll() => Ok(await svc.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        var s = await svc.GetByIdAsync(id);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(CreateSupplierRequest req)
    {
        var s = await svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = s.Id }, s);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierDto>> Update(int id, CreateSupplierRequest req)
    {
        var s = await svc.UpdateAsync(id, req);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await svc.DeleteAsync(id) ? NoContent() : NotFound();
}
