using System.Security.Claims;
using CostQualityControl.API.DTOs.Production;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductionController(IProductionService svc) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Production:view")]
    public async Task<ActionResult<List<BatchDto>>> GetAll() => Ok(await svc.GetAllAsync());

    [HttpGet("{id}")]
    [Authorize(Policy = "Production:view")]
    public async Task<ActionResult<BatchDto>> GetById(int id) { var b = await svc.GetByIdAsync(id); return b is null ? NotFound() : Ok(b); }

    [HttpPost]
    [Authorize(Policy = "Production:write")]
    public async Task<ActionResult<BatchDto>> Create(CreateBatchRequest req)
    {
        var user = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var b = await svc.CreateAsync(req, user);
        return CreatedAtAction(nameof(GetById), new { id = b.Id }, b);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Production:write")]
    public async Task<ActionResult<BatchDto>> Update(int id, UpdateBatchRequest req) { var b = await svc.UpdateAsync(id, req); return b is null ? NotFound() : Ok(b); }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Production:delete")]
    public async Task<IActionResult> Delete(int id) => await svc.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpGet("supplier-consumption")]
    [Authorize(Policy = "Production:view")]
    public async Task<ActionResult<List<SupplierConsumptionDto>>> GetSupplierConsumption()
        => Ok(await svc.GetSupplierConsumptionAsync());
}
