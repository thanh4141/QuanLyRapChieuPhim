using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Seat
{
    public string MaGhe { get; set; } = null!;

    public string MaPhong { get; set; } = null!;

    public string SoGhe { get; set; } = null!;

    public string? LoaiGhe { get; set; }

    public virtual Auditorium MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
