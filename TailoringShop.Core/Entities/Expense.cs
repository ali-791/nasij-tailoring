using System.ComponentModel.DataAnnotations;

namespace TailoringShop.Core.Entities;

public class ExpenseCategory : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class Expense : BaseEntityWithUser
{
    public Guid UUID { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(50)]
    public string ExpenseNumber { get; set; } = string.Empty;
    
    public long CategoryId { get; set; }
    
    public long CashboxId { get; set; }
    
    public long CurrencyId { get; set; }
    
    public decimal Amount { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    public DateTime ExpenseDate { get; set; } = DateTime.Today;
    
    // Navigation Properties
    public virtual ExpenseCategory Category { get; set; } = null!;
    public virtual Cashbox Cashbox { get; set; } = null!;
    public virtual Currency Currency { get; set; } = null!;
}
