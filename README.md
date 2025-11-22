# Cinema Booking System

Hệ thống quản lý rạp chiếu phim hoàn chỉnh với .NET 8 Web API và Frontend HTML/CSS/JS thuần.

## 🎯 Tính năng chính

### Backend (.NET 8 Web API)
- ✅ Authentication & Authorization với JWT Bearer Token
- ✅ Role-Based Access Control (RBAC): Admin, Staff, Customer
- ✅ Quản lý phim (CRUD, tìm kiếm, lọc, phân trang)
- ✅ Quản lý phòng chiếu và sơ đồ ghế
- ✅ Quản lý suất chiếu với validation trùng giờ
- ✅ Đặt vé với transaction, chống double-booking
- ✅ Thanh toán (giả lập online)
- ✅ Check-in vé bằng QR code
- ✅ Báo cáo doanh thu (theo ngày, theo phim, top showtimes)
- ✅ Audit logging

### Frontend (HTML/CSS/JS thuần)
- ✅ Giao diện hiện đại, responsive
- ✅ Trang chủ: danh sách phim với tìm kiếm, lọc
- ✅ Chi tiết phim: thông tin + lịch chiếu
- ✅ Đặt vé: chọn ghế, preview giá
- ✅ Thanh toán: xử lý thanh toán
- ✅ Vé của tôi: xem vé đã đặt, QR code
- ✅ Trang Admin: quản lý phim, suất chiếu, báo cáo

## 📁 Cấu trúc Project

```
BAITAPLONQLrapphim/
├── BAITAPLONQLrapphim/          # Web API Project
│   ├── Controllers/             # API Controllers
│   ├── Program.cs               # Startup configuration
│   └── appsettings.json         # Configuration
├── CinemaBooking.BLL/           # Business Logic Layer
│   ├── Services/                # Business services
│   └── Mappings/                # AutoMapper profiles
├── CinemaBooking.DAL/           # Data Access Layer
│   ├── Entities/                # Entity models
│   ├── Repositories/            # Repository pattern
│   └── CinemaDbContext.cs       # DbContext
├── CinemaBooking.Common/        # Shared DTOs & Utilities
│   └── DTOs/                    # Data Transfer Objects
└── frontend/                    # Frontend files
    ├── pages/                   # HTML pages
    ├── css/                     # Stylesheets
    └── js/                      # JavaScript modules
```

## 🚀 Cài đặt và Chạy

### Yêu cầu
- .NET 8 SDK
- SQL Server (SQL Server Express hoặc SQL Server)
- Visual Studio 2022 hoặc VS Code (tùy chọn)

### Bước 1: Cấu hình Database

1. Tạo database `CinemaBookingDbb` trong SQL Server
2. Chạy script SQL trong file schema (đã cung cấp) để tạo các bảng
3. Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=WINDOWS-PC\\SQLEXPRESS;Initial Catalog=CinemaBookingDbb;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"
  }
}
```

### Bước 2: Build và Chạy Backend

```bash
cd BAITAPLONQLrapphim
dotnet restore
dotnet build
dotnet run
```

Backend sẽ chạy tại: `https://localhost:5001` (hoặc port được cấu hình)

### Bước 3: Mở Frontend

1. Mở file `frontend/pages/index.html` trong trình duyệt
2. Hoặc sử dụng Live Server extension trong VS Code
3. Cập nhật `API_BASE_URL` trong `frontend/js/apiClient.js` nếu cần:

```javascript
const API_BASE_URL = "https://localhost:5001/api";
```

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/refresh-token` - Refresh token
- `POST /api/auth/logout` - Đăng xuất
- `POST /api/auth/forgot-password` - Quên mật khẩu
- `POST /api/auth/reset-password` - Đặt lại mật khẩu
- `GET /api/auth/me` - Thông tin user hiện tại

### Movies
- `GET /api/movies` - Danh sách phim (có search, filter, pagination)
- `GET /api/movies/{id}` - Chi tiết phim
- `POST /api/movies` - Tạo phim (Admin)
- `PUT /api/movies/{id}` - Cập nhật phim (Admin)
- `DELETE /api/movies/{id}` - Xóa phim (Admin)
- `GET /api/movies/genres` - Danh sách thể loại

### Showtimes
- `GET /api/showtimes` - Danh sách suất chiếu
- `GET /api/showtimes/{id}` - Chi tiết suất chiếu
- `POST /api/showtimes` - Tạo suất chiếu (Admin)
- `PUT /api/showtimes/{id}` - Cập nhật suất chiếu (Admin)
- `DELETE /api/showtimes/{id}` - Xóa suất chiếu (Admin)

### Bookings
- `POST /api/bookings/preview` - Preview giá vé
- `POST /api/bookings` - Tạo booking
- `GET /api/bookings/my` - Danh sách booking của user
- `POST /api/bookings/{id}/cancel` - Hủy booking

### Tickets
- `GET /api/tickets/my` - Danh sách vé của user
- `GET /api/tickets/qr/{qrCodeData}` - Lấy vé theo QR code
- `POST /api/tickets/checkin` - Check-in vé (Staff/Admin)

### Payments
- `POST /api/payments/create` - Tạo thanh toán
- `GET /api/payments/by-invoice/{invoiceId}` - Lấy payments theo invoice

### Invoices
- `GET /api/invoices/my` - Danh sách hóa đơn của user
- `GET /api/invoices/{id}` - Chi tiết hóa đơn

### Reports (Admin/Staff)
- `GET /api/reports/revenue-by-date?from={date}&to={date}` - Doanh thu theo ngày
- `GET /api/reports/revenue-by-movie?from={date}&to={date}` - Doanh thu theo phim
- `GET /api/reports/top-showtimes?from={date}&to={date}&top={number}` - Top suất chiếu

### Auditoriums & Seats
- `GET /api/auditoriums` - Danh sách phòng chiếu
- `GET /api/auditoriums/{id}/seats?showtimeId={id}` - Sơ đồ ghế

## 🔐 Authentication

Hệ thống sử dụng JWT Bearer Token:

1. Đăng nhập để nhận `token` và `refreshToken`
2. Gửi token trong header: `Authorization: Bearer {token}`
3. Token tự động refresh khi hết hạn (xử lý trong `apiClient.js`)

## 🎨 Frontend Structure

### Pages
- `index.html` - Trang chủ, danh sách phim
- `login.html` - Đăng nhập
- `register.html` - Đăng ký
- `movie-detail.html` - Chi tiết phim + lịch chiếu
- `booking.html` - Chọn ghế, đặt vé
- `payment.html` - Thanh toán
- `my-tickets.html` - Vé của tôi
- `admin-movies.html` - Quản lý phim (Admin)
- `admin-showtimes.html` - Quản lý suất chiếu (Admin)
- `admin-reports.html` - Báo cáo (Admin/Staff)

### JavaScript Modules
- `apiClient.js` - HTTP client với JWT handling
- `auth.js` - Authentication functions
- `movies.js` - Movie operations
- `showtimes.js` - Showtime operations
- `booking.js` - Booking operations
- `tickets.js` - Ticket operations
- `admin.js` - Admin operations
- `reports.js` - Report operations

### CSS
- `base.css` - Reset, typography, utilities
- `layout.css` - Header, footer, grid layout
- `components.css` - Buttons, cards, forms, modals

## 🗄️ Database Schema

Hệ thống sử dụng các bảng chính:
- **Users, Roles, UserRoles** - Authentication & RBAC
- **Permissions, RolePermissions** - Phân quyền chi tiết
- **Movies, Genres, MovieGenres** - Quản lý phim
- **Auditoriums, SeatTypes, Seats** - Phòng chiếu & ghế
- **Showtimes** - Suất chiếu
- **Reservations, Tickets** - Đặt vé
- **Invoices, Payments** - Hóa đơn & thanh toán
- **AuditLogs** - Audit trail

## 🔧 Cấu hình

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong",
    "Issuer": "CinemaBookingAPI",
    "Audience": "CinemaBookingClient",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

## 📝 Ghi chú

- Tất cả DELETE operations là soft delete (IsDeleted = 1)
- Booking sử dụng transaction để đảm bảo không double-booking
- QR code được generate tự động khi tạo ticket
- Payment hiện tại là giả lập (có thể tích hợp gateway thật sau)
- Frontend không dùng framework, chỉ HTML/CSS/JS thuần

## 🐛 Troubleshooting

### Build errors
- Xóa thư mục `obj` và `bin`, sau đó rebuild
- Kiểm tra project references trong `.csproj` files

### Database connection errors
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string trong `appsettings.json`
- Đảm bảo database đã được tạo

### CORS errors
- Backend đã cấu hình CORS cho tất cả origins
- Nếu vẫn lỗi, kiểm tra `Program.cs` - CORS configuration

## 📄 License

Dự án này được tạo cho mục đích học tập và nghiên cứu.

## 👨‍💻 Tác giả

Cinema Booking System - Full Stack Application

