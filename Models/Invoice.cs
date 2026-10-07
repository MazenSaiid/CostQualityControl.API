namespace CostQualityControl.API.Models;
public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public Supplier? SupplierEntity { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public ICollection<InvoiceItem> Items { get; set; } = [];
    public bool IsPaid { get; set; } = false;
    public ICollection<InvoicePayment> Payments { get; set; } = [];
}
