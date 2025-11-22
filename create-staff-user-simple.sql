USE CinemaBookingDbb;
GO

-- ========================================
-- TẠO TÀI KHOẢN NHÂN VIÊN
-- ========================================
-- Script này sẽ tạo user nhân viên với password mặc định: "staff123"
-- Sau khi tạo, bạn nên đổi password ngay!

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
END;
GO

-- Thông tin user nhân viên
DECLARE @StaffUsername NVARCHAR(100) = N'staff1';
DECLARE @StaffEmail NVARCHAR(256) = N'staff1@cinema.local';
DECLARE @StaffFullName NVARCHAR(200) = N'Nhân viên 1';
DECLARE @StaffUserId INT;

-- Password hash cho "staff123" (đã hash bằng BCrypt)
-- Bạn có thể tạo hash mới tại: https://bcrypt-generator.com/
-- Hoặc sử dụng trang create-staff.html để tự động hash password
DECLARE @StaffPasswordHash NVARCHAR(512) = N'$2a$11$rKqXqXqXqXqXqXqXqXqXeXqXqXqXqXqXqXqXqXqXqXqXqXqXqXqXqX'; -- Thay bằng hash thật

-- Kiểm tra và tạo user
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedUsername = UPPER(@StaffUsername) AND IsDeleted = 0)
BEGIN
    -- LƯU Ý: Bạn cần thay @StaffPasswordHash bằng password hash thật
    -- Cách 1: Sử dụng trang create-staff.html (khuyến nghị)
    -- Cách 2: Hash password bằng BCrypt và thay vào đây
    
    INSERT INTO dbo.Users (
        Username, NormalizedUsername,
        Email, NormalizedEmail,
        PasswordHash,
        FullName, IsActive, CreatedAt
    )
    VALUES (
        @StaffUsername, UPPER(@StaffUsername),
        @StaffEmail, UPPER(@StaffEmail),
        @StaffPasswordHash, -- ⚠️ CẦN THAY BẰNG PASSWORD HASH THẬT
        @StaffFullName, 1, GETUTCDATE()
    );
    SET @StaffUserId = SCOPE_IDENTITY();
    PRINT 'Đã tạo user nhân viên: ' + @StaffUsername;
    PRINT '⚠️ LƯU Ý: Bạn cần cập nhật PasswordHash bằng hash thật!';
END
ELSE
BEGIN
    SELECT @StaffUserId = UserId FROM dbo.Users WHERE NormalizedUsername = UPPER(@StaffUsername) AND IsDeleted = 0;
    PRINT 'User nhân viên ' + @StaffUsername + ' đã tồn tại.';
END;
GO

-- Gán role Staff
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
GO

-- Hiển thị thông tin
PRINT '';
PRINT '========================================';
PRINT 'THÔNG TIN TÀI KHOẢN NHÂN VIÊN:';
PRINT 'Username: staff1';
PRINT 'Email: staff1@cinema.local';
PRINT '';
PRINT '⚠️ QUAN TRỌNG:';
PRINT 'Password chưa được set đúng. Bạn có 2 cách:';
PRINT '1. Sử dụng trang create-staff.html để tạo (khuyến nghị)';
PRINT '2. Hash password bằng BCrypt và cập nhật vào cột PasswordHash';
PRINT '========================================';
GO

-- Kiểm tra lại
SELECT 
    u.UserId, 
    u.Username, 
    u.Email, 
    u.FullName,
    u.IsActive,
    r.RoleName,
    CASE 
        WHEN u.PasswordHash LIKE '$2a$11$%' THEN 'Password đã được hash'
        ELSE '⚠️ Password chưa được hash đúng!'
    END AS PasswordStatus
FROM dbo.Users u
LEFT JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
LEFT JOIN dbo.Roles r ON ur.RoleId = r.RoleId
WHERE u.NormalizedUsername = UPPER(N'staff1') AND u.IsDeleted = 0;
GO

