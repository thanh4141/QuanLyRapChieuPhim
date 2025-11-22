using CinemaBooking.Common.DTOs;

namespace CinemaBooking.BLL.Services;

public interface IPaymentService
{
    Task<PaymentDto?> CreatePaymentAsync(CreatePaymentRequest request, int userId);
    Task<PaymentDto?> CreatePaymentForCustomerAsync(CreatePaymentRequest request, int staffUserId, int customerUserId);
    Task<List<PaymentDto>> GetPaymentsByInvoiceAsync(int invoiceId);
}

