using System.ComponentModel.DataAnnotations;
using TailoringShop.Core.Enums;

namespace TailoringShop.Core.Entities;

public class Currency : BaseEntity
{
    [Required, MaxLength(3)]
    public string Code { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required, MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;
    
    public decimal ExchangeRate { get; set; } = 1;
    
    public bool IsDefault { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public byte DecimalPlaces { get; set; } = 2;
    
    // Navigation Properties
    public virtual ICollection<Cashbox> Cashboxes { get; set; } = new List<Cashbox>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Cashbox : BaseEntityWithUser
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public CashboxType Type { get; set; }
    
    public long CurrencyId { get; set; }
    
    public decimal CurrentBalance { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Currency Currency { get; set; } = null!;
    public virtual ICollection<CashboxTransaction> Transactions { get; set; } = new List<CashboxTransaction>();
}

public class CashboxTransaction : BaseEntity
{
    public long CashboxId { get; set; }
    
    public TransactionType Type { get; set; }
    
    public decimal Amount { get; set; }
    
    public decimal BalanceBefore { get; set; }
    
    public decimal BalanceAfter { get; set; }
    
    [MaxLength(50)]
    public string? ReferenceType { get; set; }
    
    public long? ReferenceId { get; set; }
    
    public long? CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual Cashbox Cashbox { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}
