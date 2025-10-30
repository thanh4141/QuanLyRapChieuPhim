using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyRapChieuPhim_Admin.code;
using System.Data;

namespace QuanLyRapChieuPhim_Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class ThanhToanController : ControllerBase
    {
        [HttpGet("danhsach")]
        public IActionResult DanhSach()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"SELECT t.[Id], h.[SốHóaĐơn], t.[PhươngThức], t.[SốTiền], t.[TrạngThái], t.[NgàyThanhToán]
                               FROM [ThanhToán] t
                               JOIN [HóaĐơn] h ON h.[Id] = t.[HóaĐơnId]
                               WHERE t.[ĐãXóa] = 0
                               ORDER BY t.[NgàyThanhToán] DESC";
                using var adapter = new SqlDataAdapter(sql, conn);
                DataTable dt = new();
                adapter.Fill(dt);
                return Ok(new ResponseModel { Success = true, Data = dt });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("them")]
        public IActionResult Them([FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"INSERT INTO [ThanhToán]([HóaĐơnId],[PhươngThức],[SốTiền],[MãGiaoDịch],[TrạngThái],[NgàyThanhToán])
                               VALUES(@HoaDonId,@PhuongThuc,@SoTien,@MaGD,@TrangThai,GETDATE())";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@HoaDonId", (int)body.HóaĐơnId);
                cmd.Parameters.AddWithValue("@PhuongThuc", (string)body.PhươngThức);
                cmd.Parameters.AddWithValue("@SoTien", (decimal)body.SốTiền);
                cmd.Parameters.AddWithValue("@MaGD", (object?)body.MãGiaoDịch ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TrangThai", (string?)body.TrạngThái ?? "Chờ");
                cmd.ExecuteNonQuery();

                return Ok(new ResponseModel { Success = true, Message = "Thêm thanh toán thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }
    }
}
