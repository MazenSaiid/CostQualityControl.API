using CostQualityControl.API.DTOs.Ingredients;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientsController(IIngredientService svc) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Ingredients:view")]
    public async Task<ActionResult<List<IngredientDto>>> GetAll() => Ok(await svc.GetAllAsync());

    [HttpGet("{id}")]
    [Authorize(Policy = "Ingredients:view")]
    public async Task<ActionResult<IngredientDto>> GetById(int id)
    {
        var item = await svc.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = "Ingredients:write")]
    public async Task<ActionResult<IngredientDto>> Create(CreateIngredientRequest req)
    {
        var item = await svc.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Ingredients:write")]
    public async Task<ActionResult<IngredientDto>> Update(int id, UpdateIngredientRequest req)
    {
        var item = await svc.UpdateAsync(id, req);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Ingredients:delete")]
    public async Task<IActionResult> Delete(int id) => await svc.DeleteAsync(id) ? NoContent() : NotFound();
}
