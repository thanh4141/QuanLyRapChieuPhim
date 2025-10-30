using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyRapChieuPhim_Admin.code;
using System.Data;

namespace QuanLyRapChieuPhim_Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class SuatChieuController : ControllerBase
    {
        [HttpGet("danhsach")]
        public IActionResult DanhSach()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"SELECT s.[Id], p.[TênPhim], pc.[TênPhòng], s.[NgàyChiếu], s.[GiờBắtĐầu], s.[GiờKếtThúc], s.[GiáCơBản]
                               FROM [SuấtChiếu] s
                               JOIN [Phim] p ON s.[PhimId]=p.[Id]
                               JOIN [PhòngChiếu] pc ON s.[PhòngChiếuId]=pc.[Id]
                               WHERE s.[ĐãXóa]=0
                               ORDER BY s.[NgàyChiếu] DESC, s.[GiờBắtĐầu]";
                using var cmd = new SqlCommand(sql, conn);
                using var adapter = new SqlDataAdapter(cmd);
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
                string sql = @"INSERT INTO [SuấtChiếu]([PhimId],[PhòngChiếuId],[NgàyChiếu],[GiờBắtĐầu],[GiờKếtThúc],[GiáCơBản])
                               VALUES(@PhimId,@PhongId,@Ngay,@BatDau,@KetThuc,@Gia)";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PhimId", (int)body.PhimId);
                cmd.Parameters.AddWithValue("@PhongId", (int)body.PhòngChiếuId);
                cmd.Parameters.AddWithValue("@Ngay", (DateTime)body.NgàyChiếu);
                cmd.Parameters.AddWithValue("@BatDau", (TimeSpan)TimeSpan.Parse((string)body.GiờBắtĐầu));
                cmd.Parameters.AddWithValue("@KetThuc", (TimeSpan)TimeSpan.Parse((string)body.GiờKếtThúc));
                cmd.Parameters.AddWithValue("@Gia", (decimal)body.GiáCơBản);
                cmd.ExecuteNonQuery();

                return Ok(new ResponseModel { Success = true, Message = "Thêm suất chiếu thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("sua/{id}")]
        public IActionResult Sua(int id, [FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"UPDATE [SuấtChiếu]
                               SET [PhimId]=@PhimId,[PhòngChiếuId]=@PhongId,[NgàyChiếu]=@Ngay,[GiờBắtĐầu]=@BatDau,[GiờKếtThúc]=@KetThuc,[GiáCơBản]=@Gia,[SửaLúc]=GETDATE()
                               WHERE [Id]=@Id AND [ĐãXóa]=0";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@PhimId", (int)body.PhimId);
                cmd.Parameters.AddWithValue("@PhongId", (int)body.PhòngChiếuId);
                cmd.Parameters.AddWithValue("@Ngay", (DateTime)body.NgàyChiếu);
                cmd.Parameters.AddWithValue("@BatDau", (TimeSpan)TimeSpan.Parse((string)body.GiờBắtĐầu));
                cmd.Parameters.AddWithValue("@KetThuc", (TimeSpan)TimeSpan.Parse((string)body.GiờKếtThúc));
                cmd.Parameters.AddWithValue("@Gia", (decimal)body.GiáCơBản);

                int rows = cmd.ExecuteNonQuery();
                return Ok(new ResponseModel { Success = rows > 0, Message = rows > 0 ? "Cập nhật thành công" : "Không tìm thấy bản ghi" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("xoa/{id}")]
        public IActionResult Xoa(int id)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("DELETE FROM [SuấtChiếu] WHERE [Id]=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                return Ok(new ResponseModel { Success = true, Message = "Đã xóa (soft-delete)" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }
    }
}
