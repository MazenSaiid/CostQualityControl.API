using CostQualityControl.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CostQualityControl.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductIngredient> ProductIngredients => Set<ProductIngredient>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<ProductionBatch> ProductionBatches => Set<ProductionBatch>();
    public DbSet<BatchIngredient> BatchIngredients => Set<BatchIngredient>();
public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RolePermission>(e =>
        {
            e.HasIndex(r => new { r.RoleName, r.Resource }).IsUnique();
        });

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Code).IsUnique();
            e.Property(p => p.SellingPrice).HasPrecision(18, 4);
            e.Property(p => p.TotalCost).HasPrecision(18, 4);
            e.Property(p => p.TotalWeight).HasPrecision(18, 4);
            e.Property(p => p.CostPerKg).HasPrecision(18, 4);
            e.Property(p => p.ProfitAmount).HasPrecision(18, 4);
            e.Property(p => p.ProfitPercentage).HasPrecision(18, 4);
        });

        builder.Entity<Ingredient>(e =>
        {
            e.Property(i => i.CurrentCost).HasPrecision(18, 4);
            e.HasOne(i => i.SupplierEntity).WithMany().HasForeignKey(i => i.SupplierId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ProductIngredient>(e =>
        {
            e.HasIndex(pi => new { pi.ProductId, pi.IngredientId }).IsUnique();
            e.Property(pi => pi.Weight).HasPrecision(18, 4);
            e.Property(pi => pi.Cost).HasPrecision(18, 4);
            e.HasOne(pi => pi.Product).WithMany(p => p.ProductIngredients).HasForeignKey(pi => pi.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pi => pi.Ingredient).WithMany(i => i.ProductIngredients).HasForeignKey(pi => pi.IngredientId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Supplier>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.ContactPerson).HasMaxLength(200);
            e.Property(s => s.Phone).HasMaxLength(50);
            e.Property(s => s.Email).HasMaxLength(200);
            e.HasMany(s => s.Invoices).WithOne(i => i.SupplierEntity).HasForeignKey(i => i.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Invoice>(e =>
        {
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
            e.Property(i => i.TotalAmount).HasPrecision(18, 4);
        });

        builder.Entity<InvoiceItem>(e =>
        {
            e.Property(i => i.Weight).HasPrecision(18, 4);
            e.Property(i => i.UnitPrice).HasPrecision(18, 4);
            e.Property(i => i.TotalPrice).HasPrecision(18, 4);
            e.HasOne(i => i.Invoice).WithMany(inv => inv.Items).HasForeignKey(i => i.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Ingredient).WithMany(ing => ing.InvoiceItems).HasForeignKey(i => i.IngredientId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InvoicePayment>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 4);
            e.HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductionBatch>(e =>
        {
            e.HasIndex(b => b.BatchNumber).IsUnique();
            e.Property(b => b.ExpectedWeight).HasPrecision(18, 4);
            e.Property(b => b.ActualWeight).HasPrecision(18, 4);
            e.Property(b => b.YieldPercentage).HasPrecision(18, 4);
            e.Property(b => b.WastePercentage).HasPrecision(18, 4);
            e.HasOne(b => b.Product).WithMany(p => p.ProductionBatches).HasForeignKey(b => b.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BatchIngredient>(e =>
        {
            e.Property(b => b.ExpectedWeight).HasPrecision(18, 4);
            e.Property(b => b.ActualWeight).HasPrecision(18, 4);
            e.HasOne(b => b.Batch).WithMany(pb => pb.BatchIngredients).HasForeignKey(b => b.BatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.Ingredient).WithMany(i => i.BatchIngredients).HasForeignKey(b => b.IngredientId).OnDelete(DeleteBehavior.Restrict);
        });

    }
}
