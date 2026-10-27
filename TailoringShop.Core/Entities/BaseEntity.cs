namespace TailoringShop.Core.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public abstract class BaseEntityWithUser : BaseEntity
{
    public long? CreatedBy { get; set; }
    public virtual User? CreatedByUser { get; set; }
}
