-- Script gán quyền Admin cho tài khoản
-- Sử dụng: Thay đổi username trong WHERE clause

USE CinemaBookingDbb;
GO

-- Tìm UserId và RoleId
DECLARE @UserId INT;
DECLARE @AdminRoleId INT;
DECLARE @Username NVARCHAR(100) = N'admin'; -- Thay đổi username ở đây

-- Tìm user theo username
SELECT @UserId = UserId 
FROM dbo.Users 
WHERE Username = @Username OR NormalizedUsername = UPPER(@Username);

-- Tìm Admin role
SELECT @AdminRoleId = RoleId 
FROM dbo.Roles 
WHERE NormalizedRoleName = N'ADMIN' AND IsDeleted = 0;

-- Kiểm tra và gán role
IF @UserId IS NULL
BEGIN
    PRINT 'Không tìm thấy user với username: ' + @Username;
    PRINT 'Vui lòng kiểm tra lại username.';
END
ELSE IF @AdminRoleId IS NULL
BEGIN
    PRINT 'Không tìm thấy role Admin.';
    PRINT 'Đang tạo role Admin...';
    
    -- Tạo role Admin nếu chưa có
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description, CreatedAt)
    VALUES (N'Admin', N'ADMIN', N'Quản trị hệ thống', GETUTCDATE());
    SET @AdminRoleId = SCOPE_IDENTITY();
    PRINT 'Đã tạo role Admin.';
END

IF @UserId IS NOT NULL AND @AdminRoleId IS NOT NULL
BEGIN
    -- Kiểm tra xem user đã có role Admin chưa
    IF EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
    BEGIN
        PRINT 'User "' + @Username + '" đã có role Admin rồi.';
    END
    ELSE
    BEGIN
        -- Gán role Admin
        INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
        VALUES (@UserId, @AdminRoleId, GETUTCDATE());
        
        PRINT '========================================';
        PRINT 'Đã gán role Admin thành công!';
        PRINT 'Username: ' + @Username;
        PRINT 'User ID: ' + CAST(@UserId AS NVARCHAR(10));
        PRINT 'Role ID: ' + CAST(@AdminRoleId AS NVARCHAR(10));
        PRINT '';
        PRINT 'Lưu ý: User cần đăng xuất và đăng nhập lại';
        PRINT 'để token mới có role Admin.';
        PRINT '========================================';
    END
END
GO

-- Xem thông tin user sau khi gán
SELECT 
    u.UserId,
    u.Username,
    u.Email,
    u.FullName,
    r.RoleName,
    r.NormalizedRoleName
FROM dbo.Users u
INNER JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
INNER JOIN dbo.Roles r ON ur.RoleId = r.RoleId
WHERE u.Username = N'admin' OR u.NormalizedUsername = N'ADMIN'
ORDER BY u.Username, r.RoleName;
GO

