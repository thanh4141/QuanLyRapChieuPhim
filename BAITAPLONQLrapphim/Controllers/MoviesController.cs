using CinemaBooking.BLL.Services;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BAITAPLONQLrapphim.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MovieDto>>>> GetMovies(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? genreId = null,
        [FromQuery] int? minDuration = null,
        [FromQuery] int? maxDuration = null,
        [FromQuery] decimal? minRating = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var request = new PagedRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _movieService.GetMoviesAsync(request, genreId, minDuration, maxDuration, minRating);
        return Ok(ApiResponse<PagedResult<MovieDto>>.SuccessResult(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MovieDto>>> GetMovie(int id)
    {
        var movie = await _movieService.GetMovieByIdAsync(id);
        if (movie == null)
        {
            return NotFound(ApiResponse<MovieDto>.ErrorResult("Movie not found"));
        }
        return Ok(ApiResponse<MovieDto>.SuccessResult(movie));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<MovieDto>>> CreateMovie([FromBody] CreateMovieRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<MovieDto>.ErrorResult("Thông tin phim không hợp lệ"));
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var movie = await _movieService.CreateMovieAsync(request, userId);
            return Ok(ApiResponse<MovieDto>.SuccessResult(movie, "Thêm phim thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MovieDto>.ErrorResult(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<MovieDto>>> UpdateMovie(int id, [FromBody] UpdateMovieRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<MovieDto>.ErrorResult("Thông tin phim không hợp lệ"));
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var movie = await _movieService.UpdateMovieAsync(id, request, userId);
            if (movie == null)
            {
                return NotFound(ApiResponse<MovieDto>.ErrorResult("Không tìm thấy phim"));
            }
            return Ok(ApiResponse<MovieDto>.SuccessResult(movie, "Cập nhật phim thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MovieDto>.ErrorResult(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMovie(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _movieService.DeleteMovieAsync(id, userId);
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResult("Movie not found"));
        }
        return Ok(ApiResponse<bool>.SuccessResult(result, "Movie deleted successfully"));
    }

    [HttpGet("genres")]
    public async Task<ActionResult<ApiResponse<List<GenreDto>>>> GetGenres()
    {
        var genres = await _movieService.GetAllGenresAsync();
        return Ok(ApiResponse<List<GenreDto>>.SuccessResult(genres));
    }
}

