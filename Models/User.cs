using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string MatKhau { get; set; } = null!;

    public string? Role { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
