using System;
using System.Collections.Generic;

namespace Identity_Service.Models;

public partial class Staff
{
    public string MaNv { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? ChucVu { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? SoDienThoai { get; set; }
}
