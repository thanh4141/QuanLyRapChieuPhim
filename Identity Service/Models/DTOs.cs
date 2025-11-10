using System.ComponentModel.DataAnnotations;

namespace Identity_Service.Models.DTO
{
    public class RegisterDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string HoTen { get; set; } = null!;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }

        [Required]
        public string MatKhau { get; set; } = null!;
    }

    public class LoginDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string MatKhau { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string MatKhauCu { get; set; } = null!;

        [Required]
        public string MatKhauMoi { get; set; } = null!;
    }
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}