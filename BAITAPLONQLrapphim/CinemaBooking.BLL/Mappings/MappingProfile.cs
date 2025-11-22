using AutoMapper;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Entities;

namespace CinemaBooking.BLL.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ===============================
        // USER MAPPINGS (Người dùng)
        // ===============================

        // Mapping từ entity User sang UserDto (1-1 theo thuộc tính)
        CreateMap<User, UserDto>();


        // ===============================
        // MOVIE MAPPINGS (Phim)
        // ===============================

        // Mapping Movie → MovieDto
        // Lấy danh sách Genre qua bảng trung gian MovieGenres
        CreateMap<Movie, MovieDto>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src =>
                src.MovieGenres.Select(mg => mg.Genre)));

        // Mapping DTO tạo phim → entity Movie
        CreateMap<CreateMovieRequest, Movie>();

        // Mapping DTO cập nhật phim → entity Movie
        CreateMap<UpdateMovieRequest, Movie>();


        // ===============================
        // GENRE MAPPINGS (Thể loại)
        // ===============================

        // Genre → GenreDto
        CreateMap<Genre, GenreDto>();


        // ===============================
        // SHOWTIME MAPPINGS (Suất chiếu)
        // ===============================

        // Showtime → ShowtimeDto
        // Lấy thêm các thông tin liên quan như tên phim, tên phòng chiếu
        CreateMap<Showtime, ShowtimeDto>()
            .ForMember(dest => dest.MovieTitle, opt =>
                opt.MapFrom(src => src.Movie.Title))
            .ForMember(dest => dest.AuditoriumName, opt =>
                opt.MapFrom(src => src.Auditorium.Name));

        // Mapping tạo showtime – bỏ qua EndTime (sẽ tính ra sau)
        CreateMap<CreateShowtimeRequest, Showtime>()
            .ForMember(dest => dest.EndTime, opt => opt.Ignore());

        // Mapping cập nhật showtime – cũng bỏ qua EndTime
        CreateMap<UpdateShowtimeRequest, Showtime>()
            .ForMember(dest => dest.EndTime, opt => opt.Ignore());


        // ===============================
        // AUDITORIUM & SEAT MAPPINGS
        // (Phòng chiếu & ghế)
        // ===============================

        // Phòng chiếu
        CreateMap<Auditorium, AuditoriumDto>();

        // Ghế → SeatDto, bao gồm loại ghế
        CreateMap<Seat, SeatDto>()
            .ForMember(dest => dest.SeatTypeName, opt =>
                opt.MapFrom(src => src.SeatType.SeatTypeName));

        // Loại ghế
        CreateMap<SeatType, SeatTypeDto>();


        // ===============================
        // BOOKING MAPPINGS (Đặt vé)
        // ===============================

        // Reservation → BookingDto
        CreateMap<Reservation, BookingDto>()
            .ForMember(dest => dest.UserName, opt =>
                opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Showtime, opt =>
                opt.MapFrom(src => src.Showtime))
            .ForMember(dest => dest.MovieTitle, opt =>
                opt.MapFrom(src => src.Showtime.Movie.Title))
            .ForMember(dest => dest.AuditoriumName, opt =>
                opt.MapFrom(src => src.Showtime.Auditorium.Name))
            .ForMember(dest => dest.ShowtimeStartTime, opt =>
                opt.MapFrom(src => src.Showtime.StartTime))
            // Lấy InvoiceId nếu tồn tại, nếu không thì null
            .ForMember(dest => dest.InvoiceId, opt =>
                opt.MapFrom(src =>
                    src.Invoices.FirstOrDefault() != null
                        ? (int?)src.Invoices.FirstOrDefault()!.InvoiceId
                        : null));

        // Ticket → TicketDto (đầy đủ thông tin ghế, suất chiếu, phim…)
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.RowLabel, opt =>
                opt.MapFrom(src => src.Seat.RowLabel))
            .ForMember(dest => dest.SeatNumber, opt =>
                opt.MapFrom(src => src.Seat.SeatNumber))
            .ForMember(dest => dest.SeatTypeName, opt =>
                opt.MapFrom(src => src.Seat.SeatType.SeatTypeName))
            .ForMember(dest => dest.ReservationCode, opt =>
                opt.MapFrom(src => src.Reservation.ReservationCode))
            .ForMember(dest => dest.MovieTitle, opt =>
                opt.MapFrom(src => src.Showtime.Movie.Title))
            .ForMember(dest => dest.AuditoriumName, opt =>
                opt.MapFrom(src => src.Showtime.Auditorium.Name))
            .ForMember(dest => dest.ShowtimeStartTime, opt =>
                opt.MapFrom(src => src.Showtime.StartTime))
            .ForMember(dest => dest.TotalAmount, opt =>
                opt.MapFrom(src => src.Reservation.TotalAmount));


        // ===============================
        // INVOICE & PAYMENT MAPPINGS
        // (Hóa đơn và thanh toán)
        // ===============================

        CreateMap<Invoice, InvoiceDto>();
        CreateMap<Payment, PaymentDto>();
    }
}
