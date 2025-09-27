using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Reservation
{
    public string MaDatCho { get; set; } = null!;

    public string MaSuatChieu { get; set; } = null!;

    public string MaGhe { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime? ThoiGianDat { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Seat MaGheNavigation { get; set; } = null!;

    public virtual Showtime MaSuatChieuNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User User { get; set; } = null!;
}
