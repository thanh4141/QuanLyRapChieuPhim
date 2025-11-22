using AutoMapper;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Entities;
using CinemaBooking.DAL.Repositories;

namespace CinemaBooking.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookingPreviewResponse?> GetBookingPreviewAsync(BookingPreviewRequest request)
    {
        if (request == null || request.ShowtimeId <= 0 || request.SeatIds == null || request.SeatIds.Count == 0)
        {
            return null;
        }

        var showtime = await _unitOfWork.Showtimes.GetShowtimeWithDetailsAsync(request.ShowtimeId);
        if (showtime == null || !showtime.IsActive || showtime.IsDeleted)
        {
            return null;
        }

        // Kiểm tra xem suất chiếu đã bắt đầu chưa
        if (showtime.StartTime <= DateTime.UtcNow)
        {
            return null;
        }

        var seats = new List<SeatPriceDto>();
        decimal subTotal = 0;

        foreach (var seatId in request.SeatIds)
        {
            var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
            if (seat == null || seat.AuditoriumId != showtime.AuditoriumId || !seat.IsActive || seat.IsDeleted)
            {
                continue;
            }

            var seatType = await _unitOfWork.SeatTypes.GetByIdAsync(seat.SeatTypeId);
            if (seatType == null || seatType.IsDeleted)
            {
                continue;
            }

            var price = showtime.BaseTicketPrice * seatType.PriceMultiplier;
            subTotal += price;

            seats.Add(new SeatPriceDto
            {
                SeatId = seat.SeatId,
                RowLabel = seat.RowLabel,
                SeatNumber = seat.SeatNumber,
                SeatTypeName = seatType.SeatTypeName,
                Price = price
            });
        }

        if (seats.Count == 0)
        {
            return null;
        }

        var taxAmount = subTotal * 0.1m; // 10% tax
        var totalAmount = subTotal + taxAmount;

        return new BookingPreviewResponse
        {
            Showtime = _mapper.Map<ShowtimeDto>(showtime),
            Seats = seats,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount
        };
    }

    public async Task<BookingDto?> CreateBookingAsync(CreateBookingRequest request, int userId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Xác thực suất chiếu
            var showtime = await _unitOfWork.Showtimes.GetShowtimeWithDetailsAsync(request.ShowtimeId);
            if (showtime == null || !showtime.IsActive || showtime.IsDeleted)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Suất chiếu không tồn tại hoặc đã bị hủy");
            }

            // Kiểm tra xem suất chiếu đã bắt đầu chưa
            if (showtime.StartTime <= DateTime.UtcNow)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Không thể đặt vé cho suất chiếu đã bắt đầu");
            }

            // Kiểm tra xem ghế có còn trống không
            var areAvailable = await _unitOfWork.Bookings.AreSeatsAvailableAsync(request.ShowtimeId, request.SeatIds);
            if (!areAvailable)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Một số ghế đã được đặt. Vui lòng chọn ghế khác.");
            }

            // Xác thực ghế thuộc về phòng chiếu
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null || seat.AuditoriumId != showtime.AuditoriumId || !seat.IsActive || seat.IsDeleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Ghế không hợp lệ hoặc không thuộc phòng chiếu này");
                }
            }

            // Tạo mã đặt chỗ
            var reservationCode = $"RES{DateTime.UtcNow:yyyyMMddHHmmss}{userId}{new Random().Next(1000, 9999)}";

            // Xác định thời gian hết hạn dựa trên phương thức thanh toán
            // Nếu phương thức thanh toán là Tiền mặt hoặc Tại quầy, cho thời gian dài hơn (24 giờ) để thanh toán tại quầy
            DateTime? expiresAt = null;
            if (request.PaymentMethod == "Cash" || request.PaymentMethod == "AtCounter")
            {
                // 24 giờ cho thanh toán tại quầy
                expiresAt = DateTime.UtcNow.AddHours(24);
            }
            else
            {
                // 20 phút cho thanh toán online
                expiresAt = DateTime.UtcNow.AddMinutes(20);
            }

            // Tạo đặt chỗ
            var reservation = new Reservation
            {
                UserId = userId,
                ShowtimeId = request.ShowtimeId,
                ReservationCode = reservationCode,
                Status = "Pending",
                ReservedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                TotalAmount = 0, // Will be calculated
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Bookings.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            decimal totalAmount = 0;
            var tickets = new List<Ticket>();

            // Tạo vé
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null) continue;

                var seatType = await _unitOfWork.SeatTypes.GetByIdAsync(seat.SeatTypeId);
                if (seatType == null) continue;

                var seatPrice = showtime.BaseTicketPrice * seatType.PriceMultiplier;
                totalAmount += seatPrice;

                var qrCodeData = $"TICKET{reservation.ReservationId}{seatId}{DateTime.UtcNow.Ticks}";

                var ticket = new Ticket
                {
                    ReservationId = reservation.ReservationId,
                    ShowtimeId = request.ShowtimeId,
                    SeatId = seatId,
                    SeatPrice = seatPrice,
                    Status = "Booked",
                    QrCodeData = qrCodeData,
                    CreatedAt = DateTime.UtcNow
                };

                tickets.Add(ticket);
                await _unitOfWork.Tickets.AddAsync(ticket);
            }

            // Update reservation total
            reservation.TotalAmount = totalAmount;
            _unitOfWork.Bookings.Update(reservation);

            // Tạo hóa đơn
            var taxAmount = totalAmount * 0.1m;
            var invoiceNumber = $"INV{DateTime.UtcNow:yyyyMMddHHmmss}{reservation.ReservationId}";

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ReservationId = reservation.ReservationId,
                UserId = userId,
                IssuedAt = DateTime.UtcNow,
                SubTotal = totalAmount,
                DiscountAmount = 0,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount + taxAmount,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            // Liên kết vé với hóa đơn
            foreach (var ticket in tickets)
            {
                ticket.InvoiceId = invoice.InvoiceId;
                _unitOfWork.Tickets.Update(ticket);
            }

            await _unitOfWork.CommitTransactionAsync();

            // Ghi nhật ký kiểm toán
            await _unitOfWork.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId,
                ActionName = "CreateBooking",
                EntityName = "Reservation",
                EntityId = reservation.ReservationId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            return await GetBookingByIdAsync(reservation.ReservationId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            // Ném lại với thông báo gốc nếu đã là ngoại lệ nghiệp vụ
            if (ex.Message.Contains("đã được đặt") || ex.Message.Contains("không tồn tại") || ex.Message.Contains("đã bắt đầu"))
            {
                throw;
            }
            throw new Exception("Lỗi khi tạo đặt vé. Vui lòng thử lại.");
        }
    }

    public async Task<BookingDto?> CreateDirectBookingAsync(CreateDirectBookingRequest request, int staffUserId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Tìm người dùng khách hàng nếu có email, nếu không thì dùng tài khoản nhân viên
            int customerUserId = staffUserId;
            if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
            {
                var customer = await _unitOfWork.Users.GetByNormalizedEmailAsync(request.CustomerEmail.ToUpperInvariant());
                if (customer == null || customer.IsDeleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Không tìm thấy khách hàng với email này");
                }
                customerUserId = customer.UserId;
            }

            // Xác thực suất chiếu
            var showtime = await _unitOfWork.Showtimes.GetShowtimeWithDetailsAsync(request.ShowtimeId);
            if (showtime == null || !showtime.IsActive || showtime.IsDeleted)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Suất chiếu không tồn tại hoặc đã bị hủy");
            }

            // Kiểm tra xem suất chiếu đã bắt đầu chưa
            if (showtime.StartTime <= DateTime.UtcNow)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Không thể đặt vé cho suất chiếu đã bắt đầu");
            }

            // Kiểm tra xem ghế có còn trống không
            var areAvailable = await _unitOfWork.Bookings.AreSeatsAvailableAsync(request.ShowtimeId, request.SeatIds);
            if (!areAvailable)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception("Một số ghế đã được đặt. Vui lòng chọn ghế khác.");
            }

            // Xác thực ghế thuộc về phòng chiếu
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null || seat.AuditoriumId != showtime.AuditoriumId || !seat.IsActive || seat.IsDeleted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new Exception("Ghế không hợp lệ hoặc không thuộc phòng chiếu này");
                }
            }

            // Tạo mã đặt chỗ
            var reservationCode = $"RES{DateTime.UtcNow:yyyyMMddHHmmss}{customerUserId}{new Random().Next(1000, 9999)}";

            // Tạo đặt chỗ với trạng thái Đã xác nhận (đặt trực tiếp được thanh toán ngay)
            var reservation = new Reservation
            {
                UserId = customerUserId,
                ShowtimeId = request.ShowtimeId,
                ReservationCode = reservationCode,
                Status = "Confirmed", // Đặt trực tiếp được xác nhận ngay
                ReservedAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow,
                ExpiresAt = null, // Không hết hạn cho đặt trực tiếp
                TotalAmount = 0, // Will be calculated
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Bookings.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            decimal totalAmount = 0;
            var tickets = new List<Ticket>();

            // Tạo vé với trạng thái Đã thanh toán
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null) continue;

                var seatType = await _unitOfWork.SeatTypes.GetByIdAsync(seat.SeatTypeId);
                if (seatType == null) continue;

                var seatPrice = showtime.BaseTicketPrice * seatType.PriceMultiplier;
                totalAmount += seatPrice;

                var qrCodeData = $"TICKET{reservation.ReservationId}{seatId}{DateTime.UtcNow.Ticks}";

                var ticket = new Ticket
                {
                    ReservationId = reservation.ReservationId,
                    ShowtimeId = request.ShowtimeId,
                    SeatId = seatId,
                    SeatPrice = seatPrice,
                    Status = "Paid", // Vé đặt trực tiếp được thanh toán ngay
                    QrCodeData = qrCodeData,
                    CreatedAt = DateTime.UtcNow
                };

                tickets.Add(ticket);
                await _unitOfWork.Tickets.AddAsync(ticket);
            }

            // Update reservation total
            reservation.TotalAmount = totalAmount;
            _unitOfWork.Bookings.Update(reservation);

            // Tạo hóa đơn
            var taxAmount = totalAmount * 0.1m;
            var invoiceNumber = $"INV{DateTime.UtcNow:yyyyMMddHHmmss}{reservation.ReservationId}";

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ReservationId = reservation.ReservationId,
                UserId = customerUserId,
                IssuedAt = DateTime.UtcNow,
                SubTotal = totalAmount,
                DiscountAmount = 0,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount + taxAmount,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            // Liên kết vé với hóa đơn
            foreach (var ticket in tickets)
            {
                ticket.InvoiceId = invoice.InvoiceId;
                _unitOfWork.Tickets.Update(ticket);
            }

            // Tạo thanh toán ngay (đặt trực tiếp được thanh toán tại quầy)
            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                UserId = customerUserId,
                PaymentMethod = request.PaymentMethod ?? "Cash",
                PaymentStatus = "Success",
                Amount = invoice.TotalAmount,
                PaidAt = DateTime.UtcNow,
                TransactionCode = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(10000, 99999)}",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            // Ghi nhật ký kiểm toán
            await _unitOfWork.AuditLogs.AddAsync(new AuditLog
            {
                UserId = staffUserId,
                ActionName = "CreateDirectBooking",
                EntityName = "Reservation",
                EntityId = reservation.ReservationId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();

            return await GetBookingByIdAsync(reservation.ReservationId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            // Ném lại với thông báo gốc nếu đã là ngoại lệ nghiệp vụ
            if (ex.Message.Contains("đã được đặt") || ex.Message.Contains("không tồn tại") || ex.Message.Contains("đã bắt đầu") || ex.Message.Contains("không tìm thấy"))
            {
                throw;
            }
            throw new Exception("Lỗi khi đặt vé trực tiếp. Vui lòng thử lại.");
        }
    }

    public async Task<PagedResult<BookingDto>> GetUserBookingsAsync(int userId, PagedRequest request)
    {
        var skip = (request.PageIndex - 1) * request.PageSize;
        var reservations = await _unitOfWork.Bookings.GetUserReservationsAsync(userId, skip, request.PageSize);
        var totalItems = await _unitOfWork.Bookings.CountUserReservationsAsync(userId);

        var bookingDtos = _mapper.Map<List<BookingDto>>(reservations);

        return new PagedResult<BookingDto>
        {
            Items = bookingDtos,
            TotalItems = totalItems,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    public async Task<bool> CancelBookingAsync(int reservationId, int userId, string? reason = null)
    {
        var reservation = await _unitOfWork.Bookings.GetByIdAsync(reservationId);
        if (reservation == null || reservation.UserId != userId || reservation.IsDeleted)
        {
            return false;
        }

        if (reservation.Status == "Confirmed" || reservation.Status == "Cancelled")
        {
            return false; // Cannot cancel confirmed or already cancelled
        }

        reservation.Status = "Cancelled";
        reservation.CancelledAt = DateTime.UtcNow;
        reservation.CancelReason = reason;
        reservation.UpdatedAt = DateTime.UtcNow;

        // Hủy tất cả vé
        var tickets = await _unitOfWork.Tickets.GetTicketsByReservationAsync(reservationId);
        foreach (var ticket in tickets)
        {
            ticket.Status = "Cancelled";
            ticket.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Tickets.Update(ticket);
        }

        _unitOfWork.Bookings.Update(reservation);
        await _unitOfWork.SaveChangesAsync();

        // Ghi nhật ký kiểm toán
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "CancelBooking",
            EntityName = "Reservation",
            EntityId = reservationId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<BookingDto?> GetBookingByIdAsync(int reservationId)
    {
        var reservation = await _unitOfWork.Bookings.GetReservationWithDetailsAsync(reservationId);
        if (reservation == null) return null;
        return _mapper.Map<BookingDto>(reservation);
    }
}

