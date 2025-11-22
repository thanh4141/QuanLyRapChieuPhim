namespace CinemaBooking.Common.DTOs;

public class RevenueByDateRequest
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class RevenueByDateResponse
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Revenues { get; set; } = new();
    public decimal TotalRevenue { get; set; }
}

public class RevenueByMovieRequest
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class RevenueByMovieResponse
{
    public List<string> MovieTitles { get; set; } = new();
    public List<decimal> Revenues { get; set; } = new();
    public decimal TotalRevenue { get; set; }
}

public class TopShowtimeDto
{
    public int ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string AuditoriumName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}

