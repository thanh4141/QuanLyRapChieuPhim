using CinemaBooking.Common.DTOs;

namespace CinemaBooking.BLL.Services;

public interface ITicketService
{
    Task<List<TicketDto>> GetUserTicketsAsync(int userId);
    Task<TicketDto?> GetTicketByQrCodeAsync(string qrCodeData);
    Task<bool> CheckInTicketAsync(string qrCodeData, int staffUserId);
    Task<List<TicketDto>> GetTodayTicketsAsync();
    Task<List<TicketDto>> GetTicketsByReservationAsync(int reservationId);
}

