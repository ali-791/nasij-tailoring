using System.ComponentModel.DataAnnotations;
using TailoringShop.Core.Enums;

namespace TailoringShop.Core.Entities;

public class SystemSettings : BaseEntity
{
    [Required, MaxLength(200)]
    public string ShopName { get; set; } = "محل الخياطة";
    
    [MaxLength(20)]
    public string? ShopPhone { get; set; }
    
    [MaxLength(500)]
    public string? ShopAddress { get; set; }
    
    [MaxLength(500)]
    public string? LogoPath { get; set; }
    
    public bool EnableNotifications { get; set; } = true;
    
    public bool EnableAutoBackup { get; set; } = false;
}

public class AuditLog : BaseEntity
{
    public long? UserId { get; set; }
    
    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    public long? EntityId { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    // Navigation Properties
    public virtual User? User { get; set; }
}

public class Backup : BaseEntity
{
    [Required, MaxLength(255)]
    public string Filename { get; set; } = string.Empty;
    
    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public BackupType Type { get; set; }
    
    public BackupStatus Status { get; set; }
    
    public string? Notes { get; set; }
    
    public long? CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual User? CreatedByUser { get; set; }
}

public class Setting : BaseEntity
{
    [Required, MaxLength(50)]
    public string Group { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    public string? Value { get; set; }
    
    [MaxLength(20)]
    public string Type { get; set; } = "string";
    
    public string? Description { get; set; }
    
    public long? UpdatedBy { get; set; }
    
    // Navigation Properties
    public virtual User? UpdatedByUser { get; set; }
}
