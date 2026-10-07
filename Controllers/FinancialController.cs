using CostQualityControl.API.DTOs.Financials;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/financials")]
[Authorize]
public class FinancialController(IFinancialService svc) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<FinancialSummaryDto>> GetSummary()
        => Ok(await svc.GetSummaryAsync());
}
