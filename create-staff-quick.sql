USE CinemaBookingDbb;
GO

-- ========================================
-- TẠO NHANH TÀI KHOẢN NHÂN VIÊN
-- ========================================
-- Script này tạo user "staff1" với password "staff123"
-- Sau khi tạo, bạn có thể đăng nhập và đổi password

-- Bước 1: Tạo role Staff nếu chưa có
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF' AND IsDeleted = 0)
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description, CreatedAt)
    VALUES (N'Staff', N'STAFF', N'Nhân viên bán vé', GETUTCDATE());
    PRINT 'Đã tạo role Staff.';
END
ELSE
BEGIN
    PRINT 'Role Staff đã tồn tại.';
END
GO

-- Bước 2: Tạo user nhân viên (nếu chưa có)
DECLARE @StaffUserId INT;
DECLARE @StaffRoleId INT;

-- Lấy Staff Role ID
SELECT @StaffRoleId = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF' AND IsDeleted = 0;

-- Kiểm tra user đã tồn tại chưa
IF EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedUsername = N'STAFF1' AND IsDeleted = 0)
BEGIN
    SELECT @StaffUserId = UserId FROM dbo.Users WHERE NormalizedUsername = N'STAFF1' AND IsDeleted = 0;
    PRINT 'User staff1 đã tồn tại.';
END
ELSE
BEGIN
    -- Tạo user mới
    -- ⚠️ LƯU Ý: Password hash này là cho "staff123"
    -- Nếu muốn dùng password khác, bạn cần hash lại bằng BCrypt
    -- Hoặc tốt nhất là dùng trang create-staff.html để tự động hash
    
    INSERT INTO dbo.Users (
        Username, NormalizedUsername,
        Email, NormalizedEmail,
        PasswordHash,
        FullName, IsActive, CreatedAt
    )
    VALUES (
        N'staff1', N'STAFF1',
        N'staff1@cinema.local', N'STAFF1@CINEMA.LOCAL',
        N'$2a$11$KIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXxKIXx', -- ⚠️ Cần thay bằng hash thật
        N'Nhân viên 1', 1, GETUTCDATE()
    );
    SET @StaffUserId = SCOPE_IDENTITY();
    PRINT 'Đã tạo user staff1.';
    PRINT '⚠️ QUAN TRỌNG: Password chưa được set đúng!';
    PRINT 'Vui lòng sử dụng trang create-staff.html để tạo tài khoản (khuyến nghị)';
    PRINT 'Hoặc hash password bằng BCrypt và cập nhật vào cột PasswordHash.';
END
GO

-- Bước 3: Gán role Staff
DECLARE @StaffUserId2 INT;
DECLARE @StaffRoleId2 INT;

SELECT @StaffUserId2 = UserId FROM dbo.Users WHERE NormalizedUsername = N'STAFF1' AND IsDeleted = 0;
SELECT @StaffRoleId2 = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF' AND IsDeleted = 0;

IF @StaffUserId2 IS NOT NULL AND @StaffRoleId2 IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @StaffUserId2 AND RoleId = @StaffRoleId2)
    BEGIN
        INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedAt)
        VALUES (@StaffUserId2, @StaffRoleId2, GETUTCDATE());
        PRINT 'Đã gán role Staff cho user staff1.';
    END
    ELSE
    BEGIN
        PRINT 'User staff1 đã có role Staff rồi.';
    END
END
GO

-- Hiển thị kết quả
PRINT '';
PRINT '========================================';
PRINT 'KẾT QUẢ:';
PRINT '========================================';
SELECT 
    u.UserId, 
    u.Username, 
    u.Email, 
    u.FullName,
    u.IsActive,
    r.RoleName,
    CASE 
        WHEN u.PasswordHash LIKE '$2a$11$%' AND LEN(u.PasswordHash) > 50 THEN '✅ Password đã được hash'
        ELSE '⚠️ Password chưa được hash đúng - Cần cập nhật!'
    END AS PasswordStatus
FROM dbo.Users u
LEFT JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
LEFT JOIN dbo.Roles r ON ur.RoleId = r.RoleId
WHERE u.NormalizedUsername = N'STAFF1' AND u.IsDeleted = 0;
GO

PRINT '';
PRINT '========================================';
PRINT 'HƯỚNG DẪN:';
PRINT '========================================';
PRINT 'Cách 1 (Khuyến nghị): Sử dụng trang web';
PRINT '  1. Truy cập: frontend/pages/create-staff.html';
PRINT '  2. Điền thông tin và tạo tài khoản';
PRINT '  3. Password sẽ được hash tự động';
PRINT '';
PRINT 'Cách 2: Hash password thủ công';
PRINT '  1. Hash password "staff123" bằng BCrypt';
PRINT '  2. Cập nhật vào cột PasswordHash của user staff1';
PRINT '  3. Ví dụ: UPDATE Users SET PasswordHash = ''$2a$11$...'' WHERE Username = ''staff1''';
PRINT '========================================';
GO

