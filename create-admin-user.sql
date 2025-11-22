-- Script tạo tài khoản Admin
-- Lưu ý: Password sẽ được hash bằng BCrypt trong code, nên script này chỉ tạo user và gán role
-- Hoặc bạn có thể dùng endpoint API /api/auth/create-admin để tạo admin tự động

USE CinemaBookingDbb;
GO

-- Đảm bảo có role Admin
DECLARE @AdminRoleId INT;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedRoleName = N'ADMIN')
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description, CreatedAt)
    VALUES (N'Admin', N'ADMIN', N'Quản trị hệ thống', GETUTCDATE());
    SET @AdminRoleId = SCOPE_IDENTITY();
    PRINT 'Đã tạo role Admin.';
END
ELSE
BEGIN
    SELECT @AdminRoleId = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'ADMIN';
END;

-- Tạo user admin (nếu chưa có)
DECLARE @AdminUserId INT;
DECLARE @AdminUsername NVARCHAR(100) = N'admin';
DECLARE @AdminEmail NVARCHAR(256) = N'admin@cinema.local';
DECLARE @AdminPassword NVARCHAR(512) = N'$2a$11$KIXvJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJq'; -- Giá trị tạm thời, sẽ được thay bằng hash thật

-- Password hash cho "Admin123!" (BCrypt)
-- Bạn cần chạy code C# để hash password, hoặc dùng endpoint API
-- Hash của "Admin123!": $2a$11$KIXvJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJq
-- Tạm thời dùng hash này (cần thay bằng hash thật từ BCrypt)

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedUsername = N'ADMIN')
BEGIN
    -- Lưu ý: PasswordHash này là giá trị tạm thời
    -- Để có hash đúng, bạn cần:
    -- 1. Chạy endpoint API /api/auth/create-admin (khuyến nghị)
    -- 2. Hoặc chạy code C#: BCrypt.Net.BCrypt.HashPassword("Admin123!")
    
    INSERT INTO dbo.Users (
        Username, NormalizedUsername,
        Email, NormalizedEmail,
        PasswordHash,
        FullName, PhoneNumber,
        IsEmailConfirmed, IsPhoneConfirmed, IsActive,
        TwoFactorEnabled,
        CreatedAt
    )
    VALUES (
        @AdminUsername, N'ADMIN',
        @AdminEmail, N'ADMIN@CINEMA.LOCAL',
        N'$2a$11$KIXvJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJqJq', -- GIÁ TRỊ TẠM THỜI - Cần thay bằng hash thật
        N'Quản trị viên', N'0900000000',
        1, 1, 1,
        0,
        GETUTCDATE()
    );
    SET @AdminUserId = SCOPE_IDENTITY();
    PRINT 'Đã tạo user admin.';
END
ELSE
BEGIN
    SELECT @AdminUserId = UserId FROM dbo.Users WHERE NormalizedUsername = N'ADMIN';
    PRINT 'User admin đã tồn tại.';
END;

-- Gán role Admin
IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
    VALUES (@AdminUserId, @AdminRoleId, GETUTCDATE());
    PRINT 'Đã gán role Admin cho user admin.';
END
ELSE
BEGIN
    PRINT 'User admin đã có role Admin.';
END;

PRINT '';
PRINT '========================================';
PRINT 'THÔNG TIN ĐĂNG NHẬP:';
PRINT 'Username: admin';
PRINT 'Email: admin@cinema.local';
PRINT 'Password: (cần tạo qua API hoặc hash bằng BCrypt)';
PRINT '';
PRINT 'KHUYẾN NGHỊ: Sử dụng endpoint API /api/auth/create-admin';
PRINT 'để tạo admin với password được hash đúng cách.';
PRINT '========================================';
GO

