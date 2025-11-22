using CinemaBooking.BLL.Services;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        // Check username first
        var existingUserByUsername = await _authService.CheckUsernameExistsAsync(request.Username);
        if (existingUserByUsername)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResult("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác."));
        }

        // Check email
        var existingUserByEmail = await _authService.CheckEmailExistsAsync(request.Email);
        if (existingUserByEmail)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResult("Email đã được sử dụng. Vui lòng sử dụng email khác."));
        }

        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResult("Đăng ký thất bại. Vui lòng thử lại."));
        }
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Đăng ký thành công!"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResult("Invalid username or password"));
        }
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Login successful"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResult("Invalid refresh token"));
        }
        return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Token refreshed"));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RefreshTokenRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.LogoutAsync(request.RefreshToken, userId);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Logout successful"));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponse>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        if (!result)
        {
            // Don't reveal if email exists for security
            return Ok(ApiResponse<ForgotPasswordResponse>.SuccessResult(
                new ForgotPasswordResponse { Token = null }, 
                "Nếu email tồn tại, link đặt lại mật khẩu đã được gửi"));
        }

        // For development: return token in response
        // In production, this should be sent via email only
        var token = await _authService.GetPasswordResetTokenAsync(request.Email);
        return Ok(ApiResponse<ForgotPasswordResponse>.SuccessResult(
            new ForgotPasswordResponse { Token = token }, 
            "Link đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra email."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Token, email và mật khẩu mới là bắt buộc"));
        }

        if (request.NewPassword.Length < 6)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Mật khẩu phải có ít nhất 6 ký tự"));
        }

        var result = await _authService.ResetPasswordAsync(request);
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Token không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu link đặt lại mật khẩu mới."));
        }
        return Ok(ApiResponse<bool>.SuccessResult(result, "Đặt lại mật khẩu thành công!"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.ErrorResult("User not found"));
        }
        return Ok(ApiResponse<UserDto>.SuccessResult(user));
    }

    [HttpPost("create-admin")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateAdminUser([FromBody] CreateAdminRequest request)
    {
        // Simple validation
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Username, email và password là bắt buộc"));
        }

        var result = await _authService.CreateAdminUserAsync(
            request.Username, 
            request.Email, 
            request.Password, 
            request.FullName ?? "Quản trị viên"
        );

        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Không thể tạo admin user. Có thể user đã tồn tại."));
        }

        return Ok(ApiResponse<bool>.SuccessResult(true, "Đã tạo tài khoản admin thành công!"));
    }

    [HttpPost("create-staff")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateStaffUser([FromBody] CreateStaffRequest request)
    {
        // Simple validation
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Username, email và password là bắt buộc"));
        }

        var result = await _authService.CreateStaffUserAsync(
            request.Username, 
            request.Email, 
            request.Password, 
            request.FullName ?? "Nhân viên"
        );

        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Không thể tạo staff user. Có thể user đã tồn tại."));
        }

        return Ok(ApiResponse<bool>.SuccessResult(true, "Đã tạo tài khoản nhân viên thành công!"));
    }
}

