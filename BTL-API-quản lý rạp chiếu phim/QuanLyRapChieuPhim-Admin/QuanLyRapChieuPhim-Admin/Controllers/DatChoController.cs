using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyRapChieuPhim_Admin.code;
using System.Data;

namespace QuanLyRapChieuPhim_Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class DatChoController : ControllerBase
    {
        [HttpPost("datghe")]
        public IActionResult DatGhe([FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("sp_QuanTri_ĐặtGhếAnToàn", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@KháchId", (int)body.KháchId);
                cmd.Parameters.AddWithValue("@SuấtChiếuId", (int)body.SuấtChiếuId);
                cmd.Parameters.AddWithValue("@DanhSáchGhếCsv", (string)body.DanhSáchGhếCsv);
                cmd.Parameters.AddWithValue("@GhiChú", (object?)body.GhiChú ?? DBNull.Value);

                using var adapter = new SqlDataAdapter(cmd);
                DataSet ds = new();
                adapter.Fill(ds);

                return Ok(new ResponseModel
                {
                    Success = true,
                    Message = "Đặt ghế thành công",
                    Data = new { DatCho = ds.Tables[0], Ve = ds.Tables[1] }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("xacnhan/{id}")]
        public IActionResult XacNhan(int id)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("sp_QuanTri_XácNhậnĐặtChỗ", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ĐặtChỗId", id);

                using var adapter = new SqlDataAdapter(cmd);
                DataTable dt = new();
                adapter.Fill(dt);

                return Ok(new ResponseModel { Success = true, Message = "Đã xác nhận đặt chỗ", Data = dt });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("huy/{id}")]
        public IActionResult Huy(int id, [FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("sp_QuanTri_HủyĐặtChỗ", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ĐặtChỗId", id);
                cmd.Parameters.AddWithValue("@LýDo", (object?)body.LýDo ?? DBNull.Value);

                using var adapter = new SqlDataAdapter(cmd);
                DataTable dt = new();
                adapter.Fill(dt);

                return Ok(new ResponseModel { Success = true, Message = "Đã hủy đặt chỗ", Data = dt });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }
    }
}
