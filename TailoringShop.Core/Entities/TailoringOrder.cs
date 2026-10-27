using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TailoringShop.Core.Enums;

namespace TailoringShop.Core.Entities;

public class TailoringOrder : BaseEntityWithUser
{
    public Guid UUID { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public long CustomerId { get; set; }
    
    public long CashboxId { get; set; }
    
    public long CurrencyId { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.New;
    
    public decimal TotalAmount { get; set; }
    
    public decimal DepositAmount { get; set; } = 0;
    
    public decimal RemainingAmount { get; set; } = 0;
    
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    
    public string? Description { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime DeliveryDate { get; set; } = DateTime.Today.AddDays(7);
    
    /// <summary>
    /// Alias لتاريخ التسليم المتوقع (للطباعة) - لا يُحفظ في قاعدة البيانات
    /// </summary>
    [NotMapped]
    public DateTime ExpectedDeliveryDate 
    { 
        get => DeliveryDate; 
        set => DeliveryDate = value; 
    }
    
    public DateTime? ActualDeliveryDate { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.Today;
    
    // Navigation Properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Cashbox Cashbox { get; set; } = null!;
    public virtual Currency Currency { get; set; } = null!;
    public virtual ICollection<TailoringOrderItem> Items { get; set; } = new List<TailoringOrderItem>();
    public virtual ICollection<TailoringOrderStatusHistory> StatusHistory { get; set; } = new List<TailoringOrderStatusHistory>();
    public virtual ICollection<TailoringOrderPayment> Payments { get; set; } = new List<TailoringOrderPayment>();
}

public class TailoringOrderItem : BaseEntity
{
    public long OrderId { get; set; }
    
    [Required, MaxLength(150)]
    public string ItemName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int Quantity { get; set; } = 1;
    
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Alias للسعر (للطباعة) - لا يُحفظ في قاعدة البيانات
    /// </summary>
    [NotMapped]
    public decimal Price 
    { 
        get => UnitPrice; 
        set => UnitPrice = value; 
    }
    
    public decimal Total { get; set; }
    
    // Navigation Properties
    public virtual TailoringOrder Order { get; set; } = null!;
}

public class TailoringOrderStatusHistory : BaseEntity
{
    public long OrderId { get; set; }
    
    public OrderStatus? FromStatus { get; set; }
    
    public OrderStatus ToStatus { get; set; }
    
    public string? Notes { get; set; }
    
    public long? CreatedBy { get; set; }
    
    public long? ChangedBy { get; set; }
    
    // Navigation Properties
    public virtual TailoringOrder Order { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
    public virtual User? ChangedByUser { get; set; }
}

public class TailoringOrderPayment : BaseEntity
{
    public long OrderId { get; set; }
    
    public long CashboxId { get; set; }
    
    public decimal Amount { get; set; }
    
    public PaymentType PaymentType { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    
    public long? CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual TailoringOrder Order { get; set; } = null!;
    public virtual Cashbox Cashbox { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}
