using CinemaBooking.Common.DTOs;

namespace CinemaBooking.BLL.Services;

public interface IReportService
{
    Task<RevenueByDateResponse> GetRevenueByDateAsync(RevenueByDateRequest request);
    Task<RevenueByMovieResponse> GetRevenueByMovieAsync(RevenueByMovieRequest request);
    Task<List<TopShowtimeDto>> GetTopShowtimesAsync(DateTime from, DateTime to, int top = 10);
}

