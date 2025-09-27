using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Promotion
{
    public string MaKm { get; set; } = null!;

    public string? TenKm { get; set; }

    public string? MoTa { get; set; }

    public decimal? GiamGia { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
