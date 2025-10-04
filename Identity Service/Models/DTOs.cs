namespace Identity_Service.Models.DTO
{
    public class RegisterDto
    {
        public string UserId { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string MatKhau { get; set; } = null!;
    }

    public class LoginDto
    {
        public string UserId { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
    }
}
