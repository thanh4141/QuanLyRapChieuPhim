using CinemaBooking.BLL.Services;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<ApiResponse<BookingPreviewResponse>>> PreviewBooking([FromBody] BookingPreviewRequest request)
    {
        if (request == null || request.ShowtimeId <= 0 || request.SeatIds == null || request.SeatIds.Count == 0)
        {
            return BadRequest(ApiResponse<BookingPreviewResponse>.ErrorResult("Thông tin đặt vé không hợp lệ"));
        }

        var result = await _bookingService.GetBookingPreviewAsync(request);
        if (result == null)
        {
            return BadRequest(ApiResponse<BookingPreviewResponse>.ErrorResult("Suất chiếu hoặc ghế không hợp lệ"));
        }
        return Ok(ApiResponse<BookingPreviewResponse>.SuccessResult(result, "Xem trước đặt vé thành công"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingDto>>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        if (request == null || request.ShowtimeId <= 0 || request.SeatIds == null || request.SeatIds.Count == 0)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResult("Vui lòng chọn ít nhất một ghế"));
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.CreateBookingAsync(request, userId);
            if (result == null)
            {
                return BadRequest(ApiResponse<BookingDto>.ErrorResult("Một số ghế đã được đặt hoặc không còn trống. Vui lòng chọn ghế khác."));
            }
            return Ok(ApiResponse<BookingDto>.SuccessResult(result, "Đặt vé thành công! Vui lòng thanh toán trong vòng 15 phút."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResult($"Lỗi khi đặt vé: {ex.Message}"));
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetMyBookings(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var request = new PagedRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _bookingService.GetUserBookingsAsync(userId, request);
        return Ok(ApiResponse<PagedResult<BookingDto>>.SuccessResult(result));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelBooking(int id, [FromBody] CancelBookingRequest? request = null)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var reason = request?.Reason;
        var result = await _bookingService.CancelBookingAsync(id, userId, reason);
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Cannot cancel this booking"));
        }
        return Ok(ApiResponse<bool>.SuccessResult(result, "Booking cancelled successfully"));
    }

    [HttpPost("staff/direct")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> CreateDirectBooking([FromBody] CreateDirectBookingRequest request)
    {
        if (request == null || request.ShowtimeId <= 0 || request.SeatIds == null || request.SeatIds.Count == 0)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResult("Vui lòng chọn ít nhất một ghế"));
        }

        try
        {
            var staffUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.CreateDirectBookingAsync(request, staffUserId);
            if (result == null)
            {
                return BadRequest(ApiResponse<BookingDto>.ErrorResult("Một số ghế đã được đặt hoặc không còn trống. Vui lòng chọn ghế khác."));
            }
            return Ok(ApiResponse<BookingDto>.SuccessResult(result, "Đặt vé trực tiếp thành công! Vé đã được thanh toán."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BookingDto>.ErrorResult($"Lỗi khi đặt vé: {ex.Message}"));
        }
    }
}

