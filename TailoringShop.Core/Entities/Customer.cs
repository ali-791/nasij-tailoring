using System.ComponentModel.DataAnnotations;

namespace TailoringShop.Core.Entities;

public class Customer : BaseEntityWithUser
{
    public Guid UUID { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    public decimal TotalPurchases { get; set; } = 0;
    
    public int TotalOrders { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public virtual ICollection<TailoringOrder> TailoringOrders { get; set; } = new List<TailoringOrder>();
}
