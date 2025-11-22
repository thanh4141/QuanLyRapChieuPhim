namespace CinemaBooking.Common.DTOs;

public class ShowtimeDto
{
    public int ShowtimeId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int AuditoriumId { get; set; }
    public string AuditoriumName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BaseTicketPrice { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public bool IsActive { get; set; }
}

public class CreateShowtimeRequest
{
    public int MovieId { get; set; }
    public int AuditoriumId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal BaseTicketPrice { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
}

public class UpdateShowtimeRequest
{
    public int MovieId { get; set; }
    public int AuditoriumId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal BaseTicketPrice { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public bool IsActive { get; set; }
}

