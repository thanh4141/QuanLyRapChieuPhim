using CinemaBooking.BLL.Services;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;

    public ShowtimesController(IShowtimeService showtimeService)
    {
        _showtimeService = showtimeService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ShowtimeDto>>>> GetShowtimes(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? movieId = null,
        [FromQuery] DateTime? date = null,
        [FromQuery] int? auditoriumId = null)
    {
        var request = new PagedRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _showtimeService.GetShowtimesAsync(request, movieId, date, auditoriumId);
        return Ok(ApiResponse<PagedResult<ShowtimeDto>>.SuccessResult(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ShowtimeDto>>> GetShowtime(int id)
    {
        var showtime = await _showtimeService.GetShowtimeByIdAsync(id);
        if (showtime == null)
        {
            return NotFound(ApiResponse<ShowtimeDto>.ErrorResult("Showtime not found"));
        }
        return Ok(ApiResponse<ShowtimeDto>.SuccessResult(showtime));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ShowtimeDto>>> CreateShowtime([FromBody] CreateShowtimeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ShowtimeDto>.ErrorResult("Dữ liệu không hợp lệ"));
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var showtime = await _showtimeService.CreateShowtimeAsync(request, userId);
            return Ok(ApiResponse<ShowtimeDto>.SuccessResult(showtime, "Tạo suất chiếu thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ShowtimeDto>.ErrorResult(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ShowtimeDto>>> UpdateShowtime(int id, [FromBody] UpdateShowtimeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ShowtimeDto>.ErrorResult("Dữ liệu không hợp lệ"));
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var showtime = await _showtimeService.UpdateShowtimeAsync(id, request, userId);
            if (showtime == null)
            {
                return NotFound(ApiResponse<ShowtimeDto>.ErrorResult("Không tìm thấy suất chiếu"));
            }
            return Ok(ApiResponse<ShowtimeDto>.SuccessResult(showtime, "Cập nhật suất chiếu thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ShowtimeDto>.ErrorResult(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteShowtime(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _showtimeService.DeleteShowtimeAsync(id, userId);
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResult("Showtime not found"));
        }
        return Ok(ApiResponse<bool>.SuccessResult(result, "Showtime deleted successfully"));
    }
}

