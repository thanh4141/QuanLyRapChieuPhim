using Cinema.Models;
using Identity_Service.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Identity_Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;

            // --- 1. Cấu hình EF Core DbContext (Sử dụng chuỗi kết nối từ appsettings.json) ---
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Đăng ký DbContext vào hệ thống DI
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(connectionString));

            // --- 2. Cấu hình JWT Authentication/Authorization ---
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Cấu hình kiểm tra cơ bản
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    // Lấy các giá trị từ appsettings.json
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],

                    // Khóa giải mã token
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };
            });

            services.AddAuthorization(); // Thêm dịch vụ Authorization
            services.AddControllers();

            // --- 3. Cấu hình Swagger (Thêm hỗ trợ JWT Bearer) ---
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service API", Version = "v1" });

                // Cấu hình nút Authorize trong Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Nhập token JWT theo format: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // --- 4. Configure the HTTP request pipeline. (Middleware) ---
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                // Swagger UI sẽ truy cập ở URL mặc định: /swagger
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
                    // KHÔNG dùng c.RoutePrefix = string.Empty; để tránh lỗi 404
                });
            }

            app.UseHttpsRedirection();

            // Đặt Authentication TRƯỚC Authorization
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
