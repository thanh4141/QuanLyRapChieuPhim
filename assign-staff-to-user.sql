USE CinemaBookingDbb;
GO

DECLARE @Username NVARCHAR(100) = N'staff1'; -- Thay đổi username ở đây
DECLARE @UserId INT;
DECLARE @StaffRoleId INT;

SELECT @UserId = UserId 
FROM dbo.Users 
WHERE Username = @Username OR NormalizedUsername = UPPER(@Username);

SELECT @StaffRoleId = RoleId 
FROM dbo.Roles 
WHERE NormalizedRoleName = N'STAFF' AND IsDeleted = 0;

-- Tạo role Staff nếu chưa có
IF @StaffRoleId IS NULL
BEGIN
    PRINT 'Không tìm thấy role Staff.';
    PRINT 'Đang tạo role Staff...';
    
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description, CreatedAt)
    VALUES (N'Staff', N'STAFF', N'Nhân viên bán vé', GETUTCDATE());
    SET @StaffRoleId = SCOPE_IDENTITY();
    PRINT 'Đã tạo role Staff.';
END

IF @UserId IS NULL
BEGIN
    PRINT 'Không tìm thấy user với username: ' + @Username;
    PRINT 'Vui lòng kiểm tra lại username.';
END
ELSE IF @StaffRoleId IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @StaffRoleId)
    BEGIN
        PRINT 'User ' + @Username + ' đã có role Staff rồi.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
        VALUES (@UserId, @StaffRoleId, GETUTCDATE());
        
        PRINT 'Đã gán role Staff thành công cho user: ' + @Username + ' (ID: ' + CAST(@UserId AS NVARCHAR(10)) + ')';
        PRINT 'Lưu ý: User cần đăng xuất và đăng nhập lại để token mới có role Staff.';
    END
END
GO

-- Kiểm tra lại các user có role Staff
PRINT 'Danh sách các user hiện có role Staff:';
SELECT 
    u.UserId, 
    u.Username, 
    u.Email, 
    r.RoleName
FROM dbo.Users u
JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
JOIN dbo.Roles r ON ur.RoleId = r.RoleId
WHERE r.NormalizedRoleName = N'STAFF';
GO

