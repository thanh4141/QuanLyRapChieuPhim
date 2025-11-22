-- Script để gán role Admin cho user
-- Sử dụng: Thay đổi username hoặc email trong WHERE clause

USE CinemaBookingDbb;
GO

-- Tìm UserId và RoleId
DECLARE @UserId INT;
DECLARE @AdminRoleId INT;

-- Tìm user theo username (thay 'nguyen van dung' bằng username của bạn)
SELECT @UserId = UserId 
FROM dbo.Users 
WHERE Username = N'nguyen van dung' OR NormalizedUsername = N'NGUYEN VAN DUNG';

-- Hoặc tìm theo email (thay 'email-cua-ban@example.com' bằng email của bạn)
-- SELECT @UserId = UserId 
-- FROM dbo.Users 
-- WHERE Email = N'email-cua-ban@example.com' OR NormalizedEmail = N'EMAIL-CUA-BAN@EXAMPLE.COM';

-- Tìm Admin role
SELECT @AdminRoleId = RoleId 
FROM dbo.Roles 
WHERE NormalizedRoleName = N'ADMIN';

-- Kiểm tra và gán role
IF @UserId IS NULL
BEGIN
    PRINT 'Không tìm thấy user. Vui lòng kiểm tra lại username/email.';
END
ELSE IF @AdminRoleId IS NULL
BEGIN
    PRINT 'Không tìm thấy role Admin. Vui lòng chạy script tạo roles trước.';
END
ELSE
BEGIN
    -- Kiểm tra xem user đã có role Admin chưa
    IF EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
    BEGIN
        PRINT 'User đã có role Admin rồi.';
    END
    ELSE
    BEGIN
        -- Gán role Admin
        INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
        VALUES (@UserId, @AdminRoleId, GETUTCDATE());
        
        PRINT 'Đã gán role Admin thành công cho user ID: ' + CAST(@UserId AS NVARCHAR(10));
        PRINT 'Lưu ý: User cần đăng xuất và đăng nhập lại để token mới có role Admin.';
    END
END
GO

