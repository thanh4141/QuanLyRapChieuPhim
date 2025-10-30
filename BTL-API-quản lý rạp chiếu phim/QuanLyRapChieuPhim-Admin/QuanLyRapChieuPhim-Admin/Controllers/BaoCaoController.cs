using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyRapChieuPhim_Admin.code;
using System.Data;

namespace QuanLyRapChieuPhim_Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class BaoCaoController : ControllerBase
    {
        [HttpGet("doanhthu")]
        public IActionResult DoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("sp_QuanTri_TổngHợpDoanhThu", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TừNgày", tuNgay);
                cmd.Parameters.AddWithValue("@ĐếnNgày", denNgay);

                using var adapter = new SqlDataAdapter(cmd);
                DataTable dt = new();
                adapter.Fill(dt);

                return Ok(new ResponseModel
                {
                    Success = true,
                    Message = $"Báo cáo doanh thu từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}",
                    Data = dt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("chitiet")]
        public IActionResult ChiTiet()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = "SELECT * FROM [vw_QuanTri_ĐặtChỗChiTiết]";
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
    }
}
