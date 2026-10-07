using CostQualityControl.API.DTOs.Dashboard;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService svc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardStatsDto>> GetStats() => Ok(await svc.GetStatsAsync());
}
