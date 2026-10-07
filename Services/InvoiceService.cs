using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Invoices;
using CostQualityControl.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Services;

public class InvoiceService(AppDbContext db, IIngredientService ingredientService) : IInvoiceService
{
    public async Task<List<InvoiceDto>> GetAllAsync()
    {
        var invoices = await db.Invoices
            .Include(i => i.Items).ThenInclude(it => it.Ingredient)
            .Include(i => i.SupplierEntity)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.Date)
            .ToListAsync();
        return invoices.Select(MapToDto).ToList();
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id)
    {
        var inv = await db.Invoices
            .Include(i => i.Items).ThenInclude(it => it.Ingredient)
            .Include(i => i.SupplierEntity)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);
        return inv is null ? null : MapToDto(inv);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest req, string createdBy)
    {
        var invoice = new Invoice { InvoiceNumber = req.InvoiceNumber, SupplierId = req.SupplierId, Date = req.Date, CreatedBy = createdBy };
        foreach (var item in req.Items)
        {
            var ing = await db.Ingredients.FindAsync(item.IngredientId);
            if (ing is null) continue;
            invoice.Items.Add(new InvoiceItem { IngredientId = item.IngredientId, Weight = item.Weight, UnitPrice = item.UnitPrice, TotalPrice = item.Weight * item.UnitPrice });
        }
        invoice.TotalAmount = invoice.Items.Sum(i => i.TotalPrice);
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        foreach (var item in req.Items)
        {
            var ing = await db.Ingredients.FindAsync(item.IngredientId);
            if (ing is not null)
                await ingredientService.UpdateAsync(item.IngredientId, new DTOs.Ingredients.UpdateIngredientRequest(
                    ing.Name, ing.Unit, item.UnitPrice, ing.SupplierId));
        }
        await db.Entry(invoice).Collection(i => i.Items).Query().Include(it => it.Ingredient).LoadAsync();
        return MapToDto(invoice);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var inv = await db.Invoices.FindAsync(id);
        if (inv is null) return false;
        db.Invoices.Remove(inv);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<InvoiceDto?> MarkPaidAsync(int id, bool isPaid)
    {
        var inv = await db.Invoices
            .Include(i => i.Items).ThenInclude(item => item.Ingredient)
            .Include(i => i.SupplierEntity)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inv is null) return null;
        inv.IsPaid = isPaid;
        await db.SaveChangesAsync();
        return MapToDto(inv);
    }

    public async Task<InvoiceDto?> AddPaymentAsync(int id, AddPaymentRequest req, string createdBy)
    {
        var inv = await db.Invoices
            .Include(i => i.Items).ThenInclude(it => it.Ingredient)
            .Include(i => i.SupplierEntity)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inv is null) return null;

        var payment = new InvoicePayment
        {
            InvoiceId = id,
            Amount = req.Amount,
            Date = req.Date,
            Notes = req.Notes,
            CreatedBy = createdBy
        };
        inv.Payments.Add(payment);

        var totalPaid = inv.Payments.Sum(p => p.Amount);
        inv.IsPaid = totalPaid >= inv.TotalAmount;

        await db.SaveChangesAsync();
        return MapToDto(inv);
    }

    public async Task<InvoiceDto?> DeletePaymentAsync(int invoiceId, int paymentId)
    {
        var inv = await db.Invoices
            .Include(i => i.Items).ThenInclude(it => it.Ingredient)
            .Include(i => i.SupplierEntity)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
        if (inv is null) return null;

        var payment = inv.Payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment is null) return null;

        inv.Payments.Remove(payment);
        var totalPaid = inv.Payments.Sum(p => p.Amount);
        inv.IsPaid = totalPaid >= inv.TotalAmount;

        await db.SaveChangesAsync();
        return MapToDto(inv);
    }

    private static InvoiceDto MapToDto(Invoice inv)
    {
        var totalPaid = inv.Payments.Sum(p => p.Amount);
        var totalDue = inv.TotalAmount - totalPaid;
        return new InvoiceDto(
            inv.Id, inv.InvoiceNumber, inv.SupplierId,
            inv.SupplierEntity?.Name ?? "", inv.Date,
            inv.Items.Select(i => new InvoiceItemDto(i.Id, i.IngredientId, i.Ingredient?.Name ?? "", i.Weight, i.UnitPrice, i.TotalPrice)).ToList(),
            inv.TotalAmount, totalPaid, totalDue < 0 ? 0 : totalDue,
            inv.CreatedAt, inv.IsPaid,
            inv.Payments.OrderByDescending(p => p.Date).Select(p => new InvoicePaymentDto(p.Id, p.Amount, p.Date, p.Notes, p.CreatedBy, p.CreatedAt)).ToList()
        );
    }
}
