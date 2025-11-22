using AutoMapper;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Entities;
using CinemaBooking.DAL.Repositories;

namespace CinemaBooking.BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaymentDto?> CreatePaymentAsync(CreatePaymentRequest request, int userId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice == null || invoice.IsDeleted || invoice.UserId != userId)
        {
            return null;
        }

        // Check reservation status and expiration
        var reservation = await _unitOfWork.Bookings.GetByIdAsync(invoice.ReservationId);
        if (reservation == null || reservation.IsDeleted)
        {
            return null;
        }

        if (reservation.Status != "Pending")
        {
            throw new Exception(reservation.Status == "Confirmed" 
                ? "Vé đã được thanh toán rồi" 
                : "Không thể thanh toán cho đặt vé này");
        }

        if (reservation.ExpiresAt.HasValue && reservation.ExpiresAt.Value <= DateTime.UtcNow)
        {
            // Đánh dấu đặt chỗ đã hết hạn
            reservation.Status = "Expired";
            reservation.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(reservation);
            await _unitOfWork.SaveChangesAsync();
            throw new Exception("Đặt vé đã hết hạn. Vui lòng đặt lại.");
        }

        // Để demo, mô phỏng thanh toán thành công ngay lập tức
        var payment = new Payment
        {
            InvoiceId = request.InvoiceId,
            UserId = userId,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "Success", // Mô phỏng
            Amount = invoice.TotalAmount,
            PaidAt = DateTime.UtcNow,
            TransactionCode = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(10000, 99999)}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Payments.AddAsync(payment);

        // Cập nhật trạng thái đặt chỗ (đã tải ở trên)
        reservation.Status = "Confirmed";
        reservation.ConfirmedAt = DateTime.UtcNow;
        reservation.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Bookings.Update(reservation);

        // Update tickets status
        var tickets = await _unitOfWork.Tickets.GetTicketsByReservationAsync(invoice.ReservationId);
        foreach (var ticket in tickets)
        {
            ticket.Status = "Paid";
            ticket.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Tickets.Update(ticket);
        }

        await _unitOfWork.SaveChangesAsync();

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "CreatePayment",
            EntityName = "Payment",
            EntityId = payment.PaymentId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto?> CreatePaymentForCustomerAsync(CreatePaymentRequest request, int staffUserId, int customerUserId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice == null || invoice.IsDeleted || invoice.UserId != customerUserId)
        {
            return null;
        }

        // Check reservation status and expiration
        var reservation = await _unitOfWork.Bookings.GetByIdAsync(invoice.ReservationId);
        if (reservation == null || reservation.IsDeleted)
        {
            return null;
        }

        // Nhân viên có thể thanh toán cho đặt chỗ hết hạn (trong thời gian hợp lý - 1 giờ sau khi hết hạn)
        bool isExpired = reservation.ExpiresAt.HasValue && reservation.ExpiresAt.Value <= DateTime.UtcNow;
        bool canPayExpired = isExpired && 
                            reservation.ExpiresAt.HasValue && 
                            (DateTime.UtcNow - reservation.ExpiresAt.Value).TotalHours <= 1;

        if (reservation.Status != "Pending" && reservation.Status != "Expired")
        {
            throw new Exception(reservation.Status == "Confirmed" 
                ? "Vé đã được thanh toán rồi" 
                : "Không thể thanh toán cho đặt vé này");
        }

        // Cho phép thanh toán cho đặt chỗ hết hạn nếu trong vòng 1 giờ (nhân viên có thể ghi đè)
        if (isExpired && !canPayExpired)
        {
            throw new Exception("Đặt vé đã hết hạn quá lâu. Vui lòng đặt lại.");
        }

        // Nếu hết hạn nhưng trong thời gian gia hạn, cho phép thanh toán
        if (isExpired && canPayExpired)
        {
            // Đặt lại trạng thái hết hạn
            reservation.Status = "Pending";
            reservation.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(reservation);
            await _unitOfWork.SaveChangesAsync();
        }

        // Tạo thanh toán
        var payment = new Payment
        {
            InvoiceId = request.InvoiceId,
            UserId = customerUserId, // Khách hàng là người thanh toán
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "Success",
            Amount = invoice.TotalAmount,
            PaidAt = DateTime.UtcNow,
            TransactionCode = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(10000, 99999)}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Payments.AddAsync(payment);

        // Cập nhật trạng thái đặt chỗ
        reservation.Status = "Confirmed";
        reservation.ConfirmedAt = DateTime.UtcNow;
        reservation.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Bookings.Update(reservation);

        // Update tickets status
        var tickets = await _unitOfWork.Tickets.GetTicketsByReservationAsync(invoice.ReservationId);
        foreach (var ticket in tickets)
        {
            ticket.Status = "Paid";
            ticket.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Tickets.Update(ticket);
        }

        await _unitOfWork.SaveChangesAsync();

        // Ghi nhật ký kiểm toán - lưu ý nhân viên đã xử lý thanh toán
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = staffUserId,
            ActionName = "CreatePaymentForCustomer",
            EntityName = "Payment",
            EntityId = payment.PaymentId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<List<PaymentDto>> GetPaymentsByInvoiceAsync(int invoiceId)
    {
        var payments = await _unitOfWork.Payments.FindAsync(p => p.InvoiceId == invoiceId && !p.IsDeleted);
        return _mapper.Map<List<PaymentDto>>(payments);
    }
}

