using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Auditorium
{
    public string MaPhong { get; set; } = null!;

    public string TenPhong { get; set; } = null!;

    public int SucChua { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
