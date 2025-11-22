# API Gateway với Ocelot

Gateway sử dụng **Ocelot** để điều hướng requests từ Frontend đến Backend Services.

## 📋 Cấu hình Ocelot

File cấu hình chính: `ocelot.json`

### Routes được cấu hình:

1. **Auth Service** (`/api/auth/*`)
   - `/api/auth/register` - Không yêu cầu authentication
   - `/api/auth/login` - Không yêu cầu authentication
   - `/api/auth/refresh-token` - Không yêu cầu authentication
   - `/api/auth/forgot-password` - Không yêu cầu authentication
   - `/api/auth/reset-password` - Không yêu cầu authentication
   - `/api/auth/{everything}` - Yêu cầu JWT Bearer token

2. **Movie Service** (`/api/movies/*`, `/api/showtimes/*`, `/api/auditoriums/*`)
   - Tất cả routes yêu cầu JWT Bearer token

3. **Booking Service** (`/api/bookings/*`, `/api/tickets/*`)
   - Tất cả routes yêu cầu JWT Bearer token

4. **Payment Service** (`/api/payments/*`, `/api/invoices/*`)
   - Tất cả routes yêu cầu JWT Bearer token

5. **Report Service** (`/api/reports/*`)
   - Tất cả routes yêu cầu JWT Bearer token

6. **Upload Service** (`/api/upload/*`)
   - Tất cả routes yêu cầu JWT Bearer token

## 🔧 Cấu hình JWT

Gateway sử dụng JWT Bearer authentication. Cấu hình trong:
- `appsettings.json`: JWT settings
- `Program.cs`: JWT authentication setup

## 🚀 Cách chạy

1. **Chạy Backend Services**:
   ```bash
   cd BAITAPLONQLrapphim
   dotnet run
   ```
   Backend sẽ chạy tại: `https://localhost:7067`

2. **Chạy API Gateway**:
   ```bash
   cd GateWay
   dotnet run
   ```
   Gateway sẽ chạy tại: `https://localhost:5001`

3. **Frontend** sẽ gọi qua Gateway tại `https://localhost:5001/api`

## ✅ Kiểm tra

### Health Check
```bash
curl https://localhost:5001/health
```

### Test Authentication qua Gateway

#### Đăng ký (không cần token)
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Test123!"}'
```

#### Đăng nhập (không cần token)
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"Test123!"}'
```

#### Gọi API có bảo vệ (cần token)
```bash
curl -X GET https://localhost:5001/api/movies \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 📝 Lưu ý

1. **File ocelot.json** phải được copy đến output directory (đã cấu hình trong `.csproj`)
2. **JWT Token** được xác thực tại Gateway trước khi forward
3. **CORS** được cấu hình để cho phép frontend
4. **Routes công khai** (register, login) không yêu cầu authentication
5. **Routes bảo vệ** yêu cầu JWT Bearer token trong header `Authorization`

## 🔒 Security Features

- JWT Authentication tại Gateway
- Token validation trước khi forward
- Route-based authentication
- CORS protection

## 📚 Tài liệu tham khảo

- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [Ocelot GitHub](https://github.com/ThreeMammals/Ocelot)

