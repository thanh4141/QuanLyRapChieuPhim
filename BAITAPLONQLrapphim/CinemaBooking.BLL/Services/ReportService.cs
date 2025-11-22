using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.BLL.Services;

public class ReportService : IReportService
{
    private readonly CinemaDbContext _context;

    public ReportService(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueByDateResponse> GetRevenueByDateAsync(RevenueByDateRequest request)
    {
        var payments = await _context.Payments
            .Where(p => p.PaymentStatus == "Success" 
                && p.PaidAt >= request.From 
                && p.PaidAt <= request.To 
                && !p.IsDeleted)
            .ToListAsync();

        var grouped = payments
            .GroupBy(p => p.PaidAt!.Value.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var response = new RevenueByDateResponse
        {
            Labels = grouped.Select(g => g.Key.ToString("yyyy-MM-dd")).ToList(),
            Revenues = grouped.Select(g => g.Sum(p => p.Amount)).ToList(),
            TotalRevenue = payments.Sum(p => p.Amount)
        };

        return response;
    }

    public async Task<RevenueByMovieResponse> GetRevenueByMovieAsync(RevenueByMovieRequest request)
    {
        var movieRevenue = await (from payment in _context.Payments
                                  join invoice in _context.Invoices on payment.InvoiceId equals invoice.InvoiceId
                                  join reservation in _context.Reservations on invoice.ReservationId equals reservation.ReservationId
                                  join showtime in _context.Showtimes on reservation.ShowtimeId equals showtime.ShowtimeId
                                  join movie in _context.Movies on showtime.MovieId equals movie.MovieId
                                  where payment.PaymentStatus == "Success"
                                     && payment.PaidAt != null
                                     && payment.PaidAt!.Value >= request.From
                                     && payment.PaidAt!.Value <= request.To
                                     && !payment.IsDeleted
                                     && !invoice.IsDeleted
                                     && !reservation.IsDeleted
                                     && !showtime.IsDeleted
                                     && !movie.IsDeleted
                                  group payment by movie into g
                                  select new
                                  {
                                      MovieTitle = g.Key.Title,
                                      Revenue = g.Sum(p => p.Amount)
                                  })
                                  .OrderByDescending(m => m.Revenue)
                                  .ToListAsync();

        var totalRevenue = await _context.Payments
            .Where(p => p.PaymentStatus == "Success"
                && p.PaidAt >= request.From
                && p.PaidAt <= request.To
                && !p.IsDeleted)
            .SumAsync(p => p.Amount);

        var response = new RevenueByMovieResponse
        {
            MovieTitles = movieRevenue.Select(m => m.MovieTitle).ToList(),
            Revenues = movieRevenue.Select(m => m.Revenue).ToList(),
            TotalRevenue = totalRevenue
        };

        return response;
    }

    public async Task<List<TopShowtimeDto>> GetTopShowtimesAsync(DateTime from, DateTime to, int top = 10)
    {
        var payments = await _context.Payments
            .Where(p => p.PaymentStatus == "Success"
                && p.PaidAt != null
                && p.PaidAt.Value >= from
                && p.PaidAt.Value <= to
                && !p.IsDeleted)
            .Include(p => p.Invoice)
            .ToListAsync();

        var invoiceIds = payments.Select(p => p.InvoiceId).Distinct().ToList();
        var invoices = await _context.Invoices
            .Where(i => invoiceIds.Contains(i.InvoiceId) && !i.IsDeleted)
            .Include(i => i.Reservation)
            .ToListAsync();

        var reservationIds = invoices.Select(i => i.ReservationId).Distinct().ToList();
        var reservations = await _context.Reservations
            .Where(r => reservationIds.Contains(r.ReservationId) && !r.IsDeleted)
            .Include(r => r.Showtime)
            .ToListAsync();

        var showtimeIds = reservations.Select(r => r.ShowtimeId).Distinct().ToList();
        var showtimes = await _context.Showtimes
            .Where(s => showtimeIds.Contains(s.ShowtimeId) && !s.IsDeleted)
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .ToListAsync();

        var tickets = await _context.Tickets
            .Where(t => reservationIds.Contains(t.ReservationId) && !t.IsDeleted)
            .ToListAsync();

        var showtimeStats = from payment in payments
                           join invoice in invoices on payment.InvoiceId equals invoice.InvoiceId
                           join reservation in reservations on invoice.ReservationId equals reservation.ReservationId
                           join showtime in showtimes on reservation.ShowtimeId equals showtime.ShowtimeId
                           group new { payment, reservation } by new { showtime, showtime.Movie, showtime.Auditorium } into g
                           select new TopShowtimeDto
                           {
                               ShowtimeId = g.Key.showtime.ShowtimeId,
                               MovieTitle = g.Key.Movie?.Title ?? "N/A",
                               AuditoriumName = g.Key.Auditorium?.Name ?? "N/A",
                               StartTime = g.Key.showtime.StartTime,
                               TicketCount = tickets.Count(t => t.ReservationId == g.Select(r => r.reservation.ReservationId).FirstOrDefault()),
                               Revenue = g.Sum(p => p.payment.Amount)
                           };

        return showtimeStats
            .OrderByDescending(s => s.TicketCount)
            .ThenByDescending(s => s.Revenue)
            .Take(top)
            .ToList();
    }
}

