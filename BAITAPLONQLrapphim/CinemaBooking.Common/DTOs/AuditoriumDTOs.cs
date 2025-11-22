namespace CinemaBooking.Common.DTOs;

public class AuditoriumDto
{
    public int AuditoriumId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocationDescription { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}

public class SeatDto
{
    public int SeatId { get; set; }
    public int AuditoriumId { get; set; }
    public string RowLabel { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public int SeatTypeId { get; set; }
    public string SeatTypeName { get; set; } = string.Empty;
    public decimal PriceMultiplier { get; set; }
    public bool IsActive { get; set; }
    public bool IsBooked { get; set; }
}

public class SeatTypeDto
{
    public int SeatTypeId { get; set; }
    public string SeatTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PriceMultiplier { get; set; }
}

public class CreateSeatRequest
{
    public int AuditoriumId { get; set; }
    public string RowLabel { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public int SeatTypeId { get; set; }
}

public class BulkCreateSeatsRequest
{
    public int AuditoriumId { get; set; }
    public string RowLabel { get; set; } = string.Empty;
    public int StartSeatNumber { get; set; }
    public int EndSeatNumber { get; set; }
    public int SeatTypeId { get; set; }
}

public class UpdateSeatRequest
{
    public string? RowLabel { get; set; }
    public int? SeatNumber { get; set; }
    public int? SeatTypeId { get; set; }
    public bool? IsActive { get; set; }
}

