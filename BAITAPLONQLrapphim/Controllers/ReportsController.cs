using CinemaBooking.BLL.Services;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue-by-date")]
    public async Task<ActionResult<ApiResponse<RevenueByDateResponse>>> GetRevenueByDate(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var request = new RevenueByDateRequest { From = from, To = to };
        var result = await _reportService.GetRevenueByDateAsync(request);
        return Ok(ApiResponse<RevenueByDateResponse>.SuccessResult(result));
    }

    [HttpGet("revenue-by-movie")]
    public async Task<ActionResult<ApiResponse<RevenueByMovieResponse>>> GetRevenueByMovie(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var request = new RevenueByMovieRequest { From = from, To = to };
        var result = await _reportService.GetRevenueByMovieAsync(request);
        return Ok(ApiResponse<RevenueByMovieResponse>.SuccessResult(result));
    }

    [HttpGet("top-showtimes")]
    public async Task<ActionResult<ApiResponse<List<TopShowtimeDto>>>> GetTopShowtimes(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int top = 10)
    {
        var result = await _reportService.GetTopShowtimesAsync(from, to, top);
        return Ok(ApiResponse<List<TopShowtimeDto>>.SuccessResult(result));
    }
}

