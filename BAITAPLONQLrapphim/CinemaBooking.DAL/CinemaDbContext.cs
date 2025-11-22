using CinemaBooking.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.DAL;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options)
    {
    }

    // Xác thực & Phân quyền (Auth & RBAC)
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    // Phim
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }

    // Phòng chiếu & Ghế
    public DbSet<Auditorium> Auditoriums { get; set; }
    public DbSet<SeatType> SeatTypes { get; set; }
    public DbSet<Seat> Seats { get; set; }

    // Suất chiếu
    public DbSet<Showtime> Showtimes { get; set; }

    // Đặt vé
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    // Hóa đơn & Thanh toán
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Kiểm toán
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Chỉ mục người dùng
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.NormalizedUsername })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.NormalizedEmail })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Chỉ mục vai trò
        modelBuilder.Entity<Role>()
            .HasIndex(r => new { r.NormalizedRoleName })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Ràng buộc duy nhất UserRole
        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();

        // Chỉ mục quyền
        modelBuilder.Entity<Permission>()
            .HasIndex(p => new { p.PermissionKey })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Ràng buộc duy nhất RolePermission
        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        // Chỉ mục thể loại
        modelBuilder.Entity<Genre>()
            .HasIndex(g => new { g.GenreName })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Chỉ mục phim
        modelBuilder.Entity<Movie>()
            .HasIndex(m => new { m.Title })
            .HasFilter("[IsDeleted] = 0");

        // Ràng buộc duy nhất MovieGenre
        modelBuilder.Entity<MovieGenre>()
            .HasIndex(mg => new { mg.MovieId, mg.GenreId })
            .IsUnique();

        // Chỉ mục phòng chiếu
        modelBuilder.Entity<Auditorium>()
            .HasIndex(a => new { a.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Chỉ mục loại ghế
        modelBuilder.Entity<SeatType>()
            .HasIndex(st => new { st.SeatTypeName })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Chỉ mục ghế và ràng buộc duy nhất
        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.AuditoriumId })
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.AuditoriumId, s.RowLabel, s.SeatNumber })
            .IsUnique();

        // Chỉ mục suất chiếu
        modelBuilder.Entity<Showtime>()
            .HasIndex(s => new { s.MovieId, s.StartTime })
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<Showtime>()
            .HasIndex(s => new { s.AuditoriumId, s.StartTime })
            .HasFilter("[IsDeleted] = 0");

        // Chỉ mục đặt chỗ
        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new { r.ReservationCode })
            .IsUnique();

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new { r.UserId, r.ReservedAt })
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new { r.ShowtimeId, r.ReservedAt })
            .HasFilter("[IsDeleted] = 0");

        // Ràng buộc duy nhất cho vé (ngăn chặn đặt trùng)
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.ShowtimeId, t.SeatId })
            .IsUnique();

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.ReservationId });

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.ShowtimeId });

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.QrCodeData });

        // Chỉ mục hóa đơn
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => new { i.InvoiceNumber })
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => new { i.UserId, i.IssuedAt })
            .HasFilter("[IsDeleted] = 0");

        // Chỉ mục thanh toán
        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.InvoiceId });

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.PaymentStatus, p.PaidAt });

        // Ràng buộc kiểm tra trạng thái đặt chỗ
        modelBuilder.Entity<Reservation>()
            .ToTable(t => t.HasCheckConstraint("CK_Reservations_Status", "[Status] IN (N'Pending', N'Confirmed', N'Cancelled', N'Expired')"));

        // Ràng buộc kiểm tra trạng thái vé
        modelBuilder.Entity<Ticket>()
            .ToTable(t => t.HasCheckConstraint("CK_Tickets_Status", "[Status] IN (N'Booked', N'Paid', N'Cancelled', N'CheckedIn')"));

        // Ràng buộc kiểm tra trạng thái thanh toán
        modelBuilder.Entity<Payment>()
            .ToTable(t => t.HasCheckConstraint("CK_Payments_Status", "[PaymentStatus] IN (N'Pending', N'Success', N'Failed', N'Refunded')"));
    }
}

