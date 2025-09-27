using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Ticket
{
    public string MaVe { get; set; } = null!;

    public string MaDatCho { get; set; } = null!;

    public decimal GiaVe { get; set; }

    public string? TrangThai { get; set; }

    public virtual Reservation MaDatChoNavigation { get; set; } = null!;
}
