using System.Security.Claims;
using CostQualityControl.API.DTOs.Invoices;
using CostQualityControl.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CostQualityControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController(IInvoiceService svc) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Invoices:view")]
    public async Task<ActionResult<List<InvoiceDto>>> GetAll() => Ok(await svc.GetAllAsync());

    [HttpGet("{id}")]
    [Authorize(Policy = "Invoices:view")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id) { var i = await svc.GetByIdAsync(id); return i is null ? NotFound() : Ok(i); }

    [HttpPost]
    [Authorize(Policy = "Invoices:write")]
    public async Task<ActionResult<InvoiceDto>> Create(CreateInvoiceRequest req)
    {
        var user = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var inv = await svc.CreateAsync(req, user);
        return CreatedAtAction(nameof(GetById), new { id = inv.Id }, inv);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Invoices:delete")]
    public async Task<IActionResult> Delete(int id) => await svc.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpPatch("{id}/paid")]
    [Authorize(Policy = "Invoices:write")]
    public async Task<ActionResult<InvoiceDto>> MarkPaid(int id, [FromBody] MarkPaidRequest req)
    {
        var inv = await svc.MarkPaidAsync(id, req.IsPaid);
        return inv is null ? NotFound() : Ok(inv);
    }

    [HttpPost("{id}/payments")]
    [Authorize(Policy = "Invoices:write")]
    public async Task<ActionResult<InvoiceDto>> AddPayment(int id, [FromBody] AddPaymentRequest req)
    {
        var user = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var inv = await svc.AddPaymentAsync(id, req, user);
        return inv is null ? NotFound() : Ok(inv);
    }

    [HttpDelete("{id}/payments/{paymentId}")]
    [Authorize(Policy = "Invoices:write")]
    public async Task<ActionResult<InvoiceDto>> DeletePayment(int id, int paymentId)
    {
        var inv = await svc.DeletePaymentAsync(id, paymentId);
        return inv is null ? NotFound() : Ok(inv);
    }
}
