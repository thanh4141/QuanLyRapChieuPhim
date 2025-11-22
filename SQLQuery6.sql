------------------------------------------------------------
-- CHỌN ĐÚNG DATABASE
------------------------------------------------------------
USE CinemaBookingDbb;
GO

------------------------------------------------------------
-- 1. VAI TRÒ (Admin / Staff / Customer)
------------------------------------------------------------
DECLARE @AdminRoleId INT, @StaffRoleId INT, @CustomerRoleId INT;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedRoleName = N'ADMIN')
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description)
    VALUES (N'Admin', N'ADMIN', N'Quản trị hệ thống');
    SET @AdminRoleId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @AdminRoleId = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'ADMIN';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF')
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description)
    VALUES (N'Staff', N'STAFF', N'Nhân viên bán vé');
    SET @StaffRoleId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @StaffRoleId = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'STAFF';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedRoleName = N'CUSTOMER')
BEGIN
    INSERT INTO dbo.Roles (RoleName, NormalizedRoleName, Description)
    VALUES (N'Customer', N'CUSTOMER', N'Khách hàng đặt vé online');
    SET @CustomerRoleId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @CustomerRoleId = RoleId FROM dbo.Roles WHERE NormalizedRoleName = N'CUSTOMER';
END;
GO

------------------------------------------------------------
-- 2. USERS MẪU: admin, staff1, customer1
-- Lưu ý: PasswordHash là chuỗi giả – khi chạy hệ thống thực, bạn hash lại.
------------------------------------------------------------
DECLARE @AdminUserId INT, @StaffUserId INT, @CustomerUserId INT;

-- ADMIN
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedUsername = N'ADMIN')
BEGIN
    INSERT INTO dbo.Users (
        Username, NormalizedUsername,
        Email, NormalizedEmail,
        PasswordHash,
        FullName, PhoneNumber,
        IsEmailConfirmed, IsPhoneConfirmed, IsActive,
        TwoFactorEnabled
    )
    VALUES (
        N'admin', N'ADMIN',
        N'admin@cinema.local', N'ADMIN@CINEMA.LOCAL',
        N'ADMIN_HASHED_PASSWORD',
        N'Quản trị hệ thống', N'0900000001',
        1, 1, 1,
        0
    );
    SET @AdminUserId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @AdminUserId = UserId FROM dbo.Users WHERE NormalizedUsername = N'ADMIN';
END;

-- STAFF
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedUsername = N'STAFF1')
BEGIN
    INSERT INTO dbo.Users (
        Username, NormalizedUsername,
        Email, NormalizedEmail,
        PasswordHash,
        FullName, PhoneNumber,
        IsEmailConfirmed, IsPhoneConfirmed, IsActive,
        TwoFactorEnabled
    )
    VALUES (
        N'staff1', N'STAFF1',
        N'staff1@cinema.local', N'STAFF1@CINEMA.LOCAL',
        N'STAFF1_HASHED_PASSWORD',
        N'Nhân viên bán vé 1', N'0900000002',
        1, 1, 1,
        0
    );
    SET @StaffUserId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @StaffUserId = UserId FROM dbo.Users WHERE NormalizedUsername = N'STAFF1';
END;

-- CUSTOMER
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE NormalizedUsername = N'CUSTOMER1')
BEGIN
    INSERT INTO dbo.Users (
        Username, NormalizedUsername,
        Email, NormalizedEmail,
        PasswordHash,
        FullName, PhoneNumber,
        IsEmailConfirmed, IsPhoneConfirmed, IsActive,
        TwoFactorEnabled
    )
    VALUES (
        N'customer1', N'CUSTOMER1',
        N'customer1@cinema.local', N'CUSTOMER1@CINEMA.LOCAL',
        N'CUSTOMER1_HASHED_PASSWORD',
        N'Khách hàng 1', N'0900000003',
        1, 0, 1,
        0
    );
    SET @CustomerUserId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @CustomerUserId = UserId FROM dbo.Users WHERE NormalizedUsername = N'CUSTOMER1';
END;
GO

------------------------------------------------------------
-- 3. GÁN ROLE CHO USER
------------------------------------------------------------
-- Gán vai trò Admin cho tài khoản Admin
IF NOT EXISTS (
    SELECT 1 FROM dbo.UserRoles
    WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId
)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES (@AdminUserId, @AdminRoleId);
END;

-- Gán vai trò Staff cho tài khoản Staff
IF NOT EXISTS (
    SELECT 1 FROM dbo.UserRoles
    WHERE UserId = @StaffUserId AND RoleId = @StaffRoleId
)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES (@StaffUserId, @StaffRoleId);
END;

-- Gán vai trò Customer cho tài khoản Customer
IF NOT EXISTS (
    SELECT 1 FROM dbo.UserRoles
    WHERE UserId = @CustomerUserId AND RoleId = @CustomerRoleId
)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES (@CustomerUserId, @CustomerRoleId);
END;
GO

------------------------------------------------------------
-- 4. PERMISSIONS MẪU & GÁN CHO ROLE
------------------------------------------------------------
-- Thêm vài quyền cơ bản
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'Movie.View')
    INSERT INTO dbo.Permissions (PermissionKey, Name, Description)
    VALUES (N'Movie.View', N'Xem danh sách phim', N'Cho phép xem phim');

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'Movie.Manage')
    INSERT INTO dbo.Permissions (PermissionKey, Name, Description)
    VALUES (N'Movie.Manage', N'Quản lý phim', N'Tạo, sửa, xoá phim');

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'Showtime.Manage')
    INSERT INTO dbo.Permissions (PermissionKey, Name, Description)
    VALUES (N'Showtime.Manage', N'Quản lý suất chiếu', N'Tạo, sửa, xoá suất chiếu');

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'Booking.Create')
    INSERT INTO dbo.Permissions (PermissionKey, Name, Description)
    VALUES (N'Booking.Create', N'Đặt vé', N'Khách hàng tạo đơn đặt vé');

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'Report.View')
    INSERT INTO dbo.Permissions (PermissionKey, Name, Description)
    VALUES (N'Report.View', N'Xem báo cáo', N'Xem doanh thu, suất chiếu đông khách');
GO

-- Gán quyền cho Admin (đầy đủ)
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @AdminRoleId, p.PermissionId, SYSUTCDATETIME()
FROM dbo.Permissions p
WHERE p.PermissionKey IN (N'Movie.View', N'Movie.Manage', N'Showtime.Manage', N'Booking.Create', N'Report.View')
  AND NOT EXISTS (
        SELECT 1 FROM dbo.RolePermissions rp
        WHERE rp.RoleId = @AdminRoleId AND rp.PermissionId = p.PermissionId
  );

-- Gán quyền cho Staff
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @StaffRoleId, p.PermissionId, SYSUTCDATETIME()
FROM dbo.Permissions p
WHERE p.PermissionKey IN (N'Movie.View', N'Showtime.Manage', N'Report.View')
  AND NOT EXISTS (
        SELECT 1 FROM dbo.RolePermissions rp
        WHERE rp.RoleId = @StaffRoleId AND rp.PermissionId = p.PermissionId
  );

-- Gán quyền cho Customer
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @CustomerRoleId, p.PermissionId, SYSUTCDATETIME()
FROM dbo.Permissions p
WHERE p.PermissionKey IN (N'Movie.View', N'Booking.Create')
  AND NOT EXISTS (
        SELECT 1 FROM dbo.RolePermissions rp
        WHERE rp.RoleId = @CustomerRoleId AND rp.PermissionId = p.PermissionId
  );
GO

------------------------------------------------------------
-- 5. GENRES (THỂ LOẠI PHIM)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Genres WHERE GenreName = N'Hành động')
    INSERT INTO dbo.Genres (GenreName, Description) VALUES (N'Hành động', N'Phim hành động');

IF NOT EXISTS (SELECT 1 FROM dbo.Genres WHERE GenreName = N'Hài')
    INSERT INTO dbo.Genres (GenreName, Description) VALUES (N'Hài', N'Phim hài');

IF NOT EXISTS (SELECT 1 FROM dbo.Genres WHERE GenreName = N'Tâm lý')
    INSERT INTO dbo.Genres (GenreName, Description) VALUES (N'Tâm lý', N'Phim tâm lý');

IF NOT EXISTS (SELECT 1 FROM dbo.Genres WHERE GenreName = N'Khoa học viễn tưởng')
    INSERT INTO dbo.Genres (GenreName, Description) VALUES (N'Khoa học viễn tưởng', N'Sci-fi');

IF NOT EXISTS (SELECT 1 FROM dbo.Genres WHERE GenreName = N'Hoạt hình')
    INSERT INTO dbo.Genres (GenreName, Description) VALUES (N'Hoạt hình', N'Phim hoạt hình');
GO

------------------------------------------------------------
-- 6. MOVIES (PHIM MẪU)
------------------------------------------------------------
DECLARE @MovieEndgameId INT, @MovieConanId INT;

IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE Title = N'Avengers: Endgame')
BEGIN
    INSERT INTO dbo.Movies (
        Title, OriginalTitle, Description,
        DurationMinutes, AgeRating, ReleaseDate,
        Country, Director, Cast,
        PosterUrl, TrailerUrl, ImdbRating
    )
    VALUES (
        N'Avengers: Endgame',
        N'Avengers: Endgame',
        N'Biệt đội siêu anh hùng đối đầu Thanos.',
        181, N'C13', '2019-04-26',
        N'Mỹ', N'Anthony Russo, Joe Russo',
        N'Robert Downey Jr., Chris Evans, Chris Hemsworth',
        N'/images/movies/endgame.jpg',
        N'https://youtube.com/watch?v=TcMBFSGVi1c',
        8.4
    );
    SET @MovieEndgameId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @MovieEndgameId = MovieId FROM dbo.Movies WHERE Title = N'Avengers: Endgame';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Movies WHERE Title = N'Detective Conan: The Scarlet Bullet')
BEGIN
    INSERT INTO dbo.Movies (
        Title, OriginalTitle, Description,
        DurationMinutes, AgeRating, ReleaseDate,
        Country, Director, Cast,
        PosterUrl, TrailerUrl, ImdbRating
    )
    VALUES (
        N'Detective Conan: The Scarlet Bullet',
        N'Me itantei Konan Hiiro no dangan',
        N'Conan phá án vụ khủng bố tại giải thể thao thế giới.',
        110, N'C13', '2021-04-16',
        N'Nhật Bản', N'Tomoka Nagaoka',
        N'Minami Takayama, Wakana Yamazaki',
        N'/images/movies/conan_scarlet_bullet.jpg',
        N'https://youtube.com/watch?v=TsxP6e0EMkI',
        6.4
    );
    SET @MovieConanId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @MovieConanId = MovieId FROM dbo.Movies WHERE Title = N'Detective Conan: The Scarlet Bullet';
END;
GO

------------------------------------------------------------
-- 7. GÁN THỂ LOẠI CHO PHIM (MovieGenres)
------------------------------------------------------------
DECLARE @GenreActionId INT, @GenreSciFiId INT, @GenreAnimeId INT;

SELECT @GenreActionId = GenreId FROM dbo.Genres WHERE GenreName = N'Hành động';
SELECT @GenreSciFiId = GenreId FROM dbo.Genres WHERE GenreName = N'Khoa học viễn tưởng';
SELECT @GenreAnimeId  = GenreId FROM dbo.Genres WHERE GenreName = N'Hoạt hình';

IF @MovieEndgameId IS NOT NULL AND @GenreActionId IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieEndgameId AND GenreId = @GenreActionId)
BEGIN
    INSERT INTO dbo.MovieGenres (MovieId, GenreId)
    VALUES (@MovieEndgameId, @GenreActionId);
END;

IF @MovieEndgameId IS NOT NULL AND @GenreSciFiId IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieEndgameId AND GenreId = @GenreSciFiId)
BEGIN
    INSERT INTO dbo.MovieGenres (MovieId, GenreId)
    VALUES (@MovieEndgameId, @GenreSciFiId);
END;

IF @MovieConanId IS NOT NULL AND @GenreAnimeId IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM dbo.MovieGenres WHERE MovieId = @MovieConanId AND GenreId = @GenreAnimeId)
BEGIN
    INSERT INTO dbo.MovieGenres (MovieId, GenreId)
    VALUES (@MovieConanId, @GenreAnimeId);
END;
GO

------------------------------------------------------------
-- 8. AUDITORIUMS (PHÒNG CHIẾU)
------------------------------------------------------------
DECLARE @Auditorium1Id INT, @Auditorium2Id INT;

IF NOT EXISTS (SELECT 1 FROM dbo.Auditoriums WHERE Name = N'Phòng 1')
BEGIN
    INSERT INTO dbo.Auditoriums (Name, LocationDescription, Capacity)
    VALUES (N'Phòng 1', N'Tầng 3 - Khu A', 100);
    SET @Auditorium1Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Auditorium1Id = AuditoriumId FROM dbo.Auditoriums WHERE Name = N'Phòng 1';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Auditoriums WHERE Name = N'Phòng 2')
BEGIN
    INSERT INTO dbo.Auditoriums (Name, LocationDescription, Capacity)
    VALUES (N'Phòng 2', N'Tầng 3 - Khu B', 80);
    SET @Auditorium2Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @Auditorium2Id = AuditoriumId FROM dbo.Auditoriums WHERE Name = N'Phòng 2';
END;
GO

------------------------------------------------------------
-- 9. SEATTYPES (LOẠI GHẾ)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.SeatTypes WHERE SeatTypeName = N'Standard')
    INSERT INTO dbo.SeatTypes (SeatTypeName, Description, PriceMultiplier)
    VALUES (N'Standard', N'Ghế thường', 1.0);

IF NOT EXISTS (SELECT 1 FROM dbo.SeatTypes WHERE SeatTypeName = N'VIP')
    INSERT INTO dbo.SeatTypes (SeatTypeName, Description, PriceMultiplier)
    VALUES (N'VIP', N'Ghế VIP', 1.5);

IF NOT EXISTS (SELECT 1 FROM dbo.SeatTypes WHERE SeatTypeName = N'Couple')
    INSERT INTO dbo.SeatTypes (SeatTypeName, Description, PriceMultiplier)
    VALUES (N'Couple', N'Ghế đôi', 2.0);
GO

DECLARE @StandardSeatTypeId INT;
SELECT @StandardSeatTypeId = SeatTypeId FROM dbo.SeatTypes WHERE SeatTypeName = N'Standard';

------------------------------------------------------------
-- 10. SEATS (MỘT ÍT GHẾ MẪU CHO PHÒNG 1)
-- Tạo 3 hàng A, B, C - mỗi hàng 10 ghế
------------------------------------------------------------
IF @Auditorium1Id IS NOT NULL AND @StandardSeatTypeId IS NOT NULL
BEGIN
    DECLARE @row CHAR(1), @i INT;
    SET @row = 'A';
    WHILE @row <= 'C'
    BEGIN
        SET @i = 1;
        WHILE @i <= 10
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM dbo.Seats
                WHERE AuditoriumId = @Auditorium1Id
                  AND RowLabel = @row
                  AND SeatNumber = @i
            )
            BEGIN
                INSERT INTO dbo.Seats (AuditoriumId, RowLabel, SeatNumber, SeatTypeId)
                VALUES (@Auditorium1Id, @row, @i, @StandardSeatTypeId);
            END;
            SET @i = @i + 1;
        END;

        SET @row = CHAR(ASCII(@row) + 1);
    END;
END;
GO

------------------------------------------------------------
-- 11. SHOWTIMES (SUẤT CHIẾU MẪU)
------------------------------------------------------------
DECLARE @Showtime1Id INT, @Showtime2Id INT;

IF @MovieEndgameId IS NOT NULL AND @Auditorium1Id IS NOT NULL
    AND NOT EXISTS (
        SELECT 1 FROM dbo.Showtimes
        WHERE MovieId = @MovieEndgameId
          AND AuditoriumId = @Auditorium1Id
          AND StartTime = '2025-11-23T13:00:00'
    )
BEGIN
    INSERT INTO dbo.Showtimes (
        MovieId, AuditoriumId,
        StartTime, EndTime,
        BaseTicketPrice, Language, Format
    )
    VALUES (
        @MovieEndgameId, @Auditorium1Id,
        '2025-11-23T13:00:00', '2025-11-23T16:00:00',
        80000, N'Phụ đề', N'2D'
    );
    SET @Showtime1Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT TOP (1) @Showtime1Id = ShowtimeId
    FROM dbo.Showtimes
    WHERE MovieId = @MovieEndgameId AND AuditoriumId = @Auditorium1Id
    ORDER BY StartTime;
END;

IF @MovieConanId IS NOT NULL AND @Auditorium2Id IS NOT NULL
    AND NOT EXISTS (
        SELECT 1 FROM dbo.Showtimes
        WHERE MovieId = @MovieConanId
          AND AuditoriumId = @Auditorium2Id
          AND StartTime = '2025-11-23T19:00:00'
    )
BEGIN
    INSERT INTO dbo.Showtimes (
        MovieId, AuditoriumId,
        StartTime, EndTime,
        BaseTicketPrice, Language, Format
    )
    VALUES (
        @MovieConanId, @Auditorium2Id,
        '2025-11-23T19:00:00', '2025-11-23T21:00:00',
        70000, N'Thuyết minh', N'2D'
    );
    SET @Showtime2Id = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT TOP (1) @Showtime2Id = ShowtimeId
    FROM dbo.Showtimes
    WHERE MovieId = @MovieConanId AND AuditoriumId = @Auditorium2Id
    ORDER BY StartTime;
END;
GO

------------------------------------------------------------
-- 12. 1 ĐƠN ĐẶT VÉ MẪU + HOÁ ĐƠN + THANH TOÁN CHO CUSTOMER1
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Reservations WHERE ReservationCode = N'RSV0001')
BEGIN
    DECLARE @SeatA1Id INT, @SeatA2Id INT;
    SELECT @SeatA1Id = SeatId FROM dbo.Seats
    WHERE AuditoriumId = @Auditorium1Id AND RowLabel = N'A' AND SeatNumber = 1;

    SELECT @SeatA2Id = SeatId FROM dbo.Seats
    WHERE AuditoriumId = @Auditorium1Id AND RowLabel = N'A' AND SeatNumber = 2;

    IF @CustomerUserId IS NOT NULL AND @Showtime1Id IS NOT NULL
       AND @SeatA1Id IS NOT NULL AND @SeatA2Id IS NOT NULL
    BEGIN
        BEGIN TRAN;

        DECLARE @ReservationId INT, @InvoiceId INT;
        DECLARE @TotalAmount DECIMAL(12,2) = 160000;

        INSERT INTO dbo.Reservations (
            UserId, ShowtimeId,
            ReservationCode, Status,
            ReservedAt, TotalAmount
        )
        VALUES (
            @CustomerUserId, @Showtime1Id,
            N'RSV0001', N'Confirmed',
            SYSUTCDATETIME(), @TotalAmount
        );

        SET @ReservationId = SCOPE_IDENTITY();

        INSERT INTO dbo.Invoices (
            InvoiceNumber, ReservationId, UserId,
            IssuedAt, SubTotal, DiscountAmount, TaxAmount, TotalAmount
        )
        VALUES (
            N'INV0001', @ReservationId, @CustomerUserId,
            SYSUTCDATETIME(), @TotalAmount, 0, 0, @TotalAmount
        );

        SET @InvoiceId = SCOPE_IDENTITY();

        INSERT INTO dbo.Tickets (
            ReservationId, ShowtimeId, SeatId,
            SeatPrice, Status,
            QrCodeData, InvoiceId
        )
        VALUES
        (@ReservationId, @Showtime1Id, @SeatA1Id, 80000, N'Paid', N'TICKET-RSV0001-A1', @InvoiceId),
        (@ReservationId, @Showtime1Id, @SeatA2Id, 80000, N'Paid', N'TICKET-RSV0001-A2', @InvoiceId);

        INSERT INTO dbo.Payments (
            InvoiceId, UserId,
            PaymentMethod, PaymentStatus,
            Amount, PaidAt,
            TransactionCode
        )
        VALUES (
            @InvoiceId, @CustomerUserId,
            N'Cash', N'Success',
            @TotalAmount, SYSUTCDATETIME(),
            N'TRAN0001'
        );

        COMMIT TRAN;
    END;
END;
GO
