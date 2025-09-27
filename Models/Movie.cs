using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Movie
{
    public string MaPhim { get; set; } = null!;

    public string TenPhim { get; set; } = null!;

    public string? TheLoai { get; set; }

    public int ThoiLuong { get; set; }

    public DateOnly? NgayKhoiChieu { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
