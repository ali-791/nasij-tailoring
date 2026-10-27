using System.ComponentModel.DataAnnotations;

namespace TailoringShop.Core.Entities;

public class User : BaseEntity
{
    public Guid UUID { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastLoginAt { get; set; }
    
    public long? CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
