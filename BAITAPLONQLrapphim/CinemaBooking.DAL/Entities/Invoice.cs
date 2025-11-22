using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Invoices")]
public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }

    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public int ReservationId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal SubTotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal TaxAmount { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("ReservationId")]
    public virtual Reservation Reservation { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

