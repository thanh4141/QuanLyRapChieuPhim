namespace CinemaBooking.Common.DTOs;

public class BookingPreviewRequest
{
    public int ShowtimeId { get; set; }
    public List<int> SeatIds { get; set; } = new();
}

public class BookingPreviewResponse
{
    public ShowtimeDto Showtime { get; set; } = null!;
    public List<SeatPriceDto> Seats { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class SeatPriceDto
{
    public int SeatId { get; set; }
    public string RowLabel { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatTypeName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CreateBookingRequest
{
    public int ShowtimeId { get; set; }
    public List<int> SeatIds { get; set; } = new();
    public string? PaymentMethod { get; set; }
}

public class BookingDto
{
    public int ReservationId { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ShowtimeId { get; set; }
    public ShowtimeDto Showtime { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<TicketDto> Tickets { get; set; } = new();
    public InvoiceDto? Invoice { get; set; }
    public int? InvoiceId { get; set; }
    // Additional info for display
    public string MovieTitle { get; set; } = string.Empty;
    public string AuditoriumName { get; set; } = string.Empty;
    public DateTime ShowtimeStartTime { get; set; }
}

public class TicketDto
{
    public int TicketId { get; set; }
    public int ReservationId { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public int ShowtimeId { get; set; }
    public int SeatId { get; set; }
    public string RowLabel { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatTypeName { get; set; } = string.Empty;
    public decimal SeatPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string QrCodeData { get; set; } = string.Empty;
    public string? QrCodeImageUrl { get; set; }
    public DateTime? CheckedInAt { get; set; }
    // Additional info for display
    public string MovieTitle { get; set; } = string.Empty;
    public string AuditoriumName { get; set; } = string.Empty;
    public DateTime ShowtimeStartTime { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CheckInRequest
{
    public string QrCodeData { get; set; } = string.Empty;
}

public class InvoiceDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ReservationId { get; set; }
    public int UserId { get; set; }
    public DateTime IssuedAt { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
}

public class PaymentDto
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionCode { get; set; }
}

public class CreatePaymentRequest
{
    public int InvoiceId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class CreateDirectBookingRequest
{
    public int ShowtimeId { get; set; }
    public List<int> SeatIds { get; set; } = new();
    public string PaymentMethod { get; set; } = "Cash"; // Default to Cash for direct booking
    public string? CustomerEmail { get; set; } // Optional: if provided, book for this customer, otherwise book for staff
}

public class CancelBookingRequest
{
    public string? Reason { get; set; }
}

