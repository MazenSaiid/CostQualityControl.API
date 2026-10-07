using CostQualityControl.API.DTOs.Invoices;
namespace CostQualityControl.API.Services;
public interface IInvoiceService
{
    Task<List<InvoiceDto>> GetAllAsync();
    Task<InvoiceDto?> GetByIdAsync(int id);
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, string createdBy);
    Task<bool> DeleteAsync(int id);
    Task<InvoiceDto?> MarkPaidAsync(int id, bool isPaid);
    Task<InvoiceDto?> AddPaymentAsync(int id, AddPaymentRequest req, string createdBy);
    Task<InvoiceDto?> DeletePaymentAsync(int invoiceId, int paymentId);
}
