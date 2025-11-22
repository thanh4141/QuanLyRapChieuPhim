using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("RolePermissions")]
public class RolePermission
{
    [Key]
    public int RolePermissionId { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required]
    public int PermissionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("PermissionId")]
    public virtual Permission Permission { get; set; } = null!;
}

