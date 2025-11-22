USE CinemaBookingDbb;
GO

DECLARE @StaffRoleId INT;
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
END;

-- Danh sách các username cần gán quyền Staff
DECLARE @Usernames TABLE (Username NVARCHAR(100));
INSERT INTO @Usernames VALUES 
    (N'staff1'),
    (N'staff2'); -- Thêm các username khác vào đây

DECLARE @CurrentUsername NVARCHAR(100);
DECLARE @CurrentUserId INT;

DECLARE user_cursor CURSOR FOR
SELECT Username FROM @Usernames;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @CurrentUsername;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @CurrentUserId = UserId 
    FROM dbo.Users 
    WHERE Username = @CurrentUsername OR NormalizedUsername = UPPER(@CurrentUsername);

    IF @CurrentUserId IS NULL
    BEGIN
        PRINT 'Không tìm thấy user với username: ' + @CurrentUsername;
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @CurrentUserId AND RoleId = @StaffRoleId)
        BEGIN
            PRINT 'User ' + @CurrentUsername + ' đã có role Staff rồi.';
        END
        ELSE
        BEGIN
            INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
            VALUES (@CurrentUserId, @StaffRoleId, GETUTCDATE());
            
            PRINT 'Đã gán role Staff thành công cho user: ' + @CurrentUsername + ' (ID: ' + CAST(@CurrentUserId AS NVARCHAR(10)) + ')';
        END;
    END;

    FETCH NEXT FROM user_cursor INTO @CurrentUsername;
END;

CLOSE user_cursor;
DEALLOCATE user_cursor;

PRINT 'Lưu ý: Các user cần đăng xuất và đăng nhập lại để token mới có role Staff.';

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

