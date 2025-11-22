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
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetMyTickets()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tickets = await _ticketService.GetUserTicketsAsync(userId);
        return Ok(ApiResponse<List<TicketDto>>.SuccessResult(tickets));
    }

    [HttpGet("qr/{qrCodeData}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> GetTicketByQrCode(string qrCodeData)
    {
        if (string.IsNullOrWhiteSpace(qrCodeData))
        {
            return BadRequest(ApiResponse<TicketDto>.ErrorResult("Mã QR code không hợp lệ"));
        }

        var ticket = await _ticketService.GetTicketByQrCodeAsync(qrCodeData);
        if (ticket == null)
        {
            return NotFound(ApiResponse<TicketDto>.ErrorResult("Không tìm thấy vé với mã QR code này"));
        }
        return Ok(ApiResponse<TicketDto>.SuccessResult(ticket));
    }

    [HttpPost("checkin")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckInTicket([FromBody] CheckInRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.QrCodeData))
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Mã QR code không hợp lệ"));
        }

        try
        {
            var staffUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _ticketService.CheckInTicketAsync(request.QrCodeData, staffUserId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Không thể check-in vé này. Vé có thể đã được check-in, chưa thanh toán, hoặc suất chiếu đã kết thúc."));
            }
            return Ok(ApiResponse<bool>.SuccessResult(result, "Check-in vé thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult($"Lỗi khi check-in: {ex.Message}"));
        }
    }

    [HttpGet("today")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetTodayTickets()
    {
        var tickets = await _ticketService.GetTodayTicketsAsync();
        return Ok(ApiResponse<List<TicketDto>>.SuccessResult(tickets));
    }

    [HttpGet("reservation/{reservationId}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetTicketsByReservation(int reservationId)
    {
        var tickets = await _ticketService.GetTicketsByReservationAsync(reservationId);
        return Ok(ApiResponse<List<TicketDto>>.SuccessResult(tickets));
    }
}

