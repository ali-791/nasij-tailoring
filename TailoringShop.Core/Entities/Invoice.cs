using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TailoringShop.Core.Enums;

namespace TailoringShop.Core.Entities;

public class Invoice : BaseEntityWithUser
{
    public Guid UUID { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public InvoiceType Type { get; set; }
    
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    public long? CustomerId { get; set; }
    
    [MaxLength(100)]
    public string? SupplierName { get; set; }
    
    public long CashboxId { get; set; }
    
    public long StoreId { get; set; }
    
    public long CurrencyId { get; set; }
    
    public decimal Subtotal { get; set; } = 0;
    
    /// <summary>
    /// Alias للتوافق مع الكود القديم - لا يُحفظ في قاعدة البيانات
    /// </summary>
    [NotMapped]
    public decimal SubTotal 
    { 
        get => Subtotal; 
        set => Subtotal = value; 
    }
    
    public DiscountType? DiscountType { get; set; }
    
    public decimal DiscountValue { get; set; } = 0;
    
    public decimal DiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Alias للخصم (للطباعة) - لا يُحفظ في قاعدة البيانات
    /// </summary>
    [NotMapped]
    public decimal Discount 
    { 
        get => DiscountAmount; 
        set => DiscountAmount = value; 
    }
    
    public decimal Total { get; set; } = 0;
    
    public decimal PaidAmount { get; set; } = 0;
    
    public decimal RemainingAmount { get; set; } = 0;
    
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    
    public string? Notes { get; set; }
    
    public DateTime InvoiceDate { get; set; } = DateTime.Today;
    
    // Navigation Properties
    public virtual Customer? Customer { get; set; }
    public virtual Cashbox Cashbox { get; set; } = null!;
    public virtual Store Store { get; set; } = null!;
    public virtual Currency Currency { get; set; } = null!;
    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public virtual ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
}

public class InvoiceItem : BaseEntity
{
    public long InvoiceId { get; set; }
    
    public long ProductId { get; set; }
    
    public string? Description { get; set; }
    
    public decimal Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal CostPrice { get; set; } = 0;
    
    public DiscountType? DiscountType { get; set; }
    
    public decimal DiscountValue { get; set; } = 0;
    
    public decimal DiscountAmount { get; set; } = 0;
    
    public decimal Subtotal { get; set; }
    
    public decimal Total { get; set; }
    
    /// <summary>
    /// سعر الإجمالي للعنصر (للطباعة) - لا يُحفظ في قاعدة البيانات
    /// </summary>
    [NotMapped]
    public decimal TotalPrice 
    { 
        get => Total; 
        set => Total = value; 
    }
    
    public decimal Profit { get; set; } = 0;
    
    // Navigation Properties
    public virtual Invoice Invoice { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

public class InvoicePayment : BaseEntity
{
    public long InvoiceId { get; set; }
    
    public long CashboxId { get; set; }
    
    public decimal Amount { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    
    public long? CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual Invoice Invoice { get; set; } = null!;
    public virtual Cashbox Cashbox { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}
