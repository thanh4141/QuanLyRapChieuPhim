using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// =======================
// 1️⃣ Đăng ký dịch vụ cơ bản
// =======================
builder.Services.AddControllers();

// Cho phép Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cho phép gọi API từ FE (React/Vue/Angular,...)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// =======================
// 2️⃣ Build app
// =======================
var app = builder.Build();

// =======================
// 3️⃣ Middleware pipeline
// =======================

// Bật Swagger ở môi trường Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bật HTTPS
app.UseHttpsRedirection();

// Bật CORS
app.UseCors("AllowAll");

// Middleware xác thực (tương lai dùng JWT có thể bật)
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// =======================
// 4️⃣ Chạy ứng dụng
// =======================
app.Run();
