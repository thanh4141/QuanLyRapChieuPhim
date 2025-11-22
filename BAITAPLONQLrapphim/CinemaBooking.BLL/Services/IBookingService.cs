using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;

namespace CinemaBooking.BLL.Services;

public interface IBookingService
{
    Task<BookingPreviewResponse?> GetBookingPreviewAsync(BookingPreviewRequest request);
    Task<BookingDto?> CreateBookingAsync(CreateBookingRequest request, int userId);
    Task<BookingDto?> CreateDirectBookingAsync(CreateDirectBookingRequest request, int staffUserId);
    Task<PagedResult<BookingDto>> GetUserBookingsAsync(int userId, PagedRequest request);
    Task<bool> CancelBookingAsync(int reservationId, int userId, string? reason = null);
}

