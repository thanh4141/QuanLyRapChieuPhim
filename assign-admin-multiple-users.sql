-- Script gán quyền Admin cho nhiều tài khoản
-- Sử dụng: Thêm username vào danh sách

USE CinemaBookingDbb;
GO

-- Danh sách username cần gán quyền Admin
DECLARE @Usernames TABLE (Username NVARCHAR(100));
INSERT INTO @Usernames VALUES 
    (N'admin'),
    (N'dung1692004');
    -- Thêm username khác vào đây nếu cần

-- Tìm Admin role
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = RoleId 
FROM dbo.Roles 
WHERE NormalizedRoleName = N'ADMIN' AND IsDeleted = 0;

-- Tạo role Admin nếu chưa có
IF @AdminRoleId IS NULL
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description, CreatedAt)
    VALUES (N'Admin', N'ADMIN', N'Quản trị hệ thống', GETUTCDATE());
    SET @AdminRoleId = SCOPE_IDENTITY();
    PRINT 'Đã tạo role Admin.';
END

-- Gán role Admin cho từng user
DECLARE @Username NVARCHAR(100);
DECLARE @UserId INT;
DECLARE @Count INT = 0;

DECLARE user_cursor CURSOR FOR
SELECT Username FROM @Usernames;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @Username;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Tìm user
    SELECT @UserId = UserId 
    FROM dbo.Users 
    WHERE Username = @Username OR NormalizedUsername = UPPER(@Username);

    IF @UserId IS NULL
    BEGIN
        PRINT 'Không tìm thấy user: ' + @Username;
    END
    ELSE
    BEGIN
        -- Kiểm tra xem đã có role Admin chưa
        IF EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
        BEGIN
            PRINT 'User "' + @Username + '" đã có role Admin rồi.';
        END
        ELSE
        BEGIN
            -- Gán role Admin
            INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
            VALUES (@UserId, @AdminRoleId, GETUTCDATE());
            
            PRINT 'Đã gán role Admin cho user: ' + @Username;
            SET @Count = @Count + 1;
        END
    END

    FETCH NEXT FROM user_cursor INTO @Username;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;

PRINT '';
PRINT '========================================';
PRINT 'Hoàn thành! Đã gán role Admin cho ' + CAST(@Count AS NVARCHAR(10)) + ' user(s).';
PRINT '';
PRINT 'Lưu ý: Các user cần đăng xuất và đăng nhập lại';
PRINT 'để token mới có role Admin.';
PRINT '========================================';
GO

-- Xem danh sách tất cả user có role Admin
SELECT 
    u.UserId,
    u.Username,
    u.Email,
    u.FullName,
    r.RoleName,
    ur.CreatedAt AS 'Ngày gán role'
FROM dbo.Users u
INNER JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId
WHERE r.NormalizedRoleName = N'ADMIN' AND u.IsDeleted = 0
ORDER BY ur.CreatedAt DESC;
GO

