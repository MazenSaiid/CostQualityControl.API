namespace CostQualityControl.API.Models;

public class RolePermission
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g. "Products", "Invoices"
    public bool CanView { get; set; }
    public bool CanWrite { get; set; }
    public bool CanDelete { get; set; }
}
