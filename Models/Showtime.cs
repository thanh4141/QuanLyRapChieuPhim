using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Showtime
{
    public string MaSuatChieu { get; set; } = null!;

    public string MaPhim { get; set; } = null!;

    public string MaPhong { get; set; } = null!;

    public DateTime GioBatDau { get; set; }

    public DateTime GioKetThuc { get; set; }

    public virtual Movie MaPhimNavigation { get; set; } = null!;

    public virtual Auditorium MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
