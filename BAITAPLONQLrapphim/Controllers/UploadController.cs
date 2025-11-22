using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CinemaBooking.Common;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public UploadController(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost("poster")]
    public async Task<ActionResult<ApiResponse<string>>> UploadPoster(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Không có file được chọn"));
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Chỉ chấp nhận file ảnh: JPG, PNG, GIF, WEBP"));
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Kích thước file không được vượt quá 5MB"));
        }

        try
        {
            // Create wwwroot/uploads/posters directory if not exists
            var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
            }

            var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "posters");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL
            var baseUrl = _configuration["BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            var fileUrl = $"{baseUrl}/uploads/posters/{fileName}";

            return Ok(ApiResponse<string>.SuccessResult(fileUrl, "Upload ảnh thành công"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResult($"Lỗi khi upload ảnh: {ex.Message}"));
        }
    }
}

