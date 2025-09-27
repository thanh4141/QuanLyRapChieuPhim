using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Invoice
{
    public string MaHoaDon { get; set; } = null!;

    public string MaDatCho { get; set; } = null!;

    public decimal TongTien { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? MaNv { get; set; }

    public string? MaKm { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual Reservation MaDatChoNavigation { get; set; } = null!;

    public virtual Promotion? MaKmNavigation { get; set; }

    public virtual Staff? MaNvNavigation { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
