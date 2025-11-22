------------------------------------------------------------
-- XÁC THỰC & PHÂN QUYỀN (AUTH & RBAC)
------------------------------------------------------------
-- Người dùng hệ thống
CREATE TABLE dbo.Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(100) NOT NULL,
    NormalizedUsername NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    PasswordHash    NVARCHAR(512) NOT NULL,
    FullName        NVARCHAR(200) NULL,
    PhoneNumber     NVARCHAR(20) NULL,
    IsEmailConfirmed    BIT NOT NULL CONSTRAINT DF_Users_IsEmailConfirmed DEFAULT(0),
    IsPhoneConfirmed    BIT NOT NULL CONSTRAINT DF_Users_IsPhoneConfirmed DEFAULT(0),
    IsActive        BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),

    -- Xác thực hai yếu tố (2FA)
    TwoFactorEnabled    BIT NOT NULL CONSTRAINT DF_Users_TwoFactorEnabled DEFAULT(0),
    TwoFactorSecretKey  NVARCHAR(256) NULL,

    -- Kiểm toán & xóa mềm
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT(0)
);
GO

CREATE UNIQUE INDEX UX_Users_Username ON dbo.Users (NormalizedUsername) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users (NormalizedEmail) WHERE IsDeleted = 0;
GO

-- Vai trò (Admin / Staff / Customer)
CREATE TABLE dbo.Roles (
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(100) NOT NULL,
    NormalizedRoleName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   INT NULL,
    UpdatedAt   DATETIME2(0) NULL,
    UpdatedBy   INT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT(0)
);
GO

CREATE UNIQUE INDEX UX_Roles_RoleName ON dbo.Roles (NormalizedRoleName) WHERE IsDeleted = 0;
GO

-- Gán nhiều vai trò cho 1 user
CREATE TABLE dbo.UserRoles (
    UserRoleId  INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL,
    RoleId      INT NOT NULL,
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_UserRoles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   INT NULL,
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId),
    CONSTRAINT UQ_UserRoles_User_Role UNIQUE (UserId, RoleId)
);
GO

-- Quyền theo màn hình & hành động (Xem, Tạo, Cập nhật, Xóa, Duyệt, Xuất, ...)
CREATE TABLE dbo.Permissions (
    PermissionId    INT IDENTITY(1,1) PRIMARY KEY,
    PermissionKey   NVARCHAR(150) NOT NULL,    -- ví dụ: "Movie.View", "Movie.Create"
    Name            NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(500) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Permissions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Permissions_IsDeleted DEFAULT(0)
);
GO

CREATE UNIQUE INDEX UX_Permissions_Key ON dbo.Permissions (PermissionKey) WHERE IsDeleted = 0;
GO

-- Gán quyền cho vai trò (RBAC)
CREATE TABLE dbo.RolePermissions (
    RolePermissionId INT IDENTITY(1,1) PRIMARY KEY,
    RoleId          INT NOT NULL,
    PermissionId    INT NOT NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_RolePermissions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(PermissionId),
    CONSTRAINT UQ_RolePermissions_Role_Permission UNIQUE (RoleId, PermissionId)
);
GO

-- Token làm mới cho JWT / OAuth2
CREATE TABLE dbo.UserRefreshTokens (
    RefreshTokenId  INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    RefreshToken    NVARCHAR(500) NOT NULL,
    ExpiresAt       DATETIME2(0) NOT NULL,
    RevokedAt       DATETIME2(0) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_UserRefreshTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    CONSTRAINT FK_UserRefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

-- Token đặt lại mật khẩu
CREATE TABLE dbo.PasswordResetTokens (
    PasswordResetTokenId INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    Token           NVARCHAR(200) NOT NULL,
    ExpiresAt       DATETIME2(0) NOT NULL,
    UsedAt          DATETIME2(0) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT UQ_PasswordResetTokens_User_Token UNIQUE (UserId, Token)
);
GO

------------------------------------------------------------
-- DANH MỤC PHIM
------------------------------------------------------------
CREATE TABLE dbo.Genres (
    GenreId     INT IDENTITY(1,1) PRIMARY KEY,
    GenreName   NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Genres_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   INT NULL,
    UpdatedAt   DATETIME2(0) NULL,
    UpdatedBy   INT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Genres_IsDeleted DEFAULT(0)
);
GO

CREATE UNIQUE INDEX UX_Genres_Name ON dbo.Genres(GenreName) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Movies (
    MovieId         INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(255) NOT NULL,
    OriginalTitle   NVARCHAR(255) NULL,
    Description     NVARCHAR(MAX) NULL,
    DurationMinutes INT NOT NULL,                 -- thời lượng
    AgeRating       NVARCHAR(20) NULL,            -- P (Mọi lứa tuổi), C13, C16, C18,...
    ReleaseDate     DATE NULL,
    Country         NVARCHAR(100) NULL,
    Director        NVARCHAR(200) NULL,
    Cast            NVARCHAR(1000) NULL,
    PosterUrl       NVARCHAR(500) NULL,
    TrailerUrl      NVARCHAR(500) NULL,
    ImdbRating      DECIMAL(3,1) NULL,           -- tuỳ chọn

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Movies_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Movies_IsDeleted DEFAULT(0)
);
GO

CREATE INDEX IX_Movies_Title ON dbo.Movies (Title) WHERE IsDeleted = 0;
GO

-- Nhiều thể loại cho 1 phim
CREATE TABLE dbo.MovieGenres (
    MovieGenreId INT IDENTITY(1,1) PRIMARY KEY,
    MovieId      INT NOT NULL,
    GenreId      INT NOT NULL,
    CONSTRAINT FK_MovieGenres_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(MovieId),
    CONSTRAINT FK_MovieGenres_Genres FOREIGN KEY (GenreId) REFERENCES dbo.Genres(GenreId),
    CONSTRAINT UQ_MovieGenres_Movie_Genre UNIQUE (MovieId, GenreId)
);
GO

------------------------------------------------------------
-- PHÒNG CHIẾU & SƠ ĐỒ GHẾ
------------------------------------------------------------
CREATE TABLE dbo.Auditoriums (
    AuditoriumId    INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(100) NOT NULL,  -- Phòng 1, Phòng 2, VIP 1,...
    LocationDescription NVARCHAR(200) NULL,  -- Tầng 3, khu A
    Capacity        INT NOT NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Auditoriums_IsActive DEFAULT(1),

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Auditoriums_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Auditoriums_IsDeleted DEFAULT(0)
);
GO

CREATE UNIQUE INDEX UX_Auditoriums_Name ON dbo.Auditoriums(Name) WHERE IsDeleted = 0;
GO

-- Loại ghế: Thường, VIP, Đôi...
CREATE TABLE dbo.SeatTypes (
    SeatTypeId      INT IDENTITY(1,1) PRIMARY KEY,
    SeatTypeName    NVARCHAR(50) NOT NULL,
    Description     NVARCHAR(200) NULL,
    PriceMultiplier DECIMAL(5,2) NOT NULL CONSTRAINT DF_SeatTypes_PriceMultiplier DEFAULT(1.0),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_SeatTypes_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_SeatTypes_IsDeleted DEFAULT(0)
);
GO

CREATE UNIQUE INDEX UX_SeatTypes_Name ON dbo.SeatTypes(SeatTypeName) WHERE IsDeleted = 0;
GO

-- Danh sách ghế trong từng phòng
CREATE TABLE dbo.Seats (
    SeatId          INT IDENTITY(1,1) PRIMARY KEY,
    AuditoriumId    INT NOT NULL,
    RowLabel        NVARCHAR(10) NOT NULL,    -- A, B, C...
    SeatNumber      INT NOT NULL,             -- 1,2,3...
    SeatTypeId      INT NOT NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Seats_IsActive DEFAULT(1),

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Seats_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Seats_IsDeleted DEFAULT(0),

    CONSTRAINT FK_Seats_Auditoriums FOREIGN KEY (AuditoriumId) REFERENCES dbo.Auditoriums(AuditoriumId),
    CONSTRAINT FK_Seats_SeatTypes FOREIGN KEY (SeatTypeId) REFERENCES dbo.SeatTypes(SeatTypeId),
    CONSTRAINT UQ_Seats_Auditorium_Row_Seat UNIQUE (AuditoriumId, RowLabel, SeatNumber)
);
GO

CREATE INDEX IX_Seats_AuditoriumId ON dbo.Seats(AuditoriumId) WHERE IsDeleted = 0;
GO

------------------------------------------------------------
-- LỊCH CHIẾU / SUẤT CHIẾU
------------------------------------------------------------
CREATE TABLE dbo.Showtimes (
    ShowtimeId      INT IDENTITY(1,1) PRIMARY KEY,
    MovieId         INT NOT NULL,
    AuditoriumId    INT NOT NULL,
    StartTime       DATETIME2(0) NOT NULL,
    EndTime         DATETIME2(0) NOT NULL,
    BaseTicketPrice DECIMAL(12,2) NOT NULL,  -- giá cơ bản, giá thực tế = base * multiplier loại ghế
    Language        NVARCHAR(50) NULL,       -- lồng tiếng, phụ đề
    Format          NVARCHAR(50) NULL,       -- 2D, 3D, IMAX,...
    IsActive        BIT NOT NULL CONSTRAINT DF_Showtimes_IsActive DEFAULT(1),

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Showtimes_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Showtimes_IsDeleted DEFAULT(0),

    CONSTRAINT FK_Showtimes_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(MovieId),
    CONSTRAINT FK_Showtimes_Auditoriums FOREIGN KEY (AuditoriumId) REFERENCES dbo.Auditoriums(AuditoriumId)
);
GO

CREATE INDEX IX_Showtimes_Movie_StartTime ON dbo.Showtimes(MovieId, StartTime) WHERE IsDeleted = 0;
CREATE INDEX IX_Showtimes_Auditorium_StartTime ON dbo.Showtimes(AuditoriumId, StartTime) WHERE IsDeleted = 0;
GO

------------------------------------------------------------
-- ĐẶT VÉ / RESERVATIONS / TICKETS
------------------------------------------------------------
-- Đặt chỗ (booking) cấp độ đơn hàng/suất
CREATE TABLE dbo.Reservations (
    ReservationId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,                 -- khách đặt
    ShowtimeId      INT NOT NULL,
    ReservationCode NVARCHAR(50) NOT NULL,        -- mã đặt chỗ hiển thị
    Status          NVARCHAR(20) NOT NULL,        -- Chờ xử lý, Đã xác nhận, Đã hủy, Hết hạn
    ReservedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_Reservations_ReservedAt DEFAULT (SYSUTCDATETIME()),
    ExpiresAt       DATETIME2(0) NULL,            -- giữ chỗ trong X phút
    ConfirmedAt     DATETIME2(0) NULL,
    CancelledAt     DATETIME2(0) NULL,
    CancelReason    NVARCHAR(500) NULL,

    TotalAmount     DECIMAL(12,2) NOT NULL CONSTRAINT DF_Reservations_TotalAmount DEFAULT(0),
    Note            NVARCHAR(500) NULL,

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Reservations_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Reservations_IsDeleted DEFAULT(0),

    CONSTRAINT FK_Reservations_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Reservations_Showtimes FOREIGN KEY (ShowtimeId) REFERENCES dbo.Showtimes(ShowtimeId),
    CONSTRAINT UQ_Reservations_Code UNIQUE (ReservationCode),
    CONSTRAINT CK_Reservations_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Cancelled', N'Expired'))
);
GO

CREATE INDEX IX_Reservations_User ON dbo.Reservations(UserId, ReservedAt DESC) WHERE IsDeleted = 0;
CREATE INDEX IX_Reservations_Showtime ON dbo.Reservations(ShowtimeId, ReservedAt DESC) WHERE IsDeleted = 0;
GO

-- Vé chi tiết theo ghế
CREATE TABLE dbo.Tickets (
    TicketId        INT IDENTITY(1,1) PRIMARY KEY,
    ReservationId   INT NOT NULL,
    ShowtimeId      INT NOT NULL,
    SeatId          INT NOT NULL,
    SeatPrice       DECIMAL(12,2) NOT NULL,
    Status          NVARCHAR(20) NOT NULL,       -- Đã đặt, Đã thanh toán, Đã hủy, Đã check-in
    QrCodeData      NVARCHAR(255) NOT NULL,      -- chuỗi dùng tạo QR
    QrCodeImageUrl  NVARCHAR(500) NULL,          -- nếu có lưu ảnh QR
    CheckedInAt     DATETIME2(0) NULL,
    CheckedInBy     INT NULL,                    -- nhân viên check-in

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Tickets_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Tickets_IsDeleted DEFAULT(0),

    CONSTRAINT FK_Tickets_Reservations FOREIGN KEY (ReservationId) REFERENCES dbo.Reservations(ReservationId),
    CONSTRAINT FK_Tickets_Showtimes FOREIGN KEY (ShowtimeId) REFERENCES dbo.Showtimes(ShowtimeId),
    CONSTRAINT FK_Tickets_Seats FOREIGN KEY (SeatId) REFERENCES dbo.Seats(SeatId),
    CONSTRAINT FK_Tickets_CheckedInBy_Users FOREIGN KEY (CheckedInBy) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Tickets_Status CHECK (Status IN (N'Booked', N'Paid', N'Cancelled', N'CheckedIn'))
);
GO

-- Chống đặt trùng ghế: 1 ghế/1 suất chỉ có 1 vé đang hoạt động
ALTER TABLE dbo.Tickets
ADD CONSTRAINT UQ_Tickets_Showtime_Seat UNIQUE (ShowtimeId, SeatId);
GO

CREATE INDEX IX_Tickets_ReservationId ON dbo.Tickets(ReservationId);
CREATE INDEX IX_Tickets_ShowtimeId ON dbo.Tickets(ShowtimeId);
CREATE INDEX IX_Tickets_QrCodeData ON dbo.Tickets(QrCodeData);
GO

------------------------------------------------------------
-- HOÁ ĐƠN & THANH TOÁN
------------------------------------------------------------
CREATE TABLE dbo.Invoices (
    InvoiceId       INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber   NVARCHAR(50) NOT NULL,    -- hoá đơn in PDF
    ReservationId   INT NOT NULL,
    UserId          INT NOT NULL,            -- khách
    IssuedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Invoices_IssuedAt DEFAULT (SYSUTCDATETIME()),
    SubTotal        DECIMAL(12,2) NOT NULL,
    DiscountAmount  DECIMAL(12,2) NOT NULL CONSTRAINT DF_Invoices_DiscountAmount DEFAULT(0),
    TaxAmount       DECIMAL(12,2) NOT NULL CONSTRAINT DF_Invoices_TaxAmount DEFAULT(0),
    TotalAmount     DECIMAL(12,2) NOT NULL,
    Note            NVARCHAR(500) NULL,

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Invoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Invoices_IsDeleted DEFAULT(0),

    CONSTRAINT FK_Invoices_Reservations FOREIGN KEY (ReservationId) REFERENCES dbo.Reservations(ReservationId),
    CONSTRAINT FK_Invoices_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT UQ_Invoices_InvoiceNumber UNIQUE (InvoiceNumber)
);
GO

CREATE INDEX IX_Invoices_User_IssuedAt ON dbo.Invoices(UserId, IssuedAt DESC) WHERE IsDeleted = 0;
GO

-- Liên kết Ticket với Invoice (để báo cáo chi tiết)
ALTER TABLE dbo.Tickets
ADD InvoiceId INT NULL;
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId);
GO

-- Thanh toán
CREATE TABLE dbo.Payments (
    PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId       INT NOT NULL,
    UserId          INT NOT NULL,
    PaymentMethod   NVARCHAR(50) NOT NULL,  -- Tiền mặt, Thẻ, VNPay, Momo,...
    PaymentStatus   NVARCHAR(20) NOT NULL,  -- Chờ xử lý, Thành công, Thất bại, Đã hoàn tiền
    Amount          DECIMAL(12,2) NOT NULL,
    PaidAt          DATETIME2(0) NULL,
    TransactionCode NVARCHAR(100) NULL,     -- mã giao dịch cổng thanh toán
    GatewayResponse NVARCHAR(MAX) NULL,     -- raw JSON/response
    Note            NVARCHAR(500) NULL,

    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       INT NULL,
    UpdatedAt       DATETIME2(0) NULL,
    UpdatedBy       INT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Payments_IsDeleted DEFAULT(0),

    CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId),
    CONSTRAINT FK_Payments_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Payments_Status CHECK (PaymentStatus IN (N'Pending', N'Success', N'Failed', N'Refunded'))
);
GO

CREATE INDEX IX_Payments_InvoiceId ON dbo.Payments(InvoiceId);
CREATE INDEX IX_Payments_Status_PaidAt ON dbo.Payments(PaymentStatus, PaidAt);
GO

------------------------------------------------------------
-- BÁO CÁO & HỖ TRỢ TÌM KIẾM / LỌC / XUẤT
------------------------------------------------------------
-- Bảng log đơn giản (tuỳ bạn có dùng hay không, hỗ trợ audit/báo cáo)
CREATE TABLE dbo.AuditLogs (
    AuditLogId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NULL,
    ActionName      NVARCHAR(200) NOT NULL,      -- Tên hành động: "CreateMovie", "UpdateShowtime", "CheckInTicket",...
    EntityName      NVARCHAR(200) NULL,          -- Tên bảng: "Movies", "Showtimes", "Tickets"
    EntityId        NVARCHAR(100) NULL,
    Details         NVARCHAR(MAX) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    IPAddress       NVARCHAR(50) NULL,
    UserAgent       NVARCHAR(500) NULL,

    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

-- Một số index hỗ trợ báo cáo doanh thu, suất chiếu đông khách
CREATE INDEX IX_Tickets_Showtime_Status ON dbo.Tickets(ShowtimeId, Status) WHERE IsDeleted = 0;
CREATE INDEX IX_Reservations_Status_Showtime ON dbo.Reservations(Status, ShowtimeId) WHERE IsDeleted = 0;
CREATE INDEX IX_Payments_PaidAt_Status ON dbo.Payments(PaidAt, PaymentStatus) WHERE IsDeleted = 0;
GO
