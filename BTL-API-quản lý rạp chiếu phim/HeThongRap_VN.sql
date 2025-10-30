/* ======================================================================
   HỆ THỐNG QUẢN LÝ RẠP CHIẾU PHIM - PHIÊN BẢN TIẾNG VIỆT
   SQL Server 2017+
   Bao gồm:
   - Tạo CSDL
   - Bảng lõi (tên bảng & cột tiếng Việt có dấu, dùng [] để bao)
   - RBAC (Vai trò / Quyền)
   - Chỉ mục
   - Trigger soft-delete + trigger audit
   - View báo cáo
   - Thủ tục (SP) quản trị (tên tiếng Việt)
   - Dữ liệu mẫu tiếng Việt
   Lưu ý: Có thể đồng tồn tại với bản tiếng Anh bằng CSDL khác.
   ====================================================================== */

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyRapPhim')
BEGIN
    CREATE DATABASE [QuanLyRapPhim];
END
GO

USE [QuanLyRapPhim];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ==============================
   1) BẢNG LÕI (TIẾNG VIỆT)
   ============================== */

-- Người dùng
IF OBJECT_ID(N'dbo.[NguờiDùng]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[NguờiDùng](
        [Id]               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TênĐăngNhập]      NVARCHAR(50)  NOT NULL UNIQUE,
        [Email]            NVARCHAR(100) NOT NULL UNIQUE,
        [BămMậtKhẩu]       NVARCHAR(255) NOT NULL,
        [Họ]               NVARCHAR(50)  NOT NULL,
        [Tên]              NVARCHAR(50)  NOT NULL,
        [SốĐiệnThoại]      NVARCHAR(20)  NULL,
        [VaiTròChuỗi]      NVARCHAR(20)  NOT NULL DEFAULT N'Khách',
        [KíchHoạt]         BIT NOT NULL DEFAULT 1,
        [XácThựcEmail]     BIT NOT NULL DEFAULT 0,
        [XácThực2Bước]     BIT NOT NULL DEFAULT 0,
        [TạoLúc]           DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]           DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]           INT NULL,
        [SửaBởi]           INT NULL,
        [ĐãXóa]            BIT NOT NULL DEFAULT 0,
        [XóaLúc]           DATETIME2 NULL,
        [XóaBởi]           INT NULL
    );
END
GO

-- Phim
IF OBJECT_ID(N'dbo.[Phim]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[Phim](
        [Id]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TênPhim]      NVARCHAR(200) NOT NULL,
        [MôTả]         NVARCHAR(MAX) NULL,
        [ThểLoại]      NVARCHAR(100) NOT NULL,
        [ThờiLượngPhút] INT NOT NULL,
        [PhânLoại]     NVARCHAR(10) NOT NULL,     -- P, C13, C16, C18...
        [NgàyPhátHành] DATE NOT NULL,
        [ĐạoDiễn]      NVARCHAR(100) NOT NULL,
        [DiễnViên]     NVARCHAR(500) NULL,
        [ẢnhPoster]    NVARCHAR(500) NULL,
        [Trailer]      NVARCHAR(500) NULL,
        [KíchHoạt]     BIT NOT NULL DEFAULT 1,
        [TạoLúc]       DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]       DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]       INT NULL,
        [SửaBởi]       INT NULL,
        [ĐãXóa]        BIT NOT NULL DEFAULT 0,
        [XóaLúc]       DATETIME2 NULL,
        [XóaBởi]       INT NULL
    );
END
GO

-- Phòng chiếu
IF OBJECT_ID(N'dbo.[PhòngChiếu]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[PhòngChiếu](
        [Id]        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TênPhòng]  NVARCHAR(100) NOT NULL,
        [SứcChứa]   INT NOT NULL,
        [LoạiMàn]   NVARCHAR(50) NOT NULL, -- 2D, 3D, IMAX, 4DX...
        [KíchHoạt]  BIT NOT NULL DEFAULT 1,
        [TạoLúc]    DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]    DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]    INT NULL,
        [SửaBởi]    INT NULL,
        [ĐãXóa]     BIT NOT NULL DEFAULT 0,
        [XóaLúc]    DATETIME2 NULL,
        [XóaBởi]    INT NULL
    );
END
GO

-- Ghế
IF OBJECT_ID(N'dbo.[Ghế]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[Ghế](
        [Id]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PhòngChiếuId]    INT NOT NULL,
        [Hàng]            NVARCHAR(5) NOT NULL,
        [Số]              INT NOT NULL,
        [LoạiGhế]         NVARCHAR(20) NOT NULL DEFAULT N'Thường', -- Thường, VIP...
        [HệSốGiá]         DECIMAL(3,2) NOT NULL DEFAULT 1.00,
        [KíchHoạt]        BIT NOT NULL DEFAULT 1,
        [TạoLúc]          DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]          DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]          INT NULL,
        [SửaBởi]          INT NULL,
        [ĐãXóa]           BIT NOT NULL DEFAULT 0,
        [XóaLúc]          DATETIME2 NULL,
        [XóaBởi]          INT NULL,
        CONSTRAINT [FK_Ghế_PhòngChiếu] FOREIGN KEY([PhòngChiếuId]) REFERENCES dbo.[PhòngChiếu]([Id]),
        CONSTRAINT [UQ_Ghế_Phòng_Hàng_Số] UNIQUE ([PhòngChiếuId],[Hàng],[Số])
    );
END
GO

-- Suất chiếu
IF OBJECT_ID(N'dbo.[SuấtChiếu]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[SuấtChiếu](
        [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PhimId]        INT NOT NULL,
        [PhòngChiếuId]  INT NOT NULL,
        [NgàyChiếu]     DATE NOT NULL,
        [GiờBắtĐầu]     TIME NOT NULL,
        [GiờKếtThúc]    TIME NOT NULL,
        [GiáCơBản]      DECIMAL(10,2) NOT NULL,
        [KíchHoạt]      BIT NOT NULL DEFAULT 1,
        [TạoLúc]        DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]        DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]        INT NULL,
        [SửaBởi]        INT NULL,
        [ĐãXóa]         BIT NOT NULL DEFAULT 0,
        [XóaLúc]        DATETIME2 NULL,
        [XóaBởi]        INT NULL,
        CONSTRAINT [FK_SuấtChiếu_Phim] FOREIGN KEY([PhimId]) REFERENCES dbo.[Phim]([Id]),
        CONSTRAINT [FK_SuấtChiếu_PhòngChiếu] FOREIGN KEY([PhòngChiếuId]) REFERENCES dbo.[PhòngChiếu]([Id])
    );
END
GO

-- Đặt chỗ
IF OBJECT_ID(N'dbo.[ĐặtChỗ]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[ĐặtChỗ](
        [Id]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [KháchId]         INT NOT NULL,
        [SuấtChiếuId]     INT NOT NULL,
        [MãĐặtChỗ]        NVARCHAR(20) NOT NULL UNIQUE,
        [TrạngThái]       NVARCHAR(20) NOT NULL DEFAULT N'Chờ', -- Chờ, ĐãXácNhận, ĐãHủy, HếtHạn
        [TổngTiền]        DECIMAL(10,2) NOT NULL,
        [NgàyĐặt]         DATETIME2 NOT NULL DEFAULT GETDATE(),
        [HếtHạn]          DATETIME2 NOT NULL,
        [GhiChú]          NVARCHAR(500) NULL,
        [TạoLúc]          DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]          DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]          INT NULL,
        [SửaBởi]          INT NULL,
        [ĐãXóa]           BIT NOT NULL DEFAULT 0,
        [XóaLúc]          DATETIME2 NULL,
        [XóaBởi]          INT NULL,
        CONSTRAINT [FK_ĐặtChỗ_NgườiDùng] FOREIGN KEY([KháchId]) REFERENCES dbo.[NguờiDùng]([Id]),
        CONSTRAINT [FK_ĐặtChỗ_SuấtChiếu] FOREIGN KEY([SuấtChiếuId]) REFERENCES dbo.[SuấtChiếu]([Id])
    );
END
GO

-- Vé
IF OBJECT_ID(N'dbo.[Vé]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[Vé](
        [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ĐặtChỗId]      INT NOT NULL,
        [GhếId]         INT NOT NULL,
        [MãVé]          NVARCHAR(20) NOT NULL UNIQUE,
        [MãQR]          NVARCHAR(500) NULL,
        [Giá]           DECIMAL(10,2) NOT NULL,
        [TrạngThái]     NVARCHAR(20) NOT NULL DEFAULT N'HoạtĐộng', -- HoạtĐộng, ĐãDùng, ĐãHủy
        [DùngLúc]       DATETIME2 NULL,
        [DùngBởi]       INT NULL,
        [TạoLúc]        DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]        DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]        INT NULL,
        [SửaBởi]        INT NULL,
        [ĐãXóa]         BIT NOT NULL DEFAULT 0,
        [XóaLúc]        DATETIME2 NULL,
        [XóaBởi]        INT NULL,
        CONSTRAINT [FK_Vé_ĐặtChỗ] FOREIGN KEY([ĐặtChỗId]) REFERENCES dbo.[ĐặtChỗ]([Id]),
        CONSTRAINT [FK_Vé_Ghế] FOREIGN KEY([GhếId]) REFERENCES dbo.[Ghế]([Id]),
        CONSTRAINT [FK_Vé_NgườiDùng_DùngBởi] FOREIGN KEY([DùngBởi]) REFERENCES dbo.[NguờiDùng]([Id])
    );
END
GO

-- Hóa đơn
IF OBJECT_ID(N'dbo.[HóaĐơn]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[HóaĐơn](
        [Id]             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ĐặtChỗId]       INT NOT NULL,
        [SốHóaĐơn]       NVARCHAR(20) NOT NULL UNIQUE,
        [TạmTính]        DECIMAL(10,2) NOT NULL,
        [TiềnThuế]       DECIMAL(10,2) NOT NULL DEFAULT 0,
        [TiềnGiảm]       DECIMAL(10,2) NOT NULL DEFAULT 0,
        [ThànhTiền]      DECIMAL(10,2) NOT NULL,
        [TrạngThái]      NVARCHAR(20) NOT NULL DEFAULT N'Chờ', -- Chờ, ĐãThanhToán, ĐãHủy, HoànTiền
        [NgàyLập]        DATETIME2 NOT NULL DEFAULT GETDATE(),
        [HạnThanhToán]   DATETIME2 NOT NULL,
        [NgàyThanhToán]  DATETIME2 NULL,
        [GhiChú]         NVARCHAR(500) NULL,
        [TạoLúc]         DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]         DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]         INT NULL,
        [SửaBởi]         INT NULL,
        [ĐãXóa]          BIT NOT NULL DEFAULT 0,
        [XóaLúc]         DATETIME2 NULL,
        [XóaBởi]         INT NULL,
        CONSTRAINT [FK_HóaĐơn_ĐặtChỗ] FOREIGN KEY([ĐặtChỗId]) REFERENCES dbo.[ĐặtChỗ]([Id])
    );
END
GO

-- Thanh toán
IF OBJECT_ID(N'dbo.[ThanhToán]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[ThanhToán](
        [Id]             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [HóaĐơnId]       INT NOT NULL,
        [PhươngThức]     NVARCHAR(20) NOT NULL, -- TiềnMặt, Thẻ, ChuyểnKhoản, VíĐiệnTử
        [SốTiền]         DECIMAL(10,2) NOT NULL,
        [MãGiaoDịch]     NVARCHAR(100) NULL,
        [TrạngThái]      NVARCHAR(20) NOT NULL DEFAULT N'Chờ', -- Chờ, ThànhCông, Lỗi, HoànTiền
        [NgàyThanhToán]  DATETIME2 NOT NULL DEFAULT GETDATE(),
        [XửLýBởi]        INT NULL,
        [GhiChú]         NVARCHAR(500) NULL,
        [TạoLúc]         DATETIME2 NOT NULL DEFAULT GETDATE(),
        [SửaLúc]         DATETIME2 NOT NULL DEFAULT GETDATE(),
        [TạoBởi]         INT NULL,
        [SửaBởi]         INT NULL,
        [ĐãXóa]          BIT NOT NULL DEFAULT 0,
        [XóaLúc]         DATETIME2 NULL,
        [XóaBởi]         INT NULL,
        CONSTRAINT [FK_ThanhToán_HóaĐơn] FOREIGN KEY([HóaĐơnId]) REFERENCES dbo.[HóaĐơn]([Id]),
        CONSTRAINT [FK_ThanhToán_NgườiDùng_XửLýBởi] FOREIGN KEY([XửLýBởi]) REFERENCES dbo.[NguờiDùng]([Id])
    );
END
GO

-- Nhật ký kiểm toán
IF OBJECT_ID(N'dbo.[NhậtKýKiểmToán]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[NhậtKýKiểmToán](
        [Id]         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [NgườiDùngId] INT NULL,
        [HànhĐộng]  NVARCHAR(50) NOT NULL,
        [Bảng]      NVARCHAR(50) NOT NULL,
        [BảnGhiId]  INT NULL,
        [GiáTrịCũ]  NVARCHAR(MAX) NULL,
        [GiáTrịMới] NVARCHAR(MAX) NULL,
        [ĐịaChỉIP]  NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [TạoLúc]    DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_NhậtKý_NgườiDùng] FOREIGN KEY([NgườiDùngId]) REFERENCES dbo.[NguờiDùng]([Id])
    );
END
GO

-- Thông báo
IF OBJECT_ID(N'dbo.[ThôngBáo]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[ThôngBáo](
        [Id]        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [NgườiDùngId] INT NOT NULL,
        [TiêuĐề]   NVARCHAR(200) NOT NULL,
        [NộiDung]  NVARCHAR(MAX) NOT NULL,
        [Loại]     NVARCHAR(20) NOT NULL, -- ThôngTin, CảnhBáo, Lỗi, ThànhCông
        [ĐãĐọc]    BIT NOT NULL DEFAULT 0,
        [ĐọcLúc]   DATETIME2 NULL,
        [TạoLúc]   DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_ThôngBáo_NgườiDùng] FOREIGN KEY([NgườiDùngId]) REFERENCES dbo.[NguờiDùng]([Id])
    );
END
GO

-- Hàng đợi tác vụ
IF OBJECT_ID(N'dbo.[HàngĐợiTácVụ]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[HàngĐợiTácVụ](
        [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [LoạiTácVụ]   NVARCHAR(50) NOT NULL,
        [DữLiệu]      NVARCHAR(MAX) NOT NULL,
        [TrạngThái]   NVARCHAR(20) NOT NULL DEFAULT N'Chờ', -- Chờ, ĐangXửLý, HoànTất, Lỗi
        [ƯuTiên]      INT NOT NULL DEFAULT 0,
        [SốLầnThử]    INT NOT NULL DEFAULT 0,
        [TốiĐaThử]    INT NOT NULL DEFAULT 3,
        [ThôngĐiệpLỗi] NVARCHAR(MAX) NULL,
        [TạoLúc]      DATETIME2 NOT NULL DEFAULT GETDATE(),
        [BắtĐầuLúc]   DATETIME2 NULL,
        [HoànTấtLúc]  DATETIME2 NULL
    );
END
GO

/* ==============================
   2) RBAC: Vai trò / Quyền
   ============================== */

IF OBJECT_ID(N'dbo.[VaiTrò]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[VaiTrò](
        [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TênVaiTrò]   NVARCHAR(50) NOT NULL UNIQUE,
        [MôTả]        NVARCHAR(200) NULL,
        [KíchHoạt]    BIT NOT NULL DEFAULT 1,
        [TạoLúc]      DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID(N'dbo.[Quyền]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[Quyền](
        [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MãQuyền]     NVARCHAR(100) NOT NULL UNIQUE,
        [PhânHệ]      NVARCHAR(50)  NOT NULL,
        [HànhĐộng]    NVARCHAR(50)  NOT NULL,
        [MôTả]        NVARCHAR(200) NULL,
        [TạoLúc]      DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID(N'dbo.[VaiTrò_Quyền]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[VaiTrò_Quyền](
        [VaiTròId]     INT NOT NULL,
        [QuyềnId]      INT NOT NULL,
        [CấpLúc]       DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_VaiTrò_Quyền] PRIMARY KEY ([VaiTròId],[QuyềnId]),
        CONSTRAINT [FK_VaiTrò_Quyền_VaiTrò] FOREIGN KEY([VaiTròId]) REFERENCES dbo.[VaiTrò]([Id]),
        CONSTRAINT [FK_VaiTrò_Quyền_Quyền]  FOREIGN KEY([QuyềnId])  REFERENCES dbo.[Quyền]([Id])
    );
END
GO

IF OBJECT_ID(N'dbo.[NgườiDùng_VaiTrò]','U') IS NULL
BEGIN
    CREATE TABLE dbo.[NgườiDùng_VaiTrò](
        [NgườiDùngId] INT NOT NULL,
        [VaiTròId]    INT NOT NULL,
        [CấpLúc]      DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_NgườiDùng_VaiTrò] PRIMARY KEY ([NgườiDùngId],[VaiTròId]),
        CONSTRAINT [FK_NgườiDùng_VaiTrò_NgườiDùng] FOREIGN KEY([NgườiDùngId]) REFERENCES dbo.[NguờiDùng]([Id]),
        CONSTRAINT [FK_NgườiDùng_VaiTrò_VaiTrò]    FOREIGN KEY([VaiTròId])    REFERENCES dbo.[VaiTrò]([Id])
    );
END
GO

/* ==============================
   3) CHỈ MỤC
   ============================== */
CREATE INDEX [IX_NgườiDùng_Email] ON dbo.[NguờiDùng]([Email]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_NgườiDùng_VaiTròChuỗi] ON dbo.[NguờiDùng]([VaiTròChuỗi]) WHERE [ĐãXóa]=0;

CREATE INDEX [IX_Phim_Tên]   ON dbo.[Phim]([TênPhim]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_Phim_ThểLoại] ON dbo.[Phim]([ThểLoại]) WHERE [ĐãXóa]=0;

CREATE INDEX [IX_SuấtChiếu_PhimId] ON dbo.[SuấtChiếu]([PhimId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_SuấtChiếu_PhòngChiếuId] ON dbo.[SuấtChiếu]([PhòngChiếuId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_SuấtChiếu_NgàyChiếu] ON dbo.[SuấtChiếu]([NgàyChiếu]) WHERE [ĐãXóa]=0;

CREATE INDEX [IX_ĐặtChỗ_KháchId] ON dbo.[ĐặtChỗ]([KháchId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_ĐặtChỗ_SuấtChiếuId] ON dbo.[ĐặtChỗ]([SuấtChiếuId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_ĐặtChỗ_TrạngThái] ON dbo.[ĐặtChỗ]([TrạngThái]) WHERE [ĐãXóa]=0;

CREATE INDEX [IX_Vé_ĐặtChỗId] ON dbo.[Vé]([ĐặtChỗId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_Vé_GhếId] ON dbo.[Vé]([GhếId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_Vé_TrạngThái] ON dbo.[Vé]([TrạngThái]) WHERE [ĐãXóa]=0;

CREATE INDEX [IX_HóaĐơn_ĐặtChỗId] ON dbo.[HóaĐơn]([ĐặtChỗId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_HóaĐơn_TrạngThái] ON dbo.[HóaĐơn]([TrạngThái]) WHERE [ĐãXóa]=0;

CREATE INDEX [IX_ThanhToán_HóaĐơnId] ON dbo.[ThanhToán]([HóaĐơnId]) WHERE [ĐãXóa]=0;
CREATE INDEX [IX_ThanhToán_TrạngThái] ON dbo.[ThanhToán]([TrạngThái]) WHERE [ĐãXóa]=0;

CREATE INDEX [IX_NhậtKý_NgườiDùngId] ON dbo.[NhậtKýKiểmToán]([NgườiDùngId]);
CREATE INDEX [IX_NhậtKý_TạoLúc] ON dbo.[NhậtKýKiểmToán]([TạoLúc]);

CREATE INDEX [IX_ThôngBáo_NgườiDùngId] ON dbo.[ThôngBáo]([NgườiDùngId]);
CREATE INDEX [IX_ThôngBáo_ĐãĐọc] ON dbo.[ThôngBáo]([ĐãĐọc]);

CREATE INDEX [IX_HàngĐợiTácVụ_TrạngThái] ON dbo.[HàngĐợiTácVụ]([TrạngThái]);
CREATE INDEX [IX_HàngĐợiTácVụ_ƯuTiên] ON dbo.[HàngĐợiTácVụ]([ƯuTiên]);
GO

/* ==============================
   4) HÀM TẠO MÃ (SP)
   ============================== */
CREATE OR ALTER PROCEDURE dbo.[TạoMãĐặtChỗ]
AS
BEGIN
    DECLARE @M NVARCHAR(20) = N'DC' + FORMAT(GETDATE(), 'yyyyMMdd') 
        + RIGHT('0000'+CAST(ABS(CHECKSUM(NEWID()))%10000 AS VARCHAR),4);
    SELECT @M AS [MãĐặtChỗ];
END
GO

CREATE OR ALTER PROCEDURE dbo.[TạoMãVé]
AS
BEGIN
    DECLARE @M NVARCHAR(20) = N'VE' + FORMAT(GETDATE(), 'yyyyMMdd') 
        + RIGHT('0000'+CAST(ABS(CHECKSUM(NEWID()))%10000 AS VARCHAR),4);
    SELECT @M AS [MãVé];
END
GO

CREATE OR ALTER PROCEDURE dbo.[TạoSốHóaĐơn]
AS
BEGIN
    DECLARE @S NVARCHAR(20) = N'HD' + FORMAT(GETDATE(), 'yyyyMMdd') 
        + RIGHT('0000'+CAST(ABS(CHECKSUM(NEWID()))%10000 AS VARCHAR),4);
    SELECT @S AS [SốHóaĐơn];
END
GO

/* ==============================
   5) TRIGGER SOFT-DELETE
   ============================== */
CREATE OR ALTER TRIGGER dbo.[trg_Phim_SoftDelete] ON dbo.[Phim]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE p SET [ĐãXóa]=1, [XóaLúc]=GETDATE(), [XóaBởi]=@NguoiId
    FROM dbo.[Phim] p JOIN deleted d ON d.Id = p.Id;
END
GO

CREATE OR ALTER TRIGGER dbo.[trg_SuấtChiếu_SoftDelete] ON dbo.[SuấtChiếu]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE s SET [ĐãXóa]=1, [XóaLúc]=GETDATE(), [XóaBởi]=@NguoiId
    FROM dbo.[SuấtChiếu] s JOIN deleted d ON d.Id = s.Id;
END
GO

CREATE OR ALTER TRIGGER dbo.[trg_Ghế_SoftDelete] ON dbo.[Ghế]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE g SET [ĐãXóa]=1, [XóaLúc]=GETDATE(), [XóaBởi]=@NguoiId
    FROM dbo.[Ghế] g JOIN deleted d ON d.Id = g.Id;
END
GO

CREATE OR ALTER TRIGGER dbo.[trg_ĐặtChỗ_SoftDelete] ON dbo.[ĐặtChỗ]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE r SET [ĐãXóa]=1, [XóaLúc]=GETDATE(), [XóaBởi]=@NguoiId
    FROM dbo.[ĐặtChỗ] r JOIN deleted d ON d.Id = r.Id;
END
GO

CREATE OR ALTER TRIGGER dbo.[trg_Vé_SoftDelete] ON dbo.[Vé]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE t SET [ĐãXóa]=1, [XóaLúc]=GETDATE(), [XóaBởi]=@NguoiId
    FROM dbo.[Vé] t JOIN deleted d ON d.Id = t.Id;
END
GO

CREATE OR ALTER TRIGGER dbo.[trg_HóaĐơn_SoftDelete] ON dbo.[HóaĐơn]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE i SET [ĐãXóa]=1, [XóaLúc]=GETDATE(), [XóaBởi]=@NguoiId
    FROM dbo.[HóaĐơn] i JOIN deleted d ON d.Id = i.Id;
END
GO

CREATE OR ALTER TRIGGER dbo.[trg_ThanhToán_SoftDelete] ON dbo.[ThanhToán]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE p SET [ĐãXóa]=1, [XóaLúc]=GETDATE(), [XóaBởi]=@NguoiId
    FROM dbo.[ThanhToán] p JOIN deleted d ON d.Id = p.Id;
END
GO

/* ==============================
   6) TRIGGER AUDIT (mẫu bảng Phim)
   ============================== */
CREATE OR ALTER TRIGGER dbo.[trg_Phim_Audit] ON dbo.[Phim]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    INSERT INTO dbo.[NhậtKýKiểmToán]([NgườiDùngId],[HànhĐộng],[Bảng],[BảnGhiId],[GiáTrịCũ],[GiáTrịMới],[TạoLúc])
    SELECT @NguoiId,
           CASE WHEN d.Id IS NULL THEN N'TẠO' ELSE N'CẬP NHẬT' END,
           N'Phim',
           i.Id,
           (SELECT d.Id, d.[TênPhim], d.[ThểLoại], d.[ThờiLượngPhút], d.[PhânLoại], d.[KíchHoạt], d.[ĐãXóa] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i.Id, i.[TênPhim], i.[ThểLoại], i.[ThờiLượngPhút], i.[PhânLoại], i.[KíchHoạt], i.[ĐãXóa] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           GETDATE()
    FROM inserted i
    LEFT JOIN deleted d ON d.Id = i.Id;
END
GO

/* ==============================
   7) VIEW CHO QUẢN TRỊ
   ============================== */
CREATE OR ALTER VIEW dbo.[vw_QuanTri_ĐặtChỗChiTiết]
AS
SELECT 
    r.[Id]              AS [ĐặtChỗId],
    r.[MãĐặtChỗ],
    r.[TrạngThái],
    r.[TổngTiền],
    r.[NgàyĐặt],
    r.[HếtHạn],
    u.[Id]              AS [KháchId],
    u.[TênĐăngNhập]     AS [Khách_TênĐăngNhập],
    u.[Email]           AS [Khách_Email],
    sc.[Id]             AS [SuấtChiếuId],
    sc.[NgàyChiếu],
    sc.[GiờBắtĐầu],
    sc.[GiờKếtThúc],
    p.[Id]              AS [PhimId],
    p.[TênPhim]         AS [Phim],
    pc.[Id]             AS [PhòngChiếuId],
    pc.[TênPhòng]       AS [PhòngChiếu]
FROM dbo.[ĐặtChỗ] r
JOIN dbo.[NguờiDùng] u  ON u.[Id]  = r.[KháchId]
JOIN dbo.[SuấtChiếu] sc ON sc.[Id] = r.[SuấtChiếuId]
JOIN dbo.[Phim] p       ON p.[Id]  = sc.[PhimId]
JOIN dbo.[PhòngChiếu] pc ON pc.[Id]= sc.[PhòngChiếuId]
WHERE r.[ĐãXóa]=0 AND sc.[ĐãXóa]=0 AND p.[ĐãXóa]=0 AND pc.[ĐãXóa]=0;
GO

CREATE OR ALTER VIEW dbo.[vw_QuanTri_DoanhThuTheoNgày]
AS
SELECT 
    CAST(hd.[NgàyLập] AS DATE) AS [Ngày],
    SUM(hd.[TạmTính])    AS [TổngTạmTính],
    SUM(hd.[TiềnThuế])   AS [TổngThuế],
    SUM(hd.[TiềnGiảm])   AS [TổngGiảm],
    SUM(hd.[ThànhTiền])  AS [TổngThànhTiền]
FROM dbo.[HóaĐơn] hd
WHERE hd.[ĐãXóa]=0 AND hd.[TrạngThái] IN (N'ĐãThanhToán', N'HoànTiền')
GROUP BY CAST(hd.[NgàyLập] AS DATE);
GO

/* ==============================
   8) THỦ TỤC (SP) QUẢN TRỊ
   ============================== */

-- Tìm kiếm phim + phân trang
CREATE OR ALTER PROCEDURE dbo.[sp_QuanTri_TìmPhim]
    @TừKhóa    NVARCHAR(200) = NULL,
    @ThểLoại   NVARCHAR(100) = NULL,
    @KíchHoạt  BIT           = NULL,
    @Trang     INT           = 1,
    @KíchThước INT           = 20,
    @SắpXếpTheo NVARCHAR(20) = N'TênPhim', -- TênPhim, NgàyPhátHành, TạoLúc
    @Chiều     NVARCHAR(4)   = N'ASC'      -- ASC | DESC
AS
BEGIN
    SET NOCOUNT ON;
    IF @Trang < 1 SET @Trang = 1;
    IF @KíchThước < 1 SET @KíchThước = 20;
    DECLARE @Offset INT = (@Trang - 1) * @KíchThước;

    ;WITH F AS (
        SELECT p.[Id], p.[TênPhim], p.[ThểLoại], p.[ThờiLượngPhút], p.[PhânLoại], p.[NgàyPhátHành], p.[KíchHoạt], p.[TạoLúc]
        FROM dbo.[Phim] p
        WHERE p.[ĐãXóa]=0
          AND (@KíchHoạt IS NULL OR p.[KíchHoạt]=@KíchHoạt)
          AND (@ThểLoại IS NULL OR p.[ThểLoại]=@ThểLoại)
          AND (@TừKhóa IS NULL OR p.[TênPhim] LIKE N'%'+@TừKhóa+N'%' OR p.[ĐạoDiễn] LIKE N'%'+@TừKhóa+N'%')
    )
    SELECT COUNT(1) AS [TổngBảnGhi] FROM F;

    SELECT [Id],[TênPhim],[ThểLoại],[ThờiLượngPhút],[PhânLoại],[NgàyPhátHành],[KíchHoạt],[TạoLúc]
    FROM F
    ORDER BY 
        CASE WHEN @SắpXếpTheo=N'TênPhim'       AND @Chiều=N'ASC'  THEN [TênPhim] END ASC,
        CASE WHEN @SắpXếpTheo=N'TênPhim'       AND @Chiều=N'DESC' THEN [TênPhim] END DESC,
        CASE WHEN @SắpXếpTheo=N'NgàyPhátHành'  AND @Chiều=N'ASC'  THEN CONVERT(NVARCHAR(30),[NgàyPhátHành],112) END ASC,
        CASE WHEN @SắpXếpTheo=N'NgàyPhátHành'  AND @Chiều=N'DESC' THEN CONVERT(NVARCHAR(30),[NgàyPhátHành],112) END DESC,
        CASE WHEN @SắpXếpTheo=N'TạoLúc'        AND @Chiều=N'ASC'  THEN CONVERT(NVARCHAR(30),[TạoLúc],126) END ASC,
        CASE WHEN @SắpXếpTheo=N'TạoLúc'        AND @Chiều=N'DESC' THEN CONVERT(NVARCHAR(30),[TạoLúc],126) END DESC
    OFFSET @Offset ROWS FETCH NEXT @KíchThước ROWS ONLY;
END
GO

-- Doanh thu theo khoảng ngày
CREATE OR ALTER PROCEDURE dbo.[sp_QuanTri_TổngHợpDoanhThu]
    @TừNgày DATE,
    @ĐếnNgày DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        CAST([NgàyLập] AS DATE) AS [Ngày],
        SUM([TạmTính])   AS [TổngTạmTính],
        SUM([TiềnThuế])  AS [TổngThuế],
        SUM([TiềnGiảm])  AS [TổngGiảm],
        SUM([ThànhTiền]) AS [TổngThànhTiền]
    FROM dbo.[HóaĐơn]
    WHERE [ĐãXóa]=0
      AND [NgàyLập] >= @TừNgày
      AND [NgàyLập] < DATEADD(DAY,1,@ĐếnNgày)
      AND [TrạngThái] IN (N'ĐãThanhToán', N'HoànTiền')
    GROUP BY CAST([NgàyLập] AS DATE)
    ORDER BY CAST([NgàyLập] AS DATE);
END
GO

-- Đặt ghế an toàn (chống double-booking)
CREATE OR ALTER PROCEDURE dbo.[sp_QuanTri_ĐặtGhếAnToàn]
    @KháchId   INT,
    @SuấtChiếuId INT,
    @DanhSáchGhếCsv NVARCHAR(MAX),
    @GhiChú    NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @KháchId IS NULL OR @KháchId<=0 THROW 50000, N'KháchId không hợp lệ', 1;
    IF @SuấtChiếuId IS NULL OR @SuấtChiếuId<=0 THROW 50001, N'SuấtChiếuId không hợp lệ', 1;
    IF @DanhSáchGhếCsv IS NULL OR LEN(@DanhSáchGhếCsv)=0 THROW 50002, N'Chưa truyền danh sách ghế', 1;

    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);

    DECLARE @Ghế TABLE([GhếId] INT PRIMARY KEY);
    INSERT INTO @Ghế([GhếId])
    SELECT DISTINCT TRY_CAST(value AS INT)
    FROM string_split(@DanhSáchGhếCsv, ',')
    WHERE TRY_CAST(value AS INT) IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM @Ghế) THROW 50003, N'Danh sách ghế không hợp lệ', 1;

    DECLARE @GiáCơBản DECIMAL(10,2), @PhòngId INT;
    SELECT @GiáCơBản = sc.[GiáCơBản], @PhòngId = sc.[PhòngChiếuId]
    FROM dbo.[SuấtChiếu] sc
    WHERE sc.[Id]=@SuấtChiếuId AND sc.[ĐãXóa]=0 AND sc.[KíchHoạt]=1;

    IF @GiáCơBản IS NULL THROW 50004, N'Suất chiếu không tồn tại hoặc không hoạt động', 1;

    BEGIN TRAN;

    -- Kiểm tra ghế đã có vé Active cho cùng suất
    IF EXISTS (
        SELECT 1
        FROM @Ghế g
        JOIN dbo.[Ghế] gh WITH (UPDLOCK, ROWLOCK) ON gh.[Id]=g.[GhếId] AND gh.[PhòngChiếuId]=@PhòngId AND gh.[ĐãXóa]=0 AND gh.[KíchHoạt]=1
        JOIN dbo.[Vé] v ON v.[GhếId]=gh.[Id] AND v.[ĐãXóa]=0 AND v.[TrạngThái]=N'HoạtĐộng'
        JOIN dbo.[ĐặtChỗ] r ON r.[Id]=v.[ĐặtChỗId] AND r.[ĐãXóa]=0 AND r.[TrạngThái] IN (N'Chờ',N'ĐãXácNhận') AND r.[SuấtChiếuId]=@SuấtChiếuId
    )
    BEGIN
        ROLLBACK TRAN; THROW 50005, N'Một số ghế đã được đặt', 1;
    END

    DECLARE @Tổng DECIMAL(10,2)=0.0;
    SELECT @Tổng = SUM(@GiáCơBản * gh.[HệSốGiá])
    FROM @Ghế g
    JOIN dbo.[Ghế] gh WITH (UPDLOCK, ROWLOCK) ON gh.[Id]=g.[GhếId] AND gh.[PhòngChiếuId]=@PhòngId AND gh.[ĐãXóa]=0 AND gh.[KíchHoạt]=1;

    DECLARE @MãĐặtChỗ NVARCHAR(20);
    DECLARE @tmp TABLE([MãĐặtChỗ] NVARCHAR(20));
    INSERT INTO @tmp EXEC dbo.[TạoMãĐặtChỗ];
    SELECT @MãĐặtChỗ=[MãĐặtChỗ] FROM @tmp;

    DECLARE @ĐặtChỗId INT;
    INSERT INTO dbo.[ĐặtChỗ]([KháchId],[SuấtChiếuId],[MãĐặtChỗ],[TrạngThái],[TổngTiền],[NgàyĐặt],[HếtHạn],[GhiChú],[TạoLúc],[SửaLúc],[TạoBởi],[SửaBởi],[ĐãXóa])
    VALUES(@KháchId,@SuấtChiếuId,@MãĐặtChỗ,N'Chờ',@Tổng,GETDATE(),DATEADD(MINUTE,15,GETDATE()),@GhiChú,GETDATE(),GETDATE(),@NguoiId,@NguoiId,0);
    SET @ĐặtChỗId = SCOPE_IDENTITY();

    DECLARE @GhếId INT, @Giá DECIMAL(10,2), @MãVé NVARCHAR(20);
    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT [GhếId] FROM @Ghế;
    OPEN cur; FETCH NEXT FROM cur INTO @GhếId;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SELECT @Giá = @GiáCơBản * gh.[HệSốGiá] FROM dbo.[Ghế] gh WITH (UPDLOCK, ROWLOCK) WHERE gh.[Id]=@GhếId;

        DECLARE @t TABLE([MãVé] NVARCHAR(20));
        INSERT INTO @t EXEC dbo.[TạoMãVé];
        SELECT @MãVé=[MãVé] FROM @t;

        INSERT INTO dbo.[Vé]([ĐặtChỗId],[GhếId],[MãVé],[MãQR],[Giá],[TrạngThái],[TạoLúc],[SửaLúc],[TạoBởi],[SửaBởi],[ĐãXóa])
        VALUES(@ĐặtChỗId,@GhếId,@MãVé,NULL,@Giá,N'HoạtĐộng',GETDATE(),GETDATE(),@NguoiId,@NguoiId,0);

        FETCH NEXT FROM cur INTO @GhếId;
    END
    CLOSE cur; DEALLOCATE cur;

    COMMIT TRAN;

    SELECT r.[Id], r.[MãĐặtChỗ], r.[TổngTiền], r.[TrạngThái], r.[NgàyĐặt], r.[HếtHạn]
    FROM dbo.[ĐặtChỗ] r WHERE r.[Id]=@ĐặtChỗId;

    SELECT v.[Id], v.[MãVé], v.[GhếId], v.[Giá], v.[TrạngThái]
    FROM dbo.[Vé] v WHERE v.[ĐặtChỗId]=@ĐặtChỗId ORDER BY v.[Id];
END
GO

-- Xác nhận / Huỷ đặt chỗ
CREATE OR ALTER PROCEDURE dbo.[sp_QuanTri_XácNhậnĐặtChỗ]
    @ĐặtChỗId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.[ĐặtChỗ]
      SET [TrạngThái]=N'ĐãXácNhận',[SửaLúc]=GETDATE()
    WHERE [Id]=@ĐặtChỗId AND [ĐãXóa]=0 AND [TrạngThái]=N'Chờ';

    SELECT [Id],[MãĐặtChỗ],[TrạngThái],[TổngTiền] FROM dbo.[ĐặtChỗ] WHERE [Id]=@ĐặtChỗId;
END
GO

CREATE OR ALTER PROCEDURE dbo.[sp_QuanTri_HủyĐặtChỗ]
    @ĐặtChỗId INT,
    @LýDo NVARCHAR(200)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.[ĐặtChỗ]
      SET [TrạngThái]=N'ĐãHủy',
          [GhiChú]=CONCAT(ISNULL([GhiChú],N''),N' | Hủy: ',ISNULL(@LýDo,N'')),
          [SửaLúc]=GETDATE()
    WHERE [Id]=@ĐặtChỗId AND [ĐãXóa]=0 AND [TrạngThái] IN (N'Chờ',N'ĐãXácNhận');

    UPDATE dbo.[Vé]
      SET [TrạngThái]=N'ĐãHủy',[SửaLúc]=GETDATE()
    WHERE [ĐặtChỗId]=@ĐặtChỗId AND [ĐãXóa]=0 AND [TrạngThái]=N'HoạtĐộng';

    SELECT [Id],[MãĐặtChỗ],[TrạngThái],[TổngTiền] FROM dbo.[ĐặtChỗ] WHERE [Id]=@ĐặtChỗId;
END
GO

-- Check-in vé
CREATE OR ALTER PROCEDURE dbo.[sp_QuanTri_CheckInVé]
    @MãVé NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NguoiId INT = TRY_CAST(SESSION_CONTEXT(N'NguoiDungId') AS INT);
    UPDATE dbo.[Vé]
      SET [TrạngThái]=N'ĐãDùng',[DùngLúc]=GETDATE(),[DùngBởi]=@NguoiId,[SửaLúc]=GETDATE()
    WHERE [MãVé]=@MãVé AND [ĐãXóa]=0 AND [TrạngThái]=N'HoạtĐộng';

    SELECT [Id],[MãVé],[TrạngThái],[DùngLúc],[DùngBởi]
    FROM dbo.[Vé] WHERE [MãVé]=@MãVé;
END
GO

-- Xuất danh sách phim (cho CSV/Excel)
CREATE OR ALTER PROCEDURE dbo.[sp_QuanTri_XuấtPhim]
    @TừKhóa  NVARCHAR(200)=NULL,
    @ThểLoại NVARCHAR(100)=NULL,
    @KíchHoạt BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id],[TênPhim],[ThểLoại],[ThờiLượngPhút],[PhânLoại],[NgàyPhátHành],[KíchHoạt]
    FROM dbo.[Phim]
    WHERE [ĐãXóa]=0
      AND (@KíchHoạt IS NULL OR [KíchHoạt]=@KíchHoạt)
      AND (@ThểLoại IS NULL OR [ThểLoại]=@ThểLoại)
      AND (@TừKhóa IS NULL OR [TênPhim] LIKE N'%'+@TừKhóa+N'%' OR [ĐạoDiễn] LIKE N'%'+@TừKhóa+N'%')
    ORDER BY [TênPhim] ASC, [Id] ASC;
END
GO

/* ==============================
   9) SEED DỮ LIỆU TIẾNG VIỆT
   ============================== */

-- Vai trò
IF NOT EXISTS (SELECT 1 FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'QuảnTrị')
    INSERT INTO dbo.[VaiTrò]([TênVaiTrò],[MôTả]) VALUES (N'QuảnTrị',N'Quản trị hệ thống');
IF NOT EXISTS (SELECT 1 FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'QuảnLý')
    INSERT INTO dbo.[VaiTrò]([TênVaiTrò],[MôTả]) VALUES (N'QuảnLý',N'Quản lý vận hành');
IF NOT EXISTS (SELECT 1 FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'NhânViên')
    INSERT INTO dbo.[VaiTrò]([TênVaiTrò],[MôTả]) VALUES (N'NhânViên',N'Nhân viên bán vé');
IF NOT EXISTS (SELECT 1 FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'Khách')
    INSERT INTO dbo.[VaiTrò]([TênVaiTrò],[MôTả]) VALUES (N'Khách',N'Khách hàng');
GO

-- Quyền (một số quyền mẫu)
IF NOT EXISTS (SELECT 1 FROM dbo.[Quyền])
BEGIN
    INSERT INTO dbo.[Quyền]([MãQuyền],[PhânHệ],[HànhĐộng],[MôTả]) VALUES
    (N'PHIM_XEM',        N'Phim',        N'Xem',       N'Xem phim'),
    (N'PHIM_TAO',        N'Phim',        N'Tạo',       N'Tạo phim'),
    (N'PHIM_SUA',        N'Phim',        N'Sửa',       N'Sửa phim'),
    (N'PHIM_XOA',        N'Phim',        N'Xoá',       N'Xoá phim'),
    (N'SUATCHIEU_XEM',   N'SuấtChiếu',   N'Xem',       N'Xem suất chiếu'),
    (N'SUATCHIEU_QUANLY',N'SuấtChiếu',   N'QuảnLý',    N'Tạo/Sửa/Xoá suất chiếu'),
    (N'DATCHO_XEM',      N'ĐặtChỗ',      N'Xem',       N'Xem đặt chỗ'),
    (N'DATCHO_QUANLY',   N'ĐặtChỗ',      N'QuảnLý',    N'Duyệt/Huỷ đặt chỗ'),
    (N'VE_CHECKIN',      N'Vé',          N'CheckIn',   N'Quét QR check-in'),
    (N'HOADON_XEM',      N'HóaĐơn',      N'Xem',       N'Xem hoá đơn'),
    (N'THANHTOAN_XEM',   N'ThanhToán',   N'Xem',       N'Xem thanh toán'),
    (N'BAOCAO_XEM',      N'BáoCáo',      N'Xem',       N'Xem báo cáo'),
    (N'NGUOIDUNG_QUANLY',N'NgườiDùng',   N'QuảnLý',    N'Quản trị người dùng');
END
GO

-- Gán quyền mặc định
DECLARE @IdQT INT = (SELECT [Id] FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'QuảnTrị');
DECLARE @IdQL INT = (SELECT [Id] FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'QuảnLý');
DECLARE @IdNV INT = (SELECT [Id] FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'NhânViên');

;WITH Q AS (SELECT [Id],[MãQuyền] FROM dbo.[Quyền])
INSERT INTO dbo.[VaiTrò_Quyền]([VaiTròId],[QuyềnId])
SELECT @IdQT, [Id] FROM Q
WHERE NOT EXISTS(SELECT 1 FROM dbo.[VaiTrò_Quyền] x WHERE x.[VaiTròId]=@IdQT AND x.[QuyềnId]=Q.[Id]);

INSERT INTO dbo.[VaiTrò_Quyền]([VaiTròId],[QuyềnId])
SELECT @IdQL, [Id] FROM Q
WHERE [MãQuyền] IN (N'PHIM_XEM',N'PHIM_TAO',N'PHIM_SUA',
                    N'SUATCHIEU_XEM',N'SUATCHIEU_QUANLY',
                    N'DATCHO_XEM',N'DATCHO_QUANLY',
                    N'VE_CHECKIN',N'HOADON_XEM',N'THANHTOAN_XEM',N'BAOCAO_XEM')
  AND NOT EXISTS(SELECT 1 FROM dbo.[VaiTrò_Quyền] x WHERE x.[VaiTròId]=@IdQL AND x.[QuyềnId]=Q.[Id]);

INSERT INTO dbo.[VaiTrò_Quyền]([VaiTròId],[QuyềnId])
SELECT @IdNV, [Id] FROM Q
WHERE [MãQuyền] IN (N'SUATCHIEU_XEM',N'DATCHO_XEM',N'VE_CHECKIN',N'HOADON_XEM',N'THANHTOAN_XEM')
  AND NOT EXISTS(SELECT 1 FROM dbo.[VaiTrò_Quyền] x WHERE x.[VaiTròId]=@IdNV AND x.[QuyềnId]=Q.[Id]);
GO

-- Người dùng mẫu
IF NOT EXISTS (SELECT 1 FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'quantri')
INSERT INTO dbo.[NguờiDùng]([TênĐăngNhập],[Email],[BămMậtKhẩu],[Họ],[Tên],[VaiTròChuỗi],[KíchHoạt],[XácThựcEmail])
VALUES (N'quantri',N'quantri@rap.com',N'$2a$11$hash_demo',N'Quản',N'Trị',N'QuảnTrị',1,1);

IF NOT EXISTS (SELECT 1 FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'quanly')
INSERT INTO dbo.[NguờiDùng]([TênĐăngNhập],[Email],[BămMậtKhẩu],[Họ],[Tên],[VaiTròChuỗi],[KíchHoạt],[XácThựcEmail])
VALUES (N'quanly',N'quanly@rap.com',N'$2a$11$hash_demo',N'Quản',N'Lý',N'QuảnLý',1,1);

IF NOT EXISTS (SELECT 1 FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'nhanvien')
INSERT INTO dbo.[NguờiDùng]([TênĐăngNhập],[Email],[BămMậtKhẩu],[Họ],[Tên],[VaiTròChuỗi],[KíchHoạt],[XácThựcEmail])
VALUES (N'nhanvien',N'nhanvien@rap.com',N'$2a$11$hash_demo',N'Nhân',N'Viên',N'NhânViên',1,1);

IF NOT EXISTS (SELECT 1 FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'khach')
INSERT INTO dbo.[NguờiDùng]([TênĐăngNhập],[Email],[BămMậtKhẩu],[Họ],[Tên],[VaiTròChuỗi],[KíchHoạt],[XácThựcEmail])
VALUES (N'khach',N'khach@rap.com',N'$2a$11$hash_demo',N'Khách',N'Hàng',N'Khách',1,1);
GO

-- Gán vai trò cho người dùng mẫu
DECLARE @UQT INT=(SELECT TOP 1 [Id] FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'quantri');
DECLARE @UQL INT=(SELECT TOP 1 [Id] FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'quanly');
DECLARE @UNV INT=(SELECT TOP 1 [Id] FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'nhanvien');
DECLARE @UKH INT=(SELECT TOP 1 [Id] FROM dbo.[NguờiDùng] WHERE [TênĐăngNhập]=N'khach');
DECLARE @IdKH INT=(SELECT [Id] FROM dbo.[VaiTrò] WHERE [TênVaiTrò]=N'Khách');

IF @UQT IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.[NgườiDùng_VaiTrò] WHERE [NgườiDùngId]=@UQT AND [VaiTròId]=@IdQT)
    INSERT INTO dbo.[NgườiDùng_VaiTrò]([NgườiDùngId],[VaiTròId]) VALUES (@UQT,@IdQT);
IF @UQL IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.[NgườiDùng_VaiTrò] WHERE [NgườiDùngId]=@UQL AND [VaiTròId]=@IdQL)
    INSERT INTO dbo.[NgườiDùng_VaiTrò]([NgườiDùngId],[VaiTròId]) VALUES (@UQL,@IdQL);
IF @UNV IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.[NgườiDùng_VaiTrò] WHERE [NgườiDùngId]=@UNV AND [VaiTròId]=@IdNV)
    INSERT INTO dbo.[NgườiDùng_VaiTrò]([NgườiDùngId],[VaiTròId]) VALUES (@UNV,@IdNV);
IF @UKH IS NOT NULL AND @IdKH IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.[NgườiDùng_VaiTrò] WHERE [NgườiDùngId]=@UKH AND [VaiTròId]=@IdKH)
    INSERT INTO dbo.[NgườiDùng_VaiTrò]([NgườiDùngId],[VaiTròId]) VALUES (@UKH,@IdKH);
GO

-- Phòng chiếu mẫu
IF NOT EXISTS (SELECT 1 FROM dbo.[PhòngChiếu])
BEGIN
INSERT INTO dbo.[PhòngChiếu]([TênPhòng],[SứcChứa],[LoạiMàn]) VALUES
(N'Phòng 1',100,N'2D'),
(N'Phòng 2', 80,N'3D'),
(N'Phòng 3',120,N'IMAX'),
(N'Phòng VIP',50,N'4DX');
END
GO

-- Ghế mẫu cho Phòng 1 (A1..J10)
IF NOT EXISTS (SELECT 1 FROM dbo.[Ghế] WHERE [PhòngChiếuId]=1)
BEGIN
    DECLARE @h CHAR(1)='A', @i INT;
    WHILE @h<='J'
    BEGIN
        SET @i=1;
        WHILE @i<=10
        BEGIN
            INSERT INTO dbo.[Ghế]([PhòngChiếuId],[Hàng],[Số],[LoạiGhế],[HệSốGiá])
            VALUES(1,@h,@i,N'Thường',1.00);
            SET @i=@i+1;
        END
        SET @h=CHAR(ASCII(@h)+1);
    END
END
GO

-- Phim mẫu (tên & mô tả tiếng Việt)
IF NOT EXISTS (SELECT 1 FROM dbo.[Phim])
BEGIN
INSERT INTO dbo.[Phim]([TênPhim],[MôTả],[ThểLoại],[ThờiLượngPhút],[PhânLoại],[NgàyPhátHành],[ĐạoDiễn],[DiễnViên],[KíchHoạt]) VALUES
(N'Biệt Đội Siêu Anh Hùng: Hồi Kết', N'Sau khi ác nhân hủy diệt nửa vũ trụ, các anh hùng còn lại hợp lực để đảo ngược.', N'Hành động', 181, N'C13', '2019-04-26', N'Anthony Russo, Joe Russo', N'Robert Downey Jr., Chris Evans', 1),
(N'Người Nhện: Không Còn Nhà', N'Peter Parker đối mặt hệ quả khi danh tính bị lộ.', N'Hành động', 148, N'C13', '2021-12-17', N'Jon Watts', N'Tom Holland, Zendaya, Benedict Cumberbatch', 1),
(N'Phi Công Siêu Đẳng: Maverick', N'Maverick trở lại với nhiệm vụ nguy hiểm nhất.', N'Hành động', 131, N'C13', '2022-05-27', N'Joseph Kosinski', N'Tom Cruise, Miles Teller', 1),
(N'Chiến Binh Báo Đen: Wakanda Bất Diệt', N'Wakanda đối mặt thử thách mới.', N'Hành động', 161, N'C13', '2022-11-11', N'Ryan Coogler', N'Letitia Wright, Angela Bassett', 1),
(N'Avatar: Dòng Chảy Của Nước', N'Gia đình Jake khám phá đại dương Pandora.', N'Khoa học viễn tưởng', 192, N'C13', '2022-12-16', N'James Cameron', N'Sam Worthington, Zoe Saldana', 1);
END
GO

-- Suất chiếu mẫu
IF NOT EXISTS (SELECT 1 FROM dbo.[SuấtChiếu] WHERE [NgàyChiếu]='2024-01-15')
BEGIN
INSERT INTO dbo.[SuấtChiếu]([PhimId],[PhòngChiếuId],[NgàyChiếu],[GiờBắtĐầu],[GiờKếtThúc],[GiáCơBản]) VALUES
(1,1,'2024-01-15','09:00:00','12:01:00',  80000),
(1,2,'2024-01-15','14:00:00','17:01:00', 100000),
(2,1,'2024-01-15','19:00:00','21:28:00',  80000),
(3,3,'2024-01-15','10:00:00','12:11:00', 120000),
(4,2,'2024-01-15','16:00:00','18:41:00', 100000),
(5,4,'2024-01-15','20:00:00','23:12:00', 150000);
END
GO

PRINT N'✅ ĐÃ TẠO THÀNH CÔNG HỆ THỐNG RẠP PHIM (TIẾNG VIỆT).';
PRINT N'ℹ️ Trước khi ghi/xoá trong phiên API, đặt UserId để audit/soft-delete: EXEC sp_set_session_context @key=N''NguoiDungId'', @value=1;';