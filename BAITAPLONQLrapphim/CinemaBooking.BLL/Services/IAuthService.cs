using CinemaBooking.Common.DTOs;

namespace CinemaBooking.BLL.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(string refreshToken, int userId);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
    Task<bool> CheckUsernameExistsAsync(string username);
    Task<bool> CheckEmailExistsAsync(string email);
    Task<bool> CreateAdminUserAsync(string username, string email, string password, string fullName);
    Task<bool> CreateStaffUserAsync(string username, string email, string password, string fullName);
    Task<string?> GetPasswordResetTokenAsync(string email);
}

