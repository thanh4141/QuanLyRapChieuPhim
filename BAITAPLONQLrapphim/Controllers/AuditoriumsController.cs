using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditoriumsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditoriumsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditoriumDto>>>> GetAuditoriums()
    {
        var auditoriums = await _unitOfWork.Auditoriums.FindAsync(a => !a.IsDeleted);
        var dtos = auditoriums.Select(a => new AuditoriumDto
        {
            AuditoriumId = a.AuditoriumId,
            Name = a.Name,
            LocationDescription = a.LocationDescription,
            Capacity = a.Capacity,
            IsActive = a.IsActive
        }).ToList();

        return Ok(ApiResponse<List<AuditoriumDto>>.SuccessResult(dtos));
    }

    [HttpGet("{id}/seats")]
    public async Task<ActionResult<ApiResponse<List<SeatDto>>>> GetSeats(int id, [FromQuery] int? showtimeId = null)
    {
        var seats = await _unitOfWork.Seats.FindAsync(s => s.AuditoriumId == id && !s.IsDeleted);
        var seatDtos = new List<SeatDto>();

        // Get booked seats for this showtime if provided
        var bookedSeatIds = new HashSet<int>();
        if (showtimeId.HasValue)
        {
            var bookedTickets = await _unitOfWork.Tickets.GetBookedTicketsForShowtimeAsync(showtimeId.Value);
            bookedSeatIds = bookedTickets.Select(t => t.SeatId).ToHashSet();
        }

        foreach (var seat in seats)
        {
            var seatType = await _unitOfWork.SeatTypes.GetByIdAsync(seat.SeatTypeId);
            seatDtos.Add(new SeatDto
            {
                SeatId = seat.SeatId,
                AuditoriumId = seat.AuditoriumId,
                RowLabel = seat.RowLabel,
                SeatNumber = seat.SeatNumber,
                SeatTypeId = seat.SeatTypeId,
                SeatTypeName = seatType?.SeatTypeName ?? "",
                PriceMultiplier = seatType?.PriceMultiplier ?? 1m,
                IsActive = seat.IsActive,
                IsBooked = bookedSeatIds.Contains(seat.SeatId)
            });
        }

        return Ok(ApiResponse<List<SeatDto>>.SuccessResult(seatDtos));
    }

    [HttpGet("seat-types")]
    public async Task<ActionResult<ApiResponse<List<SeatTypeDto>>>> GetSeatTypes()
    {
        var seatTypes = await _unitOfWork.SeatTypes.FindAsync(st => !st.IsDeleted);
        var dtos = seatTypes.Select(st => new SeatTypeDto
        {
            SeatTypeId = st.SeatTypeId,
            SeatTypeName = st.SeatTypeName,
            Description = st.Description,
            PriceMultiplier = st.PriceMultiplier
        }).ToList();

        return Ok(ApiResponse<List<SeatTypeDto>>.SuccessResult(dtos));
    }

    [HttpPost("{id}/seats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SeatDto>>> CreateSeat(int id, [FromBody] CreateSeatRequest request)
    {
        if (request.AuditoriumId != id)
        {
            return BadRequest(ApiResponse<SeatDto>.ErrorResult("Auditorium ID không khớp"));
        }

        var auditorium = await _unitOfWork.Auditoriums.GetByIdAsync(id);
        if (auditorium == null || auditorium.IsDeleted)
        {
            return NotFound(ApiResponse<SeatDto>.ErrorResult("Không tìm thấy phòng chiếu"));
        }

        var seatType = await _unitOfWork.SeatTypes.GetByIdAsync(request.SeatTypeId);
        if (seatType == null || seatType.IsDeleted)
        {
            return BadRequest(ApiResponse<SeatDto>.ErrorResult("Loại ghế không hợp lệ"));
        }

        // Check if seat already exists
        var existingSeat = await _unitOfWork.Seats.FirstOrDefaultAsync(s =>
            s.AuditoriumId == id &&
            s.RowLabel == request.RowLabel &&
            s.SeatNumber == request.SeatNumber &&
            !s.IsDeleted);

        if (existingSeat != null)
        {
            return BadRequest(ApiResponse<SeatDto>.ErrorResult($"Ghế {request.RowLabel}{request.SeatNumber} đã tồn tại"));
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var seat = new CinemaBooking.DAL.Entities.Seat
        {
            AuditoriumId = id,
            RowLabel = request.RowLabel,
            SeatNumber = request.SeatNumber,
            SeatTypeId = request.SeatTypeId,
            IsActive = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Seats.AddAsync(seat);
        await _unitOfWork.SaveChangesAsync();

        // Update auditorium capacity
        var seatCount = await _unitOfWork.Seats.CountAsync(s => s.AuditoriumId == id && !s.IsDeleted);
        auditorium.Capacity = seatCount;
        _unitOfWork.Auditoriums.Update(auditorium);
        await _unitOfWork.SaveChangesAsync();

        var seatDto = new SeatDto
        {
            SeatId = seat.SeatId,
            AuditoriumId = seat.AuditoriumId,
            RowLabel = seat.RowLabel,
            SeatNumber = seat.SeatNumber,
            SeatTypeId = seat.SeatTypeId,
            SeatTypeName = seatType.SeatTypeName,
            PriceMultiplier = seatType.PriceMultiplier,
            IsActive = seat.IsActive
        };

        return Ok(ApiResponse<SeatDto>.SuccessResult(seatDto, "Thêm ghế thành công"));
    }

    [HttpPost("{id}/seats/bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> BulkCreateSeats(int id, [FromBody] BulkCreateSeatsRequest request)
    {
        if (request.AuditoriumId != id)
        {
            return BadRequest(ApiResponse<int>.ErrorResult("Auditorium ID không khớp"));
        }

        if (request.StartSeatNumber > request.EndSeatNumber)
        {
            return BadRequest(ApiResponse<int>.ErrorResult("Số ghế bắt đầu phải nhỏ hơn số ghế kết thúc"));
        }

        if (request.EndSeatNumber - request.StartSeatNumber > 50)
        {
            return BadRequest(ApiResponse<int>.ErrorResult("Chỉ có thể tạo tối đa 50 ghế mỗi lần"));
        }

        var auditorium = await _unitOfWork.Auditoriums.GetByIdAsync(id);
        if (auditorium == null || auditorium.IsDeleted)
        {
            return NotFound(ApiResponse<int>.ErrorResult("Không tìm thấy phòng chiếu"));
        }

        var seatType = await _unitOfWork.SeatTypes.GetByIdAsync(request.SeatTypeId);
        if (seatType == null || seatType.IsDeleted)
        {
            return BadRequest(ApiResponse<int>.ErrorResult("Loại ghế không hợp lệ"));
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        int createdCount = 0;
        int skippedCount = 0;

        for (int seatNum = request.StartSeatNumber; seatNum <= request.EndSeatNumber; seatNum++)
        {
            // Check if seat already exists
            var existingSeat = await _unitOfWork.Seats.FirstOrDefaultAsync(s =>
                s.AuditoriumId == id &&
                s.RowLabel == request.RowLabel &&
                s.SeatNumber == seatNum &&
                !s.IsDeleted);

            if (existingSeat != null)
            {
                skippedCount++;
                continue;
            }

            var seat = new CinemaBooking.DAL.Entities.Seat
            {
                AuditoriumId = id,
                RowLabel = request.RowLabel,
                SeatNumber = seatNum,
                SeatTypeId = request.SeatTypeId,
                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Seats.AddAsync(seat);
            createdCount++;
        }

        await _unitOfWork.SaveChangesAsync();

        // Update auditorium capacity
        var seatCount = await _unitOfWork.Seats.CountAsync(s => s.AuditoriumId == id && !s.IsDeleted);
        auditorium.Capacity = seatCount;
        _unitOfWork.Auditoriums.Update(auditorium);
        await _unitOfWork.SaveChangesAsync();

        var message = $"Đã tạo {createdCount} ghế";
        if (skippedCount > 0)
        {
            message += $", bỏ qua {skippedCount} ghế đã tồn tại";
        }

        return Ok(ApiResponse<int>.SuccessResult(createdCount, message));
    }

    [HttpPut("seats/{seatId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SeatDto>>> UpdateSeat(int seatId, [FromBody] UpdateSeatRequest request)
    {
        var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
        if (seat == null || seat.IsDeleted)
        {
            return NotFound(ApiResponse<SeatDto>.ErrorResult("Không tìm thấy ghế"));
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check for duplicate if row/seat number is being changed
        if (request.RowLabel != null || request.SeatNumber.HasValue)
        {
            var newRowLabel = request.RowLabel ?? seat.RowLabel;
            var newSeatNumber = request.SeatNumber ?? seat.SeatNumber;

            var existingSeat = await _unitOfWork.Seats.FirstOrDefaultAsync(s =>
                s.AuditoriumId == seat.AuditoriumId &&
                s.RowLabel == newRowLabel &&
                s.SeatNumber == newSeatNumber &&
                s.SeatId != seatId &&
                !s.IsDeleted);

            if (existingSeat != null)
            {
                return BadRequest(ApiResponse<SeatDto>.ErrorResult($"Ghế {newRowLabel}{newSeatNumber} đã tồn tại"));
            }
        }

        if (request.RowLabel != null) seat.RowLabel = request.RowLabel;
        if (request.SeatNumber.HasValue) seat.SeatNumber = request.SeatNumber.Value;
        if (request.SeatTypeId.HasValue)
        {
            var seatType = await _unitOfWork.SeatTypes.GetByIdAsync(request.SeatTypeId.Value);
            if (seatType == null || seatType.IsDeleted)
            {
                return BadRequest(ApiResponse<SeatDto>.ErrorResult("Loại ghế không hợp lệ"));
            }
            seat.SeatTypeId = request.SeatTypeId.Value;
        }
        if (request.IsActive.HasValue) seat.IsActive = request.IsActive.Value;

        seat.UpdatedBy = userId;
        seat.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Seats.Update(seat);
        await _unitOfWork.SaveChangesAsync();

        var seatTypeEntity = await _unitOfWork.SeatTypes.GetByIdAsync(seat.SeatTypeId);
        var seatDto = new SeatDto
        {
            SeatId = seat.SeatId,
            AuditoriumId = seat.AuditoriumId,
            RowLabel = seat.RowLabel,
            SeatNumber = seat.SeatNumber,
            SeatTypeId = seat.SeatTypeId,
            SeatTypeName = seatTypeEntity?.SeatTypeName ?? "",
            PriceMultiplier = seatTypeEntity?.PriceMultiplier ?? 1m,
            IsActive = seat.IsActive
        };

        return Ok(ApiResponse<SeatDto>.SuccessResult(seatDto, "Cập nhật ghế thành công"));
    }

    [HttpDelete("seats/{seatId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeat(int seatId)
    {
        var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
        if (seat == null || seat.IsDeleted)
        {
            return NotFound(ApiResponse<bool>.ErrorResult("Không tìm thấy ghế"));
        }

        // Check if seat has active tickets
        var hasTickets = await _unitOfWork.Tickets.ExistsAsync(t => t.SeatId == seatId && t.Status != "Cancelled");
        if (hasTickets)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Không thể xóa ghế đã có vé. Vui lòng vô hiệu hóa thay vì xóa."));
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        seat.IsDeleted = true;
        seat.UpdatedBy = userId;
        seat.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Seats.Update(seat);

        // Update auditorium capacity
        var auditorium = await _unitOfWork.Auditoriums.GetByIdAsync(seat.AuditoriumId);
        if (auditorium != null)
        {
            var seatCount = await _unitOfWork.Seats.CountAsync(s => s.AuditoriumId == seat.AuditoriumId && !s.IsDeleted);
            auditorium.Capacity = seatCount;
            _unitOfWork.Auditoriums.Update(auditorium);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse<bool>.SuccessResult(true, "Xóa ghế thành công"));
    }
}

