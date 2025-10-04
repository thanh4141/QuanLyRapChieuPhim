CREATE DATABASE Cinema;
USE Cinema;

-- Các bảng
-- Bảng Phim
CREATE TABLE Movies (
    MaPhim VARCHAR(50) PRIMARY KEY,     
    TenPhim NVARCHAR(255) NOT NULL,            
    TheLoai NVARCHAR(100),              
    ThoiLuong INT NOT NULL,                   
    NgayKhoiChieu DATE,                      
    MoTa NVARCHAR(MAX)
);

-- Bảng Phòng chiếu
CREATE TABLE Auditoriums (
    MaPhong VARCHAR(50) PRIMARY KEY,  
    TenPhong NVARCHAR(100) NOT NULL,          
    SucChua INT NOT NULL                       
);

-- Bảng Ghế
CREATE TABLE Seats (
    MaGhe VARCHAR(50) PRIMARY KEY,    
    MaPhong VARCHAR(50) NOT NULL,                 
    SoGhe VARCHAR(10) NOT NULL,              
    LoaiGhe NVARCHAR(50),                    
    FOREIGN KEY (MaPhong) REFERENCES Auditoriums(MaPhong)
);

-- Bảng Suất chiếu
CREATE TABLE Showtimes (
    MaSuatChieu VARCHAR(50) PRIMARY KEY, 
    MaPhim VARCHAR(50) NOT NULL,                      
    MaPhong VARCHAR(50) NOT NULL,                       
    GioBatDau DATETIME NOT NULL,            
    GioKetThuc DATETIME NOT NULL,              
    FOREIGN KEY (MaPhim) REFERENCES Movies(MaPhim),
    FOREIGN KEY (MaPhong) REFERENCES Auditoriums(MaPhong)
);

-- Bảng Người dùng (Khách hàng & Admin)
CREATE TABLE Users (
    UserId VARCHAR(50) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    SoDienThoai VARCHAR(15),
    MatKhau VARCHAR(255) NOT NULL, -- hash mật khẩu
    Role NVARCHAR(50) DEFAULT 'Customer' -- Customer, Staff, Admin
);

-- Bảng Nhân viên
CREATE TABLE Staff (
    MaNV VARCHAR(50) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    ChucVu NVARCHAR(50), -- Thu ngân, Quản lý, Soát vé
    NgaySinh DATE,
    SoDienThoai VARCHAR(15)
);

-- Bảng Đặt chỗ
CREATE TABLE Reservations (
    MaDatCho VARCHAR(50) PRIMARY KEY,   
    MaSuatChieu VARCHAR(50) NOT NULL,                
    MaGhe VARCHAR(50) NOT NULL,                       
    UserId VARCHAR(50) NOT NULL,    
    ThoiGianDat DATETIME DEFAULT CURRENT_TIMESTAMP, 
    FOREIGN KEY (MaSuatChieu) REFERENCES Showtimes(MaSuatChieu),
    FOREIGN KEY (MaGhe) REFERENCES Seats(MaGhe),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Bảng Vé
CREATE TABLE Tickets (
    MaVe VARCHAR(50) PRIMARY KEY,      
    MaDatCho VARCHAR(50) NOT NULL,                   
    GiaVe DECIMAL(10,2) NOT NULL,            
    TrangThai VARCHAR(20) DEFAULT 'Valid',   
    FOREIGN KEY (MaDatCho) REFERENCES Reservations(MaDatCho)
);

-- Bảng Khuyến mãi
CREATE TABLE Promotions (
    MaKM VARCHAR(50) PRIMARY KEY,
    TenKM NVARCHAR(100),
    MoTa NVARCHAR(255),
    GiamGia DECIMAL(5,2), -- %
    NgayBatDau DATE,
    NgayKetThuc DATE
);

-- Bảng Hóa đơn
CREATE TABLE Invoices (
    MaHoaDon VARCHAR(50) PRIMARY KEY,               
    MaDatCho VARCHAR(50) NOT NULL,               
    TongTien DECIMAL(10,2) NOT NULL,      
    NgayTao DATETIME DEFAULT CURRENT_TIMESTAMP,
    MaNV VARCHAR(50),  -- nhân viên lập hóa đơn
    MaKM VARCHAR(50),  -- khuyến mãi (nếu có)
    FOREIGN KEY (MaDatCho) REFERENCES Reservations(MaDatCho),
    FOREIGN KEY (MaNV) REFERENCES Staff(MaNV),
    FOREIGN KEY (MaKM) REFERENCES Promotions(MaKM)
);

-- Bảng Thanh toán
CREATE TABLE Payments (
    MaThanhToan VARCHAR(50) PRIMARY KEY, 
    MaHoaDon VARCHAR(50) NOT NULL,                  
    SoTien DECIMAL(10,2) NOT NULL,           
    HinhThucTT NVARCHAR(50) NOT NULL,         
    NgayThanhToan DATETIME DEFAULT CURRENT_TIMESTAMP, 
    FOREIGN KEY (MaHoaDon) REFERENCES Invoices(MaHoaDon)
);

-- Bảng Sản phẩm (Đồ ăn/nước uống)
CREATE TABLE Products (
    MaSP VARCHAR(50) PRIMARY KEY,
    TenSP NVARCHAR(100),
    Gia DECIMAL(10,2),
    Loai NVARCHAR(50) -- Đồ ăn, Nước uống
);

-- Bảng Chi tiết hóa đơn (mua thêm sản phẩm)
CREATE TABLE InvoiceDetails (
    MaCTHD VARCHAR(50) PRIMARY KEY,
    MaHoaDon VARCHAR(50),
    MaSP VARCHAR(50),
    SoLuong INT,
    FOREIGN KEY (MaHoaDon) REFERENCES Invoices(MaHoaDon),
    FOREIGN KEY (MaSP) REFERENCES Products(MaSP)
);

--Store Proc Đặt vé
GO
CREATE PROCEDURE sp_BookTicket
    @UserId VARCHAR(50),
    @MaSuatChieu VARCHAR(50),
    @MaGhe VARCHAR(50),
    @GiaVe DECIMAL(10,2)
AS
BEGIN
    -- Kiểm tra ghế đã đặt chưa
    IF EXISTS (
        SELECT 1 FROM Reservations
        WHERE MaSuatChieu = @MaSuatChieu AND MaGhe = @MaGhe
    )
    BEGIN
        RAISERROR('Ghế đã được đặt!', 16, 1);
        RETURN;
    END

    -- Tạo mã đặt chỗ
    DECLARE @MaDatCho VARCHAR(50) = NEWID();
    INSERT INTO Reservations(MaDatCho, MaSuatChieu, MaGhe, UserId)
    VALUES(@MaDatCho, @MaSuatChieu, @MaGhe, @UserId);

    -- Tạo vé
    DECLARE @MaVe VARCHAR(50) = NEWID();
    INSERT INTO Tickets(MaVe, MaDatCho, GiaVe)
    VALUES(@MaVe, @MaDatCho, @GiaVe);

    -- Tạo hóa đơn
    DECLARE @MaHoaDon VARCHAR(50) = NEWID();
    INSERT INTO Invoices(MaHoaDon, MaDatCho, TongTien)
    VALUES(@MaHoaDon, @MaDatCho, @GiaVe);
END

--Store Proc Thanh toán
GO
CREATE PROCEDURE sp_MakePayment
    @MaHoaDon VARCHAR(50),
    @SoTien DECIMAL(10,2),
    @HinhThucTT NVARCHAR(50)
AS
BEGIN
    DECLARE @MaThanhToan VARCHAR(50) = NEWID();

    INSERT INTO Payments(MaThanhToan, MaHoaDon, SoTien, HinhThucTT)
    VALUES(@MaThanhToan, @MaHoaDon, @SoTien, @HinhThucTT);

    -- Nếu thanh toán đủ thì update trạng thái vé
    DECLARE @TongTien DECIMAL(10,2) =
        (SELECT TongTien FROM Invoices WHERE MaHoaDon = @MaHoaDon);

    DECLARE @DaThanhToan DECIMAL(10,2) =
        (SELECT SUM(SoTien) FROM Payments WHERE MaHoaDon = @MaHoaDon);

    IF @DaThanhToan >= @TongTien
    BEGIN
        UPDATE Tickets
        SET TrangThai = 'Paid'
        WHERE MaDatCho = (SELECT MaDatCho FROM Invoices WHERE MaHoaDon = @MaHoaDon);
    END
END

--Store Proc Huỷ vé
GO
CREATE PROCEDURE sp_CancelTicket
    @MaVe VARCHAR(50)
AS
BEGIN
    DECLARE @MaDatCho VARCHAR(50), @MaHoaDon VARCHAR(50);

    -- Lấy mã đặt chỗ
    SELECT @MaDatCho = MaDatCho FROM Tickets WHERE MaVe = @MaVe;

    IF @MaDatCho IS NULL
    BEGIN
        RAISERROR('Vé không tồn tại!', 16, 1);
        RETURN;
    END

    -- Lấy mã hóa đơn
    SELECT @MaHoaDon = MaHoaDon FROM Invoices WHERE MaDatCho = @MaDatCho;

    -- Xóa thanh toán (nếu có)
    DELETE FROM Payments WHERE MaHoaDon = @MaHoaDon;

    -- Xóa hóa đơn
    DELETE FROM Invoices WHERE MaHoaDon = @MaHoaDon;

    -- Xóa vé
    DELETE FROM Tickets WHERE MaVe = @MaVe;

    -- Xóa đặt chỗ
    DELETE FROM Reservations WHERE MaDatCho = @MaDatCho;
END

--Store Proc Thống kê báo cáo	
GO
CREATE PROCEDURE sp_ThongKeDoanhThu
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SELECT 
        CONVERT(DATE, NgayThanhToan) AS Ngay,
        SUM(SoTien) AS DoanhThu
    FROM Payments
    WHERE NgayThanhToan BETWEEN @FromDate AND @ToDate
    GROUP BY CONVERT(DATE, NgayThanhToan)
    ORDER BY Ngay;
END
-- Đăng ký
GO
CREATE PROCEDURE sp_RegisterUser
    @UserId NVARCHAR(50),
    @HoTen NVARCHAR(100),
    @Email NVARCHAR(100),
    @SoDienThoai NVARCHAR(15),
    @MatKhau NVARCHAR(255),   -- Mật khẩu HASHED
    @Role NVARCHAR(50) = 'Customer'
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId OR Email = @Email)
    BEGIN
        RAISERROR('UserId hoặc Email đã tồn tại', 16, 1);
        RETURN;
    END
    INSERT INTO Users (UserId, HoTen, Email, SoDienThoai, MatKhau, Role)
    VALUES (@UserId, @HoTen, @Email, @SoDienThoai, @MatKhau, @Role);

END
GO
-- Đăng nhập
CREATE PROCEDURE sp_LoginUser
    @UserId NVARCHAR(50)
AS
BEGIN
    SELECT UserId, HoTen, Email, Role, MatKhau
    FROM Users
    WHERE UserId = @UserId;
END
GO
--Lấy User theo ID
CREATE PROCEDURE sp_GetUserById
    @UserId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        UserId,
        HoTen,
        Email,
        SoDienThoai,
        MatKhau,
        Role
    FROM [User]
    WHERE UserId = @UserId;
END
GO