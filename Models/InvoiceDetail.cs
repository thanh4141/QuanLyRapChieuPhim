using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class InvoiceDetail
{
    public string MaCthd { get; set; } = null!;

    public string? MaHoaDon { get; set; }

    public string? MaSp { get; set; }

    public int? SoLuong { get; set; }

    public virtual Invoice? MaHoaDonNavigation { get; set; }

    public virtual Product? MaSpNavigation { get; set; }
}
