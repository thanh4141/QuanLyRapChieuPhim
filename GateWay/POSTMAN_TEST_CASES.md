# 📋 Danh Sách Test Cases cho Postman

## 🎯 Base URL
```
http://localhost:5000/api
```

---

## 📝 Thứ Tự Test (Quan Trọng!)

**Test theo thứ tự này:**
1. ✅ Setup - Tạo tài khoản admin
2. ✅ Authentication - Login để lấy token
3. ✅ Public APIs - Không cần token
4. ✅ Protected APIs - Cần token

---

## 1️⃣ SETUP - Tạo Tài Khoản Admin

### **Test 1.1: Tạo Admin User**
```
POST http://localhost:5000/api/auth/create-admin
Content-Type: application/json

{
  "username": "admin",
  "email": "admin@cinema.com",
  "password": "Admin123!",
  "fullName": "System Administrator"
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Đã tạo tài khoản admin thành công!",
  "data": true
}
```

**Test Cases:**
- ✅ Tạo admin thành công
- ❌ Tạo admin với username đã tồn tại (400 Bad Request)
- ❌ Tạo admin thiếu trường bắt buộc (400 Bad Request)

---

### **Test 1.2: Tạo Customer User (Register)**
```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "customer1",
  "email": "customer1@example.com",
  "password": "Customer123!",
  "fullName": "Customer One",
  "phoneNumber": "0123456789"
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Đăng ký thành công!",
  "data": {
    "token": "...",
    "refreshToken": "...",
    "user": {...}
  }
}
```

**Test Cases:**
- ✅ Đăng ký thành công
- ❌ Đăng ký với username đã tồn tại
- ❌ Đăng ký với email đã tồn tại
- ❌ Đăng ký thiếu trường bắt buộc

---

## 2️⃣ AUTHENTICATION - Login & Token

### **Test 2.1: Login Admin**
```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresAt": "2024-01-01T13:00:00Z",
    "user": {
      "userId": 1,
      "username": "admin",
      "email": "admin@cinema.com",
      "fullName": "System Administrator",
      "roles": ["Admin"]
    }
  }
}
```

**⚠️ QUAN TRỌNG:** Copy `data.token` để dùng cho các test sau!

**Test Cases:**
- ✅ Login thành công với admin
- ✅ Login thành công với customer
- ❌ Login với username sai (401 Unauthorized)
- ❌ Login với password sai (401 Unauthorized)
- ❌ Login thiếu username/password (400 Bad Request)

---

### **Test 2.2: Get Current User (Cần Token)**
```
GET http://localhost:5000/api/auth/me
Authorization: Bearer {token}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "username": "admin",
    "email": "admin@cinema.com",
    "fullName": "System Administrator",
    "roles": ["Admin"]
  }
}
```

**Test Cases:**
- ✅ Lấy thông tin user với token hợp lệ
- ❌ Không có token (401 Unauthorized)
- ❌ Token hết hạn (401 Unauthorized)

---

### **Test 2.3: Refresh Token**
```
POST http://localhost:5000/api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "{refresh_token_from_login}"
}
```

**Expected Response:** `200 OK` - Trả về token mới

**Test Cases:**
- ✅ Refresh token thành công
- ❌ Refresh token không hợp lệ (401 Unauthorized)
- ❌ Refresh token đã hết hạn (401 Unauthorized)

---

### **Test 2.4: Logout (Cần Token)**
```
POST http://localhost:5000/api/auth/logout
Authorization: Bearer {token}
Content-Type: application/json

{
  "refreshToken": "{refresh_token}"
}
```

**Expected Response:** `200 OK`

---

## 3️⃣ PUBLIC APIs - Không Cần Token

### **Test 3.1: Get Movies (Danh sách phim)**
```
GET http://localhost:5000/api/movies?pageIndex=1&pageSize=10
```

**Query Parameters:**
- `pageIndex` (optional): Số trang (default: 1)
- `pageSize` (optional): Số item mỗi trang (default: 10)
- `search` (optional): Tìm kiếm theo tên
- `genreId` (optional): Lọc theo thể loại
- `minDuration` (optional): Thời lượng tối thiểu
- `maxDuration` (optional): Thời lượng tối đa
- `minRating` (optional): Đánh giá tối thiểu
- `sortBy` (optional): Sắp xếp theo (title, releaseDate, rating)
- `sortDirection` (optional): asc/desc (default: asc)

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 10,
    "pageIndex": 1,
    "pageSize": 10,
    "totalPages": 1
  }
}
```

**Test Cases:**
- ✅ Lấy danh sách phim (trang 1)
- ✅ Lấy danh sách phim với pagination
- ✅ Tìm kiếm phim theo tên
- ✅ Lọc phim theo thể loại
- ✅ Sắp xếp phim

---

### **Test 3.2: Get Movie by ID**
```
GET http://localhost:5000/api/movies/{id}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "movieId": 1,
    "title": "Avengers: Endgame",
    "description": "...",
    "duration": 181,
    "releaseDate": "2024-01-01",
    "posterUrl": "...",
    "trailerUrl": "...",
    "rating": 9.5,
    "genre": {...}
  }
}
```

**Test Cases:**
- ✅ Lấy phim hợp lệ
- ❌ Lấy phim không tồn tại (404 Not Found)

---

### **Test 3.3: Get Genres (Danh sách thể loại)**
```
GET http://localhost:5000/api/movies/genres
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "genreId": 1,
      "name": "Action",
      "description": "..."
    }
  ]
}
```

---

### **Test 3.4: Get Showtimes (Danh sách suất chiếu)**
```
GET http://localhost:5000/api/showtimes?pageIndex=1&pageSize=10
```

**Query Parameters:**
- `pageIndex` (optional): Số trang
- `pageSize` (optional): Số item mỗi trang
- `movieId` (optional): Lọc theo phim
- `date` (optional): Lọc theo ngày (yyyy-MM-dd)
- `auditoriumId` (optional): Lọc theo phòng chiếu

**Expected Response:** `200 OK`

**Test Cases:**
- ✅ Lấy danh sách suất chiếu
- ✅ Lọc suất chiếu theo phim
- ✅ Lọc suất chiếu theo ngày

---

### **Test 3.5: Get Showtime by ID**
```
GET http://localhost:5000/api/showtimes/{id}
```

**Expected Response:** `200 OK`

---

### **Test 3.6: Get Auditoriums (Danh sách phòng chiếu)**
```
GET http://localhost:5000/api/auditoriums
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "auditoriumId": 1,
      "name": "Phòng 1",
      "capacity": 100,
      "isActive": true
    }
  ]
}
```

---

### **Test 3.7: Get Seats (Danh sách ghế)**
```
GET http://localhost:5000/api/auditoriums/{id}/seats?showtimeId={showtimeId}
```

**Query Parameters:**
- `showtimeId` (optional): Lọc ghế đã đặt cho suất chiếu

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "seatId": 1,
      "row": "A",
      "number": 1,
      "seatType": "Regular",
      "isBooked": false
    }
  ]
}
```

**Test Cases:**
- ✅ Lấy danh sách ghế
- ✅ Lấy danh sách ghế với trạng thái đặt (showtimeId)

---

## 4️⃣ PROTECTED APIs - Cần Token (Admin)

### **Test 4.1: Create Movie (Cần Admin Token)**
```
POST http://localhost:5000/api/movies
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "title": "Avengers: Endgame",
  "description": "The epic conclusion to the Infinity Saga",
  "duration": 181,
  "releaseDate": "2024-01-01",
  "genreId": 1,
  "posterUrl": "https://example.com/poster.jpg",
  "trailerUrl": "https://youtube.com/watch?v=...",
  "rating": 9.5
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Thêm phim thành công",
  "data": {
    "movieId": 1,
    "title": "Avengers: Endgame",
    ...
  }
}
```

**Test Cases:**
- ✅ Tạo phim thành công (Admin)
- ❌ Tạo phim không có token (401 Unauthorized)
- ❌ Tạo phim với Customer token (403 Forbidden)
- ❌ Tạo phim thiếu trường bắt buộc (400 Bad Request)

---

### **Test 4.2: Update Movie (Cần Admin Token)**
```
PUT http://localhost:5000/api/movies/{id}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "title": "Avengers: Endgame (Updated)",
  "description": "Updated description",
  "duration": 182,
  "releaseDate": "2024-01-01",
  "genreId": 1,
  "rating": 9.6
}
```

**Expected Response:** `200 OK`

**Test Cases:**
- ✅ Cập nhật phim thành công
- ❌ Cập nhật phim không tồn tại (404 Not Found)
- ❌ Cập nhật phim không có quyền (403 Forbidden)

---

### **Test 4.3: Delete Movie (Cần Admin Token)**
```
DELETE http://localhost:5000/api/movies/{id}
Authorization: Bearer {admin_token}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Movie deleted successfully",
  "data": true
}
```

**Test Cases:**
- ✅ Xóa phim thành công
- ❌ Xóa phim không tồn tại (404 Not Found)

---

### **Test 4.4: Create Showtime (Cần Admin Token)**
```
POST http://localhost:5000/api/showtimes
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "movieId": 1,
  "auditoriumId": 1,
  "startTime": "2024-01-01T18:00:00",
  "endTime": "2024-01-01T21:00:00",
  "price": 150000,
  "isActive": true
}
```

**Expected Response:** `200 OK`

**Test Cases:**
- ✅ Tạo suất chiếu thành công
- ❌ Tạo suất chiếu trùng lịch (400 Bad Request)
- ❌ Tạo suất chiếu không có quyền (403 Forbidden)

---

### **Test 4.5: Update Showtime (Cần Admin Token)**
```
PUT http://localhost:5000/api/showtimes/{id}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "movieId": 1,
  "auditoriumId": 1,
  "startTime": "2024-01-01T19:00:00",
  "endTime": "2024-01-01T22:00:00",
  "price": 160000,
  "isActive": true
}
```

---

### **Test 4.6: Delete Showtime (Cần Admin Token)**
```
DELETE http://localhost:5000/api/showtimes/{id}
Authorization: Bearer {admin_token}
```

---

### **Test 4.7: Upload Movie Poster (Cần Admin Token)**
```
POST http://localhost:5000/api/upload/poster
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

file: [chọn file ảnh]
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Upload ảnh thành công",
  "data": "https://localhost:7067/uploads/posters/{filename}"
}
```

**Test Cases:**
- ✅ Upload ảnh thành công (JPG, PNG, GIF, WEBP)
- ❌ Upload file không phải ảnh (400 Bad Request)
- ❌ Upload file quá 5MB (400 Bad Request)
- ❌ Upload không có file (400 Bad Request)

---

## 5️⃣ PROTECTED APIs - Cần Token (User/Customer)

### **Test 5.1: Preview Booking (Cần User Token)**
```
POST http://localhost:5000/api/bookings/preview
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "showtimeId": 1,
  "seatIds": [1, 2, 3]
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "showtime": {...},
    "seats": [...],
    "totalPrice": 450000,
    "serviceFee": 10000,
    "finalPrice": 460000
  }
}
```

**Test Cases:**
- ✅ Xem trước booking thành công
- ❌ Xem trước với ghế đã được đặt (400 Bad Request)
- ❌ Xem trước không có token (401 Unauthorized)

---

### **Test 5.2: Create Booking (Cần User Token)**
```
POST http://localhost:5000/api/bookings
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "showtimeId": 1,
  "seatIds": [1, 2, 3],
  "paymentMethod": "CreditCard"
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Đặt vé thành công! Vui lòng thanh toán trong vòng 15 phút.",
  "data": {
    "bookingId": 1,
    "status": "Pending",
    "totalPrice": 460000,
    "expiresAt": "2024-01-01T18:15:00",
    "invoice": {...}
  }
}
```

**Test Cases:**
- ✅ Tạo booking thành công
- ❌ Tạo booking với ghế đã được đặt (400 Bad Request)
- ❌ Tạo booking không có token (401 Unauthorized)
- ❌ Tạo booking với showtime không tồn tại (400 Bad Request)

---

### **Test 5.3: Get My Bookings (Cần User Token)**
```
GET http://localhost:5000/api/bookings/my?pageIndex=1&pageSize=10
Authorization: Bearer {user_token}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 5,
    "pageIndex": 1,
    "pageSize": 10
  }
}
```

**Test Cases:**
- ✅ Lấy danh sách booking của user
- ✅ Pagination hoạt động đúng
- ❌ Không có token (401 Unauthorized)

---

### **Test 5.4: Cancel Booking (Cần User Token)**
```
POST http://localhost:5000/api/bookings/{id}/cancel
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "reason": "Thay đổi kế hoạch"
}
```

**Lưu ý:** Request body là JSON object với field `reason` (optional).

**Expected Response:** `200 OK`

**Test Cases:**
- ✅ Hủy booking thành công (trước khi thanh toán)
- ❌ Hủy booking đã thanh toán (400 Bad Request)
- ❌ Hủy booking của user khác (400 Bad Request)

---

### **Test 5.5: Create Payment (Cần User Token)**
```
POST http://localhost:5000/api/payments
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "invoiceId": 1,
  "paymentMethod": "CreditCard",
  "amount": 460000,
  "paymentDate": "2024-01-01T18:10:00"
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Thanh toán thành công! Vé đã được xác nhận.",
  "data": {
    "paymentId": 1,
    "status": "Completed",
    "amount": 460000,
    "paymentDate": "2024-01-01T18:10:00"
  }
}
```

**Test Cases:**
- ✅ Thanh toán thành công
- ❌ Thanh toán với invoice không hợp lệ (400 Bad Request)
- ❌ Thanh toán với số tiền sai (400 Bad Request)
- ❌ Thanh toán invoice đã thanh toán (400 Bad Request)

---

### **Test 5.6: Get Payments by Invoice (Cần User Token)**
```
GET http://localhost:5000/api/payments/invoice/{invoiceId}
Authorization: Bearer {user_token}
```

**Expected Response:** `200 OK`

---

### **Test 5.7: Get My Tickets (Cần User Token)**
```
GET http://localhost:5000/api/tickets/my
Authorization: Bearer {user_token}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "ticketId": 1,
      "bookingId": 1,
      "seat": {...},
      "showtime": {...},
      "qrCode": "...",
      "status": "Confirmed"
    }
  ]
}
```

---

## 6️⃣ PROTECTED APIs - Cần Token (Staff/Admin)

### **Test 6.1: Create Direct Booking (Cần Staff/Admin Token)**
```
POST http://localhost:5000/api/bookings/staff/direct
Authorization: Bearer {staff_token}
Content-Type: application/json

{
  "customerUserId": 2,
  "showtimeId": 1,
  "seatIds": [4, 5],
  "paymentMethod": "Cash"
}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "message": "Đặt vé trực tiếp thành công! Vé đã được thanh toán.",
  "data": {...}
}
```

**Test Cases:**
- ✅ Tạo booking trực tiếp thành công (Staff/Admin)
- ❌ Tạo booking trực tiếp với Customer token (403 Forbidden)

---

### **Test 6.2: Create Payment for Customer (Cần Staff/Admin Token)**
```
POST http://localhost:5000/api/payments/staff/for-customer?customerUserId=2
Authorization: Bearer {staff_token}
Content-Type: application/json

{
  "invoiceId": 1,
  "paymentMethod": "Cash",
  "amount": 460000
}
```

**Expected Response:** `200 OK`

---

### **Test 6.3: Get Ticket by QR Code (Cần Staff/Admin Token)**
```
GET http://localhost:5000/api/tickets/qr/{qrCodeData}
Authorization: Bearer {staff_token}
```

**Expected Response:** `200 OK`

**Test Cases:**
- ✅ Lấy vé bằng QR code thành công
- ❌ QR code không hợp lệ (404 Not Found)

---

### **Test 6.4: Check In Ticket (Cần Staff/Admin Token)**
```
POST http://localhost:5000/api/tickets/checkin
Authorization: Bearer {staff_token}
Content-Type: application/json

{
  "qrCodeData": "..."
}
```

**Expected Response:** `200 OK`

---

## 7️⃣ REPORTS APIs - Cần Token (Admin/Staff)

### **Test 7.1: Get Revenue by Date (Cần Admin/Staff Token)**
```
GET http://localhost:5000/api/reports/revenue-by-date?from=2024-01-01&to=2024-01-31
Authorization: Bearer {admin_token}
```

**Expected Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "from": "2024-01-01",
    "to": "2024-01-31",
    "totalRevenue": 50000000,
    "dailyRevenue": [
      {
        "date": "2024-01-01",
        "revenue": 5000000
      }
    ]
  }
}
```

**Test Cases:**
- ✅ Lấy doanh thu theo ngày thành công
- ❌ Không có quyền (403 Forbidden)

---

### **Test 7.2: Get Revenue by Movie (Cần Admin/Staff Token)**
```
GET http://localhost:5000/api/reports/revenue-by-movie?from=2024-01-01&to=2024-01-31
Authorization: Bearer {admin_token}
```

**Expected Response:** `200 OK`

---

### **Test 7.3: Get Top Showtimes (Cần Admin/Staff Token)**
```
GET http://localhost:5000/api/reports/top-showtimes?from=2024-01-01&to=2024-01-31&top=10
Authorization: Bearer {admin_token}
```

**Expected Response:** `200 OK`

---

## 8️⃣ INVOICES APIs - Cần Token

### **Test 8.1: Get Invoice by ID (Cần User Token)**
```
GET http://localhost:5000/api/invoices/{id}
Authorization: Bearer {user_token}
```

**Expected Response:** `200 OK`

---

## 📊 Tổng Kết Test Cases

### **Theo Loại API:**
- ✅ **Public APIs:** 7 endpoints (không cần token)
- ✅ **Auth APIs:** 8 endpoints
- ✅ **Admin APIs:** 7 endpoints (cần Admin token)
- ✅ **User APIs:** 7 endpoints (cần User token)
- ✅ **Staff/Admin APIs:** 4 endpoints (cần Staff/Admin token)
- ✅ **Reports APIs:** 3 endpoints (cần Admin/Staff token)

### **Tổng:** ~36 endpoints

---

## 🎯 Checklist Test

### **Phase 1: Setup**
- [ ] Tạo admin user
- [ ] Tạo customer user (register)
- [ ] Login admin → Lấy admin token
- [ ] Login customer → Lấy customer token

### **Phase 2: Public APIs**
- [ ] Get movies
- [ ] Get movie by ID
- [ ] Get genres
- [ ] Get showtimes
- [ ] Get showtime by ID
- [ ] Get auditoriums
- [ ] Get seats

### **Phase 3: Admin APIs**
- [ ] Create movie
- [ ] Update movie
- [ ] Delete movie
- [ ] Create showtime
- [ ] Update showtime
- [ ] Delete showtime
- [ ] Upload poster

### **Phase 4: User APIs**
- [ ] Preview booking
- [ ] Create booking
- [ ] Get my bookings
- [ ] Cancel booking
- [ ] Create payment
- [ ] Get my tickets

### **Phase 5: Staff/Admin APIs**
- [ ] Create direct booking
- [ ] Create payment for customer
- [ ] Get ticket by QR code
- [ ] Check in ticket

### **Phase 6: Reports**
- [ ] Get revenue by date
- [ ] Get revenue by movie
- [ ] Get top showtimes

---

## 💡 Tips cho Postman

1. **Tạo Environment:**
   - `base_url`: `http://localhost:5000/api`
   - `admin_token`: (sẽ được set sau khi login)
   - `user_token`: (sẽ được set sau khi login)

2. **Tự động lưu token:**
   - Thêm Test Script vào request Login:
   ```javascript
   if (pm.response.code === 200) {
       var jsonData = pm.response.json();
       if (jsonData.success && jsonData.data.token) {
           pm.environment.set("admin_token", jsonData.data.token);
       }
   }
   ```

3. **Dùng {{variable}} trong requests:**
   - URL: `{{base_url}}/movies`
   - Authorization: `Bearer {{admin_token}}`

4. **Tạo Collection:**
   - Tổ chức theo folders: Auth, Movies, Bookings, Payments, Reports, etc.

---

## 🚨 Lưu Ý

1. **Thứ tự test quan trọng:** Phải tạo admin và login trước khi test các API cần token
2. **Token hết hạn:** Token có thời hạn 60 phút, cần login lại
3. **Gateway phải chạy:** Đảm bảo Gateway đang chạy tại `http://localhost:5000`
4. **Backend phải chạy:** Đảm bảo Backend đang chạy tại `https://localhost:7067`

