using System;
using System.Collections.Generic;

namespace Identity_Service.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string MatKhau { get; set; } = null!;

    public string? Role { get; set; }
}
