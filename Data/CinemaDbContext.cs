using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyRapPhim.Models;

namespace QuanLyRapPhim.Data;

public partial class CinemaDbContext : DbContext
{
    public CinemaDbContext()
    {
    }

    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditorium> Auditoriums { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__Auditori__20BD5E5BBFAD0F06");

            entity.Property(e => e.MaPhong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenPhong).HasMaxLength(100);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__Invoices__835ED13BC0E4583F");

            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDatCho)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKm)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaKM");
            entity.Property(e => e.MaNv)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaNV");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TongTien).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaDatChoNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.MaDatCho)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__MaDatC__5441852A");

            entity.HasOne(d => d.MaKmNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.MaKm)
                .HasConstraintName("FK__Invoices__MaKM__5629CD9C");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__Invoices__MaNV__5535A963");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.MaCthd).HasName("PK__InvoiceD__1E4FA771049C5C1A");

            entity.Property(e => e.MaCthd)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCTHD");
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaSP");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__InvoiceDe__MaHoa__60A75C0F");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK__InvoiceDet__MaSP__619B8048");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MaPhim).HasName("PK__Movies__4AC03DE3D3E15591");

            entity.Property(e => e.MaPhim)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenPhim).HasMaxLength(255);
            entity.Property(e => e.TheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__Payments__D4B258441FE19357");

            entity.Property(e => e.MaThanhToan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HinhThucTt)
                .HasMaxLength(50)
                .HasColumnName("HinhThucTT");
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayThanhToan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoTien).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.MaHoaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__MaHoaD__59FA5E80");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.MaSp).HasName("PK__Products__2725081CC91B41D2");

            entity.Property(e => e.MaSp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaSP");
            entity.Property(e => e.Gia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Loai).HasMaxLength(50);
            entity.Property(e => e.TenSp)
                .HasMaxLength(100)
                .HasColumnName("TenSP");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.MaKm).HasName("PK__Promotio__2725CF15E63F5617");

            entity.Property(e => e.MaKm)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaKM");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenKm)
                .HasMaxLength(100)
                .HasColumnName("TenKM");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.MaDatCho).HasName("PK__Reservat__707DAE6B08A99E14");

            entity.Property(e => e.MaDatCho)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaGhe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSuatChieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ThoiGianDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MaGheNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.MaGhe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__MaGhe__49C3F6B7");

            entity.HasOne(d => d.MaSuatChieuNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.MaSuatChieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__MaSua__48CFD27E");

            entity.HasOne(d => d.User).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservati__UserI__4AB81AF0");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.MaGhe).HasName("PK__Seats__3CD3C67BAF8ACB22");

            entity.Property(e => e.MaGhe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoaiGhe).HasMaxLength(50);
            entity.Property(e => e.MaPhong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoGhe)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Seats)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Seats__MaPhong__3B75D760");
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(e => e.MaSuatChieu).HasName("PK__Showtime__CF5984D2D84843B2");

            entity.Property(e => e.MaSuatChieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GioBatDau).HasColumnType("datetime");
            entity.Property(e => e.GioKetThuc).HasColumnType("datetime");
            entity.Property(e => e.MaPhim)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhong)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MaPhimNavigation).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.MaPhim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Showtimes__MaPhi__3E52440B");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Showtimes__MaPho__3F466844");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.MaNv).HasName("PK__Staff__2725D70AA443D798");

            entity.Property(e => e.MaNv)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaNV");
            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.MaVe).HasName("PK__Tickets__2725100FB9A3F439");

            entity.Property(e => e.MaVe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GiaVe).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaDatCho)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Valid");

            entity.HasOne(d => d.MaDatChoNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.MaDatCho)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__MaDatCh__4E88ABD4");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0D380BA7");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534052A4C36").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("Customer");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
