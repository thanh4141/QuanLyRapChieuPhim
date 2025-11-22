using AutoMapper;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Entities;
using CinemaBooking.DAL.Repositories;

namespace CinemaBooking.BLL.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TicketService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<TicketDto>> GetUserTicketsAsync(int userId)
    {
        var tickets = await _unitOfWork.Tickets.GetUserTicketsAsync(userId);
        return _mapper.Map<List<TicketDto>>(tickets);
    }

    public async Task<TicketDto?> GetTicketByQrCodeAsync(string qrCodeData)
    {
        var ticket = await _unitOfWork.Tickets.GetTicketByQrCodeAsync(qrCodeData);
        if (ticket == null) return null;
        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<bool> CheckInTicketAsync(string qrCodeData, int staffUserId)
    {
        var ticket = await _unitOfWork.Tickets.GetTicketByQrCodeAsync(qrCodeData);
        if (ticket == null || ticket.IsDeleted)
        {
            return false;
        }

        if (ticket.Status != "Paid")
        {
            return false; // Only paid tickets can be checked in
        }

        if (ticket.CheckedInAt != null)
        {
            return false; // Already checked in
        }

        var showtime = await _unitOfWork.Showtimes.GetByIdAsync(ticket.ShowtimeId);
        if (showtime == null || showtime.EndTime < DateTime.UtcNow)
        {
            return false; // Showtime has ended
        }

        ticket.Status = "CheckedIn";
        ticket.CheckedInAt = DateTime.UtcNow;
        ticket.CheckedInBy = staffUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Tickets.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Ghi nhật ký kiểm toán
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = staffUserId,
            ActionName = "CheckInTicket",
            EntityName = "Ticket",
            EntityId = ticket.TicketId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<TicketDto>> GetTodayTicketsAsync()
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        // Lấy suất chiếu cho hôm nay
        var todayShowtimes = await _unitOfWork.Showtimes.FindAsync(s => 
            !s.IsDeleted && 
            s.StartTime >= todayStart && 
            s.StartTime < todayEnd);

        var showtimeIds = todayShowtimes.Select(s => s.ShowtimeId).ToList();
        
        if (showtimeIds.Count == 0)
        {
            return new List<TicketDto>();
        }

        // Lấy tất cả vé và lọc
        var allTickets = await _unitOfWork.Tickets.GetAllAsync();
        var todayTickets = allTickets
            .Where(t => !t.IsDeleted && 
                   showtimeIds.Contains(t.ShowtimeId) &&
                   (t.Status == "Paid" || t.Status == "CheckedIn"))
            .ToList();

        // Tải suất chiếu để sắp xếp
        var ticketDtos = new List<TicketDto>();
        foreach (var ticket in todayTickets)
        {
            var showtime = await _unitOfWork.Showtimes.GetByIdAsync(ticket.ShowtimeId);
            if (showtime != null)
            {
                var ticketDto = _mapper.Map<TicketDto>(ticket);
                ticketDtos.Add(ticketDto);
            }
        }

        return ticketDtos.OrderBy(t => t.ShowtimeStartTime).ToList();
    }

    public async Task<List<TicketDto>> GetTicketsByReservationAsync(int reservationId)
    {
        var tickets = await _unitOfWork.Tickets.GetTicketsByReservationAsync(reservationId);
        return _mapper.Map<List<TicketDto>>(tickets);
    }
}

