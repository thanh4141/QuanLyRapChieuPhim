using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyRapChieuPhim_Admin.code;
using System.Data;

namespace QuanLyRapChieuPhim_Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class HoaDonController : ControllerBase
    {
        [HttpGet("danhsach")]
        public IActionResult DanhSach()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"SELECT h.[Id], h.[SốHóaĐơn], r.[MãĐặtChỗ], h.[TạmTính], h.[TiềnThuế], h.[TiềnGiảm], 
                                      h.[ThànhTiền], h.[TrạngThái], h.[NgàyLập], h.[HạnThanhToán]
                               FROM [HóaĐơn] h
                               JOIN [ĐặtChỗ] r ON r.[Id] = h.[ĐặtChỗId]
                               WHERE h.[ĐãXóa] = 0
                               ORDER BY h.[NgàyLập] DESC";
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

                // Tạo số hóa đơn tự động
                string maHD = "";
                using (var cmdGen = new SqlCommand("dbo.[TạoSốHóaĐơn]", conn))
                {
                    cmdGen.CommandType = CommandType.StoredProcedure;
                    using var reader = cmdGen.ExecuteReader();
                    if (reader.Read())
                        maHD = reader["SốHóaĐơn"].ToString()!;
                }

                string sql = @"INSERT INTO [HóaĐơn]([ĐặtChỗId],[SốHóaĐơn],[TạmTính],[TiềnThuế],[TiềnGiảm],[ThànhTiền],[TrạngThái],[HạnThanhToán])
                               VALUES(@DatChoId,@SoHD,@TamTinh,@Thue,@Giam,@ThanhTien,@TrangThai,@HanTT)";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DatChoId", (int)body.ĐặtChỗId);
                cmd.Parameters.AddWithValue("@SoHD", maHD);
                cmd.Parameters.AddWithValue("@TamTinh", (decimal)body.TạmTính);
                cmd.Parameters.AddWithValue("@Thue", (decimal?)body.TiềnThuế ?? 0);
                cmd.Parameters.AddWithValue("@Giam", (decimal?)body.TiềnGiảm ?? 0);
                cmd.Parameters.AddWithValue("@ThanhTien", (decimal)body.ThànhTiền);
                cmd.Parameters.AddWithValue("@TrangThai", (string?)body.TrạngThái ?? "Chờ");
                cmd.Parameters.AddWithValue("@HanTT", (DateTime)body.HạnThanhToán);
                cmd.ExecuteNonQuery();

                return Ok(new ResponseModel { Success = true, Message = "Thêm hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("capnhat/{id}")]
        public IActionResult CapNhatTrangThai(int id, [FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"UPDATE [HóaĐơn]
                               SET [TrạngThái] = @TrangThai, [NgàyThanhToán] = @Ngay, [SửaLúc] = GETDATE()
                               WHERE [Id] = @Id AND [ĐãXóa] = 0";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@TrangThai", (string)body.TrạngThái);
                cmd.Parameters.AddWithValue("@Ngay", DateTime.Now);
                cmd.ExecuteNonQuery();

                return Ok(new ResponseModel { Success = true, Message = "Cập nhật trạng thái hóa đơn thành công" });
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
                using var cmd = new SqlCommand("DELETE FROM [HóaĐơn] WHERE [Id]=@Id", conn);
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
