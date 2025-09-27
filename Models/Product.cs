using System;
using System.Collections.Generic;

namespace QuanLyRapPhim.Models;

public partial class Product
{
    public string MaSp { get; set; } = null!;

    public string? TenSp { get; set; }

    public decimal? Gia { get; set; }

    public string? Loai { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
