# Hướng dẫn cài đặt API Gateway với Microservices

## 🎯 Kiến trúc

Hệ thống đã được cấu hình theo kiến trúc **Gateway Microservices**:

```
Frontend (HTML/JS)
    ↓
API Gateway (https://localhost:5001) - YARP Reverse Proxy
    ↓
Backend Services (https://localhost:7067)
    ├── Auth Service
    ├── Movie Service
    ├── Booking Service
    ├── Payment Service
    └── Report Service
```

## 📋 Các thành phần

### 1. API Gateway (`GateWay/`)
- **URL**: `https://localhost:5001`
- **Port**: 5001 (HTTPS), 5000 (HTTP)
- **Công nghệ**: YARP (Yet Another Reverse Proxy)
- **Chức năng**:
  - Xác thực JWT token
  - Điều hướng requests đến các services
  - Xử lý CORS
  - Load balancing (có thể mở rộng)

### 2. Backend Services (`BAITAPLONQLrapphim/`)
- **URL**: `https://localhost:7067`
- **Port**: 7067 (HTTPS)
- **Services**:
  - `/api/auth/*` - Authentication Service
  - `/api/movies/*`, `/api/showtimes/*`, `/api/auditoriums/*` - Movie Service
  - `/api/bookings/*`, `/api/tickets/*` - Booking Service
  - `/api/payments/*`, `/api/invoices/*` - Payment Service
  - `/api/reports/*` - Report Service
  - `/api/upload/*` - Upload Service

### 3. Frontend (`frontend/`)
- **URL**: `file://` hoặc web server
- **API Client**: Gọi qua Gateway (`https://localhost:5001/api/*`)

## 🚀 Cách chạy

### Bước 1: Chạy Backend Services

```bash
cd BAITAPLONQLrapphim
dotnet run
```

Backend sẽ chạy tại: `https://localhost:7067`

### Bước 2: Chạy API Gateway

```bash
cd GateWay
dotnet run
```

Gateway sẽ chạy tại: `https://localhost:5001`

### Bước 3: Mở Frontend

Mở file `frontend/pages/index.html` trong trình duyệt hoặc chạy web server.

## 🔧 Cấu hình

### Gateway Configuration (`GateWay/appsettings.json`)

Gateway được cấu hình để route requests đến backend:

- **Auth Routes** → `/api/auth/*` → `auth-cluster` → `https://localhost:7067`
- **Movie Routes** → `/api/movies/*`, `/api/showtimes/*`, `/api/auditoriums/*` → `movie-cluster` → `https://localhost:7067`
- **Booking Routes** → `/api/bookings/*`, `/api/tickets/*` → `booking-cluster` → `https://localhost:7067`
- **Payment Routes** → `/api/payments/*`, `/api/invoices/*` → `payment-cluster` → `https://localhost:7067`
- **Report Routes** → `/api/reports/*` → `report-cluster` → `https://localhost:7067`
- **Upload Routes** → `/api/upload/*` → `upload-cluster` → `https://localhost:7067`

### Frontend Configuration (`frontend/js/apiClient.js`)

Frontend đã được cấu hình để gọi qua Gateway:

```javascript
const API_BASE_URL = "https://localhost:5001/api";
```

## ✅ Kiểm tra

### 1. Health Check Gateway

```bash
curl https://localhost:5001/health
```

Kết quả mong đợi:
```json
{
  "status": "healthy",
  "service": "API Gateway"
}
```

### 2. Test Authentication qua Gateway

```bash
# Đăng ký
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Test123!"}'

# Đăng nhập
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"Test123!"}'
```

### 3. Swagger UI

Truy cập: `https://localhost:5001`

Swagger UI sẽ hiển thị tất cả các endpoints qua Gateway.

## 🔒 Security

- JWT Authentication được xác thực tại Gateway
- Token được forward đến backend services
- CORS được cấu hình để cho phép frontend

## 📝 Lưu ý

1. **Certificate SSL**: Cần chấp nhận self-signed certificate khi test
2. **CORS**: Gateway đã cấu hình CORS để cho phép tất cả origins (có thể tùy chỉnh)
3. **Token Forwarding**: Gateway tự động forward JWT token đến backend services
4. **Error Handling**: Lỗi từ backend sẽ được forward về frontend thông qua gateway

## 🚧 Mở rộng (Future)

Để tách thành microservices thực sự, có thể:

1. Tách Backend thành nhiều service riêng biệt:
   - `AuthService` (Port 7001)
   - `MovieService` (Port 7002)
   - `BookingService` (Port 7003)
   - `PaymentService` (Port 7004)
   - `ReportService` (Port 7005)

2. Cập nhật Gateway routing trong `appsettings.json`:
   ```json
   "Clusters": {
     "auth-cluster": {
       "Destinations": {
         "destination1": {
           "Address": "https://localhost:7001"
         }
       }
     },
     ...
   }
   ```

3. Sử dụng Docker Compose để chạy tất cả services cùng nhau

