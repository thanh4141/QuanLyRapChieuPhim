# Cinema Booking API Gateway

API Gateway cho hệ thống đặt vé rạp chiếu phim sử dụng YARP (Yet Another Reverse Proxy).

## 🎯 Chức năng

API Gateway đóng vai trò trung gian giữa Frontend và các Microservices:
- **Routing**: Điều hướng requests đến các services tương ứng
- **Authentication**: Xác thực JWT token trước khi forward
- **Load Balancing**: Có thể mở rộng để cân bằng tải
- **CORS**: Xử lý CORS cho frontend
- **API Documentation**: Swagger UI tích hợp

## 📋 Cấu trúc Routing

Gateway điều hướng requests theo pattern:

| Route Pattern | Cluster | Service Backend |
|--------------|---------|----------------|
| `/api/auth/*` | auth-cluster | Auth Service (https://localhost:7067) |
| `/api/movies/*` | movie-cluster | Movie Service (https://localhost:7067) |
| `/api/showtimes/*` | movie-cluster | Movie Service (https://localhost:7067) |
| `/api/auditoriums/*` | movie-cluster | Movie Service (https://localhost:7067) |
| `/api/bookings/*` | booking-cluster | Booking Service (https://localhost:7067) |
| `/api/tickets/*` | booking-cluster | Booking Service (https://localhost:7067) |
| `/api/payments/*` | payment-cluster | Payment Service (https://localhost:7067) |
| `/api/invoices/*` | payment-cluster | Payment Service (https://localhost:7067) |
| `/api/reports/*` | report-cluster | Report Service (https://localhost:7067) |
| `/api/upload/*` | upload-cluster | Upload Service (https://localhost:7067) |

## 🚀 Cách chạy

1. **Cấu hình Backend Services**:
   - Đảm bảo Backend API đang chạy tại `https://localhost:7067`

2. **Chạy Gateway**:
   ```bash
   cd GateWay
   dotnet run
   ```

3. **Truy cập Gateway**:
   - Gateway URL: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001`
   - Health Check: `https://localhost:5001/health`

## 🔧 Cấu hình

### appsettings.json

Cấu hình routing trong `appsettings.json`:
- `Routes`: Định nghĩa routing patterns
- `Clusters`: Định nghĩa backend destinations
- `Services`: URLs của các microservices

### JWT Settings

Gateway sử dụng cùng JWT settings với backend để xác thực token:
```json
"JwtSettings": {
  "SecretKey": "...",
  "Issuer": "CinemaBookingAPI",
  "Audience": "CinemaBookingClient"
}
```

## 📝 Flow Request

```
Frontend → API Gateway (https://localhost:5001) 
         → Xác thực JWT Token
         → Route đến Service tương ứng
         → Backend Service (https://localhost:7067)
         → Response về Gateway
         → Response về Frontend
```

## 🔒 Security

- JWT Authentication được xác thực tại Gateway
- Token được forward đến backend services
- CORS được cấu hình để cho phép frontend gọi API

## 📦 Dependencies

- `Yarp.ReverseProxy` - Reverse proxy cho .NET
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI documentation

