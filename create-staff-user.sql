USE CinemaBookingDbb;
GO

-- Tạo role Staff nếu chưa có
DECLARE @StaffRoleId INT;
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF' AND IsDeleted = 0)
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description, CreatedAt)
    VALUES (N'Staff', N'STAFF', N'Nhân viên bán vé', GETUTCDATE());
    SET @StaffRoleId = SCOPE_IDENTITY();
    PRINT 'Đã tạo role Staff.';
END
ELSE
BEGIN
    SELECT @StaffRoleId = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF' AND IsDeleted = 0;
    PRINT 'Role Staff đã tồn tại.';
END;
GO

-- Thông tin user nhân viên mới
DECLARE @StaffUsername NVARCHAR(100) = N'staff1';
DECLARE @StaffEmail NVARCHAR(256) = N'staff1@cinema.local';
DECLARE @StaffPassword NVARCHAR(512) = N'$2a$11$YourHashedPasswordHereExample123456789012345678901234567890'; -- THAY THẾ BẰNG PASSWORD HASH THẬT
DECLARE @StaffFullName NVARCHAR(200) = N'Nhân viên 1';
DECLARE @StaffUserId INT;

-- Kiểm tra và tạo user nhân viên nếu chưa có
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedUsername = UPPER(@StaffUsername) AND IsDeleted = 0)
BEGIN
    -- Tạo password hash (ví dụ: password = "staff123")
    -- Trong thực tế, bạn nên hash password bằng BCrypt
    -- Ví dụ hash cho "staff123": $2a$11$KIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXx
    
    INSERT INTO dbo.Users (
        Username, NormalizedUsername,
        Email, NormalizedEmail,
        PasswordHash,
        FullName, IsActive, CreatedAt
    )
    VALUES (
        @StaffUsername, UPPER(@StaffUsername),
        @StaffEmail, UPPER(@StaffEmail),
        @StaffPassword, -- THAY THẾ BẰNG PASSWORD HASH THẬT
        @StaffFullName, 1, GETUTCDATE()
    );
    SET @StaffUserId = SCOPE_IDENTITY();
    PRINT 'Đã tạo user nhân viên: ' + @StaffUsername;
END
ELSE
BEGIN
    SELECT @StaffUserId = UserId FROM dbo.Users WHERE NormalizedUsername = UPPER(@StaffUsername) AND IsDeleted = 0;
    PRINT 'User nhân viên ' + @StaffUsername + ' đã tồn tại.';
END;

-- Gán role Staff cho user nhân viên
IF @StaffUserId IS NOT NULL AND @StaffRoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @StaffUserId AND RoleId = @StaffRoleId)
    BEGIN
        INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
        VALUES (@StaffUserId, @StaffRoleId, GETUTCDATE());
        PRINT 'Đã gán role Staff cho user ' + @StaffUsername + '.';
    END
    ELSE
    BEGIN
        PRINT 'User ' + @StaffUsername + ' đã có role Staff rồi.';
    END;
END
ELSE
BEGIN
    PRINT 'Không thể gán role Staff. User ID hoặc Staff Role ID không hợp lệ.';
END;
GO

PRINT '';
PRINT '========================================';
PRINT 'Thông tin tài khoản nhân viên:';
PRINT 'Username: staff1';
PRINT 'Email: staff1@cinema.local';
PRINT 'Password: (cần hash bằng BCrypt)';
PRINT '';
PRINT 'Lưu ý: Bạn cần hash password bằng BCrypt và cập nhật vào cột PasswordHash.';
PRINT 'Hoặc sử dụng trang create-staff.html để tạo tài khoản.';
PRINT '========================================';
GO

-- Kiểm tra lại thông tin user nhân viên
SELECT 
    u.UserId, 
    u.Username, 
    u.Email, 
    u.FullName,
    u.IsActive,
    r.RoleName
FROM dbo.Users u
LEFT JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
LEFT JOIN dbo.Roles r ON ur.RoleId = r.RoleId
WHERE u.NormalizedUsername = UPPER(N'staff1') AND u.IsDeleted = 0;
GO

