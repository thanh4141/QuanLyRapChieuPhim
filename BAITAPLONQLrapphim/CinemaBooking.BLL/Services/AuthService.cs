using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Entities;
using CinemaBooking.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CinemaBooking.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if username or email already exists
        var existingUser = await _unitOfWork.Users.GetByNormalizedUsernameAsync(request.Username.ToUpperInvariant());
        if (existingUser != null)
        {
            return null; // Username already exists
        }

        existingUser = await _unitOfWork.Users.GetByNormalizedEmailAsync(request.Email.ToUpperInvariant());
        if (existingUser != null)
        {
            return null; // Email already exists
        }

        // Create new user
        var user = new User
        {
            Username = request.Username,
            NormalizedUsername = request.Username.ToUpperInvariant(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign Customer role by default
        var customerRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.NormalizedRoleName == "CUSTOMER" && !r.IsDeleted);
        if (customerRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = customerRole.RoleId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.UserRoles.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        // Generate tokens
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new UserRefreshToken
        {
            UserId = user.UserId,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.UserRefreshTokens.AddAsync(refreshTokenEntity);
        
        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.UserId,
            ActionName = "Register",
            EntityName = "User",
            EntityId = user.UserId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        var userDto = await GetUserDtoAsync(user.UserId);
        if (userDto == null) return null;

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByNormalizedUsernameAsync(request.Username.ToUpperInvariant());
        if (user == null || !user.IsActive || user.IsDeleted)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Generate tokens
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new UserRefreshToken
        {
            UserId = user.UserId,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.UserRefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.UserId,
            ActionName = "Login",
            EntityName = "User",
            EntityId = user.UserId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        var userDto = await GetUserDtoAsync(user.UserId);
        if (userDto == null) return null;

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshTokenEntity = await _unitOfWork.UserRefreshTokens.FirstOrDefaultAsync(rt => 
            rt.RefreshToken == request.RefreshToken && 
            rt.ExpiresAt > DateTime.UtcNow &&
            rt.RevokedAt == null);

        if (refreshTokenEntity == null)
        {
            return null;
        }

        var user = await _unitOfWork.Users.GetByIdAsync(refreshTokenEntity.UserId);

        if (user == null || !user.IsActive || user.IsDeleted)
        {
            return null;
        }

        // Revoke old refresh token
        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
        _unitOfWork.UserRefreshTokens.Update(refreshTokenEntity);

        // Generate new tokens
        var token = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Save new refresh token
        var newRefreshTokenEntity = new UserRefreshToken
        {
            UserId = user.UserId,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.UserRefreshTokens.AddAsync(newRefreshTokenEntity);

        await _unitOfWork.SaveChangesAsync();

        var userDto = await GetUserDtoAsync(user.UserId);
        if (userDto == null) return null;

        return new AuthResponse
        {
            Token = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken, int userId)
    {
        // Find and revoke the specific refresh token
        var refreshTokenEntity = await _unitOfWork.UserRefreshTokens.FirstOrDefaultAsync(rt => 
            rt.RefreshToken == refreshToken && 
            rt.UserId == userId &&
            rt.RevokedAt == null);

        if (refreshTokenEntity != null)
        {
            refreshTokenEntity.RevokedAt = DateTime.UtcNow;
            _unitOfWork.UserRefreshTokens.Update(refreshTokenEntity);
        }

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "Logout",
            EntityName = "User",
            EntityId = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByNormalizedEmailAsync(request.Email.ToUpperInvariant());
        if (user == null || user.IsDeleted)
        {
            return false; // Don't reveal if user exists
        }

        // Invalidate old unused tokens for this user
        var oldTokens = await _unitOfWork.PasswordResetTokens.FindAsync(prt =>
            prt.UserId == user.UserId &&
            prt.UsedAt == null &&
            prt.ExpiresAt > DateTime.UtcNow);

        foreach (var oldToken in oldTokens)
        {
            oldToken.UsedAt = DateTime.UtcNow; // Mark as used to invalidate
            _unitOfWork.PasswordResetTokens.Update(oldToken);
        }

        // Generate new token
        var token = GeneratePasswordResetToken();
        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PasswordResetTokens.AddAsync(resetToken);

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.UserId,
            ActionName = "ForgotPassword",
            EntityName = "PasswordResetToken",
            EntityId = resetToken.Token,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // In real implementation, send email with token
        // For development, you can log the token or return it in response
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByNormalizedEmailAsync(request.Email.ToUpperInvariant());
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Verify token
        var resetToken = await _unitOfWork.PasswordResetTokens.FirstOrDefaultAsync(prt =>
            prt.UserId == user.UserId &&
            prt.Token == request.Token &&
            prt.ExpiresAt > DateTime.UtcNow &&
            prt.UsedAt == null);

        if (resetToken == null)
        {
            return false; // Invalid or expired token
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);

        // Mark token as used
        resetToken.UsedAt = DateTime.UtcNow;
        _unitOfWork.PasswordResetTokens.Update(resetToken);

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.UserId,
            ActionName = "ResetPassword",
            EntityName = "User",
            EntityId = user.UserId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateStaffUserAsync(string username, string email, string password, string fullName)
    {
        // Check if username or email already exists
        if (await CheckUsernameExistsAsync(username) || await CheckEmailExistsAsync(email))
        {
            return false;
        }

        var user = new User
        {
            Username = username,
            NormalizedUsername = username.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var staffRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.NormalizedRoleName == "STAFF" && !r.IsDeleted);
        if (staffRole == null)
        {
            staffRole = new Role { RoleName = "Staff", NormalizedRoleName = "STAFF", Description = "Nhân viên bán vé", CreatedAt = DateTime.UtcNow };
            await _unitOfWork.Roles.AddAsync(staffRole);
            await _unitOfWork.SaveChangesAsync();
        }

        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = staffRole.RoleId,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.UserRoles.AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.UserId,
            ActionName = "CreateStaffUser",
            EntityName = "User",
            EntityId = user.UserId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string?> GetPasswordResetTokenAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByNormalizedEmailAsync(email.ToUpperInvariant());
        if (user == null || user.IsDeleted)
        {
            return null;
        }

        // Get the most recent unused token
        var resetToken = await _unitOfWork.PasswordResetTokens.FirstOrDefaultAsync(prt =>
            prt.UserId == user.UserId &&
            prt.UsedAt == null &&
            prt.ExpiresAt > DateTime.UtcNow);

        return resetToken?.Token;
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        return await GetUserDtoAsync(userId);
    }

    private async Task<UserDto?> GetUserDtoAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
        if (user == null) return null;

        var roles = await _unitOfWork.Users.GetUserRolesAsync(userId);
        var roleNames = roles.Select(r => r.RoleName).ToList();

        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Roles = roleNames
        };
    }

    private string GenerateJwtToken(User user)
    {
        var roles = _unitOfWork.Users.GetUserRolesAsync(user.UserId).Result;
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r.RoleName)).ToList();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roleClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(60);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GeneratePasswordResetToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<bool> CheckUsernameExistsAsync(string username)
    {
        var existingUser = await _unitOfWork.Users.GetByNormalizedUsernameAsync(username.ToUpperInvariant());
        return existingUser != null;
    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        var existingUser = await _unitOfWork.Users.GetByNormalizedEmailAsync(email.ToUpperInvariant());
        return existingUser != null;
    }

    public async Task<bool> CreateAdminUserAsync(string username, string email, string password, string fullName)
    {
        // Check if admin user already exists
        var existingUser = await _unitOfWork.Users.GetByNormalizedUsernameAsync(username.ToUpperInvariant());
        if (existingUser != null)
        {
            return false; // User already exists
        }

        // Get or create Admin role
        var adminRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.NormalizedRoleName == "ADMIN" && !r.IsDeleted);
        if (adminRole == null)
        {
            // Create Admin role if not exists
            adminRole = new Role
            {
                RoleName = "Admin",
                NormalizedRoleName = "ADMIN",
                Description = "Quản trị hệ thống",
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Roles.AddAsync(adminRole);
            await _unitOfWork.SaveChangesAsync();
        }

        // Create admin user
        var user = new User
        {
            Username = username,
            NormalizedUsername = username.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            IsEmailConfirmed = true,
            IsPhoneConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign Admin role
        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = adminRole.RoleId,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.UserRoles.AddAsync(userRole);

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.UserId,
            ActionName = "CreateAdminUser",
            EntityName = "User",
            EntityId = user.UserId.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

