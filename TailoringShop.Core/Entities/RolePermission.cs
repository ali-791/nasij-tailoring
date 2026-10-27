using System.ComponentModel.DataAnnotations;

namespace TailoringShop.Core.Entities;

public class Role : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    public bool IsSystem { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Permission : BaseEntity
{
    [Required, MaxLength(50)]
    public string Module { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required, MaxLength(150)]
    public string DisplayName { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation Properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

public class UserRole
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.Now;
    public long? AssignedBy { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual User? AssignedByUser { get; set; }
}
