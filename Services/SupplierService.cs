using CostQualityControl.API.Data;
using CostQualityControl.API.DTOs.Suppliers;
using CostQualityControl.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Services;

public class SupplierService(AppDbContext db) : ISupplierService
{
    public async Task<List<SupplierDto>> GetAllAsync()
        => await db.Suppliers.OrderBy(s => s.Name).Select(s => MapToDto(s)).ToListAsync();

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var s = await db.Suppliers.FindAsync(id);
        return s is null ? null : MapToDto(s);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest req)
    {
        var s = new Supplier { Name = req.Name, ContactPerson = req.ContactPerson, Phone = req.Phone, Email = req.Email, Notes = req.Notes };
        db.Suppliers.Add(s);
        await db.SaveChangesAsync();
        return MapToDto(s);
    }

    public async Task<SupplierDto?> UpdateAsync(int id, CreateSupplierRequest req)
    {
        var s = await db.Suppliers.FindAsync(id);
        if (s is null) return null;
        s.Name = req.Name; s.ContactPerson = req.ContactPerson; s.Phone = req.Phone; s.Email = req.Email; s.Notes = req.Notes;
        await db.SaveChangesAsync();
        return MapToDto(s);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var s = await db.Suppliers.FindAsync(id);
        if (s is null) return false;
        db.Suppliers.Remove(s);
        await db.SaveChangesAsync();
        return true;
    }

    private static SupplierDto MapToDto(Supplier s)
        => new(s.Id, s.Name, s.ContactPerson, s.Phone, s.Email, s.Notes, s.CreatedAt);
}
