using System.ComponentModel.DataAnnotations;
using TailoringShop.Core.Enums;

namespace TailoringShop.Core.Entities;

public class Category : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product : BaseEntityWithUser
{
    public Guid UUID { get; set; } = Guid.NewGuid();
    
    [MaxLength(50)]
    public string? SKU { get; set; }
    
    [MaxLength(50)]
    public string? Barcode { get; set; }
    
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    
    public long? CategoryId { get; set; }
    
    /// <summary>
    /// الصندوق الذي ينتمي إليه المنتج (من فاتورة الشراء)
    /// </summary>
    public long? CashboxId { get; set; }
    
    public decimal CostPrice { get; set; } = 0;
    
    public decimal SellingPrice { get; set; }
    
    public long CurrencyId { get; set; }
    
    [MaxLength(20)]
    public string Unit { get; set; } = "piece";
    
    public int MinStockLevel { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Category? Category { get; set; }
    public virtual Cashbox? Cashbox { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}

public class Store : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Location { get; set; }
    
    public bool IsDefault { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}

public class Inventory : BaseEntity
{
    public long ProductId { get; set; }
    
    public long StoreId { get; set; }
    
    public decimal Quantity { get; set; } = 0;
    
    public decimal ReservedQuantity { get; set; } = 0;
    
    // Computed Property
    public decimal AvailableQuantity => Quantity - ReservedQuantity;
    
    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Store Store { get; set; } = null!;
}

public class InventoryMovement : BaseEntity
{
    public long ProductId { get; set; }
    
    public long StoreId { get; set; }
    
    public MovementType Type { get; set; }
    
    public decimal Quantity { get; set; }
    
    public decimal QuantityBefore { get; set; }
    
    public decimal QuantityAfter { get; set; }
    
    public decimal? UnitCost { get; set; }
    
    [MaxLength(50)]
    public string? ReferenceType { get; set; }
    
    public long? ReferenceId { get; set; }
    
    public string? Notes { get; set; }
    
    public long? CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Store Store { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}
