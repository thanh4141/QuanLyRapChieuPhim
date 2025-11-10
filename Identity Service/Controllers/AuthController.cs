using Identity_Service.Models;
using Identity_Service.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Identity_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connStr;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(config), "Connection string không được để trống");
            _config = config;
            _passwordHasher = new PasswordHasher<User>();
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Validation đầu vào
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            // 2. Validate password strength
            var passwordValidation = ValidatePassword(dto.MatKhau);
            if (!passwordValidation.IsValid)
            {
                return BadRequest(new { message = passwordValidation.ErrorMessage });
            }

            // 3. Validate email format (nếu có)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !IsValidEmail(dto.Email))
            {
                return BadRequest(new { message = "Email không hợp lệ" });
            }

            // 4. Validate phone number (nếu có)
            if (!string.IsNullOrWhiteSpace(dto.SoDienThoai) && !IsValidVietnamesePhone(dto.SoDienThoai))
            {
                return BadRequest(new { message = "Số điện thoại không hợp lệ (định dạng: 0xxxxxxxxx)" });
            }

            SqlConnection? con = null;
            try
            {
                con = new SqlConnection(_connStr);
                await con.OpenAsync();

                // 5. Kiểm tra UserId đã tồn tại chưa
                if (await UserExistsAsync(con, dto.UserId))
                {
                    _logger.LogWarning("Attempt to register existing UserId: {UserId}", dto.UserId);
                    return Conflict(new { message = $"UserId '{dto.UserId}' đã tồn tại" });
                }

                // 6. Kiểm tra Email đã tồn tại chưa (nếu có)
                if (!string.IsNullOrWhiteSpace(dto.Email) && await EmailExistsAsync(con, dto.Email))
                {
                    _logger.LogWarning("Attempt to register existing Email: {Email}", dto.Email);
                    return Conflict(new { message = "Email đã được sử dụng" });
                }

                // 7. Hash password
                var userForHashing = new User { UserId = dto.UserId };
                var hashedPassword = _passwordHasher.HashPassword(userForHashing, dto.MatKhau);

                // 8. Insert user vào database
                using var cmd = new SqlCommand("sp_RegisterUser", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30; // Timeout 30 giây

                cmd.Parameters.AddWithValue("@UserId", dto.UserId);
                cmd.Parameters.AddWithValue("@HoTen", dto.HoTen);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(dto.Email) ? DBNull.Value : dto.Email);
                cmd.Parameters.AddWithValue("@SoDienThoai", string.IsNullOrWhiteSpace(dto.SoDienThoai) ? DBNull.Value : dto.SoDienThoai);
                cmd.Parameters.AddWithValue("@MatKhau", hashedPassword);

                var rows = await cmd.ExecuteNonQueryAsync();

                if (rows > 0)
                {
                    _logger.LogInformation("User registered successfully: {UserId}", dto.UserId);
                    return Ok(new
                    {
                        message = "Đăng ký thành công",
                        userId = dto.UserId,
                        registeredAt = DateTime.Now
                    });
                }

                _logger.LogError("Failed to register user: {UserId} - No rows affected", dto.UserId);
                return StatusCode(500, new { message = "Không thể đăng ký. Vui lòng thử lại sau." });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Duplicate key error
                _logger.LogWarning(ex, "Duplicate key error for UserId: {UserId}", dto.UserId);
                return Conflict(new { message = "UserId hoặc Email đã tồn tại" });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during registration for UserId: {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Lỗi cơ sở dữ liệu. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for UserId: {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Đã xảy ra lỗi không mong muốn" });
            }
            finally
            {
                if (con?.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
                con?.Dispose();
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.MatKhau))
            {
                return BadRequest(new { message = "UserId và mật khẩu không được để trống" });
            }

            SqlConnection? con = null;
            SqlDataReader? reader = null;

            try
            {
                con = new SqlConnection(_connStr);
                await con.OpenAsync();

                using var cmd = new SqlCommand("sp_GetUserById", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;
                cmd.Parameters.AddWithValue("@UserId", dto.UserId);

                reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var userId = reader["UserId"]?.ToString();
                    var hoTen = reader["HoTen"]?.ToString();
                    var email = reader["Email"]?.ToString();
                    var role = reader["Role"]?.ToString() ?? "User";
                    var hashedPassword = reader["MatKhau"]?.ToString();

                    // Đóng reader trước khi xử lý tiếp
                    await reader.CloseAsync();
                    reader.Dispose();

                    // 2. Kiểm tra null
                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(hashedPassword))
                    {
                        _logger.LogWarning("Invalid user data for UserId: {UserId}", dto.UserId);
                        return Unauthorized(new { message = "Sai UserId hoặc mật khẩu" });
                    }

                    // 3. Verify password
                    var userForVerify = new User { UserId = userId };
                    var result = _passwordHasher.VerifyHashedPassword(
                        userForVerify,
                        hashedPassword,
                        dto.MatKhau
                    );

                    if (result == PasswordVerificationResult.Success)
                    {
                        // 4. Tạo JWT token
                        var token = GenerateJwtToken(userId, role);

                        _logger.LogInformation("User logged in successfully: {UserId}", userId);

                        return Ok(new
                        {
                            message = "Đăng nhập thành công",
                            token,
                            expiresIn = 3600, // 1 giờ (tính bằng giây)
                            user = new
                            {
                                UserId = userId,
                                HoTen = hoTen,
                                Email = email,
                                Role = role
                            }
                        });
                    }
                    else if (result == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        // 5. Password cần rehash (thuật toán được nâng cấp)
                        _logger.LogInformation("Password needs rehash for UserId: {UserId}", userId);

                        var token = GenerateJwtToken(userId, role);

                        return Ok(new
                        {
                            message = "Đăng nhập thành công",
                            token,
                            expiresIn = 3600,
                            user = new
                            {
                                UserId = userId,
                                HoTen = hoTen,
                                Email = email,
                                Role = role
                            }
                        });
                    }

                    // Failed verification
                    _logger.LogWarning("Failed login attempt for UserId: {UserId}", dto.UserId);
                    return Unauthorized(new { message = "Sai UserId hoặc mật khẩu" });
                }

                // User not found
                _logger.LogWarning("Login attempt for non-existent UserId: {UserId}", dto.UserId);
                return Unauthorized(new { message = "Sai UserId hoặc mật khẩu" });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during login for UserId: {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Lỗi cơ sở dữ liệu. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for UserId: {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Đã xảy ra lỗi không mong muốn" });
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.CloseAsync();
                    reader.Dispose();
                }

                if (con?.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
                con?.Dispose();
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // 1. Validation đầu vào
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            // 2. Validate độ mạnh của mật khẩu MỚI
            var passwordValidation = ValidatePassword(dto.MatKhauMoi);
            if (!passwordValidation.IsValid)
            {
                return BadRequest(new { message = passwordValidation.ErrorMessage });
            }

            SqlConnection? con = null;
            SqlDataReader? reader = null;

            try
            {
                con = new SqlConnection(_connStr);
                await con.OpenAsync();

                // A. Lấy thông tin người dùng và Hash mật khẩu cũ từ DB
                using var cmdGet = new SqlCommand("sp_GetUserById", con);
                cmdGet.CommandType = CommandType.StoredProcedure;
                cmdGet.Parameters.AddWithValue("@UserId", dto.UserId);

                reader = await cmdGet.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var userId = reader["UserId"]?.ToString();
                    var hashedPassword = reader["MatKhau"]?.ToString();

                    await reader.CloseAsync(); // Đóng reader
                    reader.Dispose();

                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(hashedPassword))
                    {
                        return Unauthorized(new { message = "Người dùng không tồn tại hoặc dữ liệu không hợp lệ" });
                    }

                    // B. Verify mật khẩu CŨ
                    var userForVerify = new User { UserId = userId };
                    var verifyResult = _passwordHasher.VerifyHashedPassword(
                        userForVerify,
                        hashedPassword,
                        dto.MatKhauCu // Mật khẩu cũ
                    );

                    // Kiểm tra kết quả Verify
                    if (verifyResult != PasswordVerificationResult.Success &&
                        verifyResult != PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        _logger.LogWarning("Change password failed: Incorrect old password for UserId: {UserId}", dto.UserId);
                        return Unauthorized(new { message = "Mật khẩu cũ không đúng" });
                    }

                    // C. Hash mật khẩu MỚI
                    var newHashedPassword = _passwordHasher.HashPassword(userForVerify, dto.MatKhauMoi);

                    // D. Cập nhật mật khẩu mới vào database
                    var rowsAffected = await UpdatePasswordHashAsync(con, userId, newHashedPassword);

                    if (rowsAffected > 0)
                    {
                        _logger.LogInformation("Password changed successfully for UserId: {UserId}", userId);
                        return Ok(new { message = "Đổi mật khẩu thành công" });
                    }

                    _logger.LogError("Failed to update password hash for UserId: {UserId}", userId);
                    return StatusCode(500, new { message = "Không thể cập nhật mật khẩu. Vui lòng thử lại." });
                }

                // Người dùng không tìm thấy
                return NotFound(new { message = "Người dùng không tồn tại" });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during password change for UserId: {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Lỗi cơ sở dữ liệu. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password change for UserId: {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Đã xảy ra lỗi không mong muốn" });
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.CloseAsync();
                    reader.Dispose();
                }

                if (con?.State == ConnectionState.Open)
                {
                    await con.CloseAsync();
                }
                con?.Dispose();
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            using var con = new SqlConnection(_connStr);
            await con.OpenAsync();

            var cmd = new SqlCommand("SELECT UserId FROM Users WHERE Email = @Email", con);
            cmd.Parameters.AddWithValue("@Email", dto.Email);

            var userId = (string?)await cmd.ExecuteScalarAsync();

            if (string.IsNullOrEmpty(userId))
                return Ok(new { message = "Nếu email tồn tại, hệ thống đã xử lý." });

            // Tạo reset token
            var token = GenerateResetPasswordToken(userId);

            // Test nội bộ: log ra màn hình console
            _logger.LogInformation("RESET TOKEN: {Token}", token);

            // Trả token thẳng về để test cho nhanh
            return Ok(new
            {
                message = "Token reset đã được tạo (test nội bộ).",
                token = token
            });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
                }, out _);

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var flag = principal.FindFirst("reset_password")?.Value;

                if (flag != "true")
                    return BadRequest(new { message = "Token không hợp lệ" });

                if (userId == null)
                    return BadRequest(new { message = "Token không chứa UserId" });

                // Hash mật khẩu mới
                var userObj = new User { UserId = userId };
                var newHash = _passwordHasher.HashPassword(userObj, dto.NewPassword);

                using var con = new SqlConnection(_connStr);
                await con.OpenAsync();

                var cmd = new SqlCommand("sp_UpdateUserPassword", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@MatKhau", newHash);

                var rows = await cmd.ExecuteNonQueryAsync();

                if (rows > 0)
                    return Ok(new { message = "Đặt lại mật khẩu thành công" });

                return StatusCode(500, new { message = "Không thể cập nhật mật khẩu" });
            }
            catch
            {
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn" });
            }
        }


        #region Helper Methods

        private string GenerateJwtToken(string userId, string role)
        {
            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key không được cấu hình");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role),
                new Claim("role", role), // Custom claim
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Đổi sang UTC và tăng lên 1 giờ
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateResetPasswordToken(string userId)
        {
            var jwtKey = _config["Jwt:Key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("reset_password", "true"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        /// <summary>
        /// Kiểm tra UserId đã tồn tại chưa
        /// </summary>
        private async Task<bool> UserExistsAsync(SqlConnection con, string userId)
        {
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE UserId = @UserId", con);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        /// <summary>
        /// Kiểm tra Email đã tồn tại chưa
        /// </summary>
        private async Task<bool> EmailExistsAsync(SqlConnection con, string email)
        {
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", con);
            cmd.Parameters.AddWithValue("@Email", email);

            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        /// <summary>
        /// Validate độ mạnh của password
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Mật khẩu không được để trống");
            }

            if (password.Length < 8)
            {
                return (false, "Mật khẩu phải có ít nhất 8 ký tự");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Mật khẩu phải có ít nhất 1 chữ cái viết hoa");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Mật khẩu phải có ít nhất 1 chữ cái viết thường");
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "Mật khẩu phải có ít nhất 1 chữ số");
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
            {
                return (false, "Mật khẩu phải có ít nhất 1 ký tự đặc biệt");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Regex pattern cho email
                var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate số điện thoại Việt Nam (10-11 số, bắt đầu bằng 0)
        /// </summary>
        private bool IsValidVietnamesePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Loại bỏ khoảng trắng, dấu gạch ngang, dấu ngoặc
            phone = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Kiểm tra format: 0xxxxxxxxx (10-11 số)
            return Regex.IsMatch(phone, @"^0[0-9]{9,10}$");
        }

        /// <summary>
        /// Cập nhật Hash mật khẩu mới vào database sử dụng Stored Procedure
        /// </summary>
        private async Task<int> UpdatePasswordHashAsync(SqlConnection con, string userId, string newHash)
        {
            // Gọi Stored Procedure sp_UpdateUserPassword
            using var cmd = new SqlCommand("sp_UpdateUserPassword", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@MatKhau", newHash);

            // ExecuteNonQueryAsync trả về số dòng bị ảnh hưởng
            return await cmd.ExecuteNonQueryAsync();
        }

        #endregion
    }
}