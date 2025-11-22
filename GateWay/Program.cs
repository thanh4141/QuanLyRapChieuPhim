using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Thêm Ocelot configuration file
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Thêm các dịch vụ vào container

// Ocelot API Gateway
builder.Services.AddOcelot(builder.Configuration);

// Xác thực JWT (để gateway xác thực token trước khi forward)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Cinema Booking API Gateway (Ocelot)", 
        Version = "v1",
        Description = "API Gateway cho hệ thống đặt vé rạp chiếu phim sử dụng Ocelot"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng Bearer scheme. Nhập 'Bearer' [dấu cách] rồi nhập token của bạn",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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

// Cấu hình pipeline yêu cầu HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cinema Booking API Gateway v1");
        c.RoutePrefix = string.Empty; // Swagger UI ở root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint (trước Ocelot middleware)
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    service = "API Gateway (Ocelot)",
    version = "1.0.0"
}));

// Ocelot Middleware - Phải đặt sau UseAuthentication và UseAuthorization
await app.UseOcelot();

app.Run();
