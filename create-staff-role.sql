USE CinemaBookingDbb;
GO

-- Tạo role Staff nếu chưa có
DECLARE @StaffRoleId INT;
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF')
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description, CreatedAt)
    VALUES (N'Staff', N'STAFF', N'Nhân viên bán vé', GETUTCDATE());
    SET @StaffRoleId = SCOPE_IDENTITY();
    PRINT 'Đã tạo role Staff.';
END
ELSE
BEGIN
    SELECT @StaffRoleId = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF';
    PRINT 'Role Staff đã tồn tại.';
END;
GO

-- Kiểm tra lại role Staff
SELECT 
    RoleId, 
    RoleName, 
    NormalizedRoleName, 
    Description
FROM dbo.Roles 
WHERE NormalizedRoleName = N'STAFF';
GO

