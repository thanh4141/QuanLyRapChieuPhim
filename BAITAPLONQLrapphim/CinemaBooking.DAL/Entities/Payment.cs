using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Payments")]
public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Success, Failed, Refunded

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    [MaxLength(100)]
    public string? TransactionCode { get; set; }

    public string? GatewayResponse { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("InvoiceId")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

