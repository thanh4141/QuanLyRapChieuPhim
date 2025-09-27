using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Payment
{
    public string MaThanhToan { get; set; } = null!;

    public string MaHoaDon { get; set; } = null!;

    public decimal SoTien { get; set; }

    public string HinhThucTt { get; set; } = null!;

    public DateTime? NgayThanhToan { get; set; }

    public virtual Invoice MaHoaDonNavigation { get; set; } = null!;
}
