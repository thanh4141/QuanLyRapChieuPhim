using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyRapChieuPhim_Admin.code;
using System.Data;

namespace QuanLyRapChieuPhim_Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class PhimController : ControllerBase
    {
        [HttpGet("danhsach")]
        public IActionResult DanhSachPhim(string? tukhoa = null, string? theloai = null, bool? kichhoat = null, int trang = 1, int kichthuoc = 20)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("sp_QuanTri_TìmPhim", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TừKhóa", (object?)tukhoa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ThểLoại", (object?)theloai ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KíchHoạt", (object?)kichhoat ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Trang", trang);
                cmd.Parameters.AddWithValue("@KíchThước", kichthuoc);

                using var adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                return Ok(new ResponseModel
                {
                    Success = true,
                    Data = new
                    {
                        Tong = ds.Tables[0].Rows[0][0],
                        DanhSach = ds.Tables[1]
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("them")]
        public IActionResult ThemPhim([FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"INSERT INTO [Phim]([TênPhim],[MôTả],[ThểLoại],[ThờiLượngPhút],[PhânLoại],[NgàyPhátHành],[ĐạoDiễn],[DiễnViên],[KíchHoạt])
                               VALUES(@Ten,@MoTa,@TheLoai,@ThoiLuong,@PhanLoai,@Ngay,@DaoDien,@DienVien,1)";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Ten", (string)body.TênPhim);
                cmd.Parameters.AddWithValue("@MoTa", (string?)body.MôTả ?? "");
                cmd.Parameters.AddWithValue("@TheLoai", (string)body.ThểLoại);
                cmd.Parameters.AddWithValue("@ThoiLuong", (int)body.ThờiLượngPhút);
                cmd.Parameters.AddWithValue("@PhanLoai", (string)body.PhânLoại);
                cmd.Parameters.AddWithValue("@Ngay", (DateTime)body.NgàyPhátHành);
                cmd.Parameters.AddWithValue("@DaoDien", (string)body.ĐạoDiễn);
                cmd.Parameters.AddWithValue("@DienVien", (string?)body.DiễnViên ?? "");

                cmd.ExecuteNonQuery();
                return Ok(new ResponseModel { Success = true, Message = "Thêm phim thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("sua/{id}")]
        public IActionResult SuaPhim(int id, [FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                string sql = @"UPDATE [Phim]
                               SET [TênPhim]=@Ten,[MôTả]=@MoTa,[ThểLoại]=@TheLoai,
                                   [ThờiLượngPhút]=@ThoiLuong,[PhânLoại]=@PhanLoai,
                                   [NgàyPhátHành]=@Ngay,[ĐạoDiễn]=@DaoDien,[DiễnViên]=@DienVien,[SửaLúc]=GETDATE()
                               WHERE Id=@Id AND [ĐãXóa]=0";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Ten", (string)body.TênPhim);
                cmd.Parameters.AddWithValue("@MoTa", (string?)body.MôTả ?? "");
                cmd.Parameters.AddWithValue("@TheLoai", (string)body.ThểLoại);
                cmd.Parameters.AddWithValue("@ThoiLuong", (int)body.ThờiLượngPhút);
                cmd.Parameters.AddWithValue("@PhanLoai", (string)body.PhânLoại);
                cmd.Parameters.AddWithValue("@Ngay", (DateTime)body.NgàyPhátHành);
                cmd.Parameters.AddWithValue("@DaoDien", (string)body.ĐạoDiễn);
                cmd.Parameters.AddWithValue("@DienVien", (string?)body.DiễnViên ?? "");

                int rows = cmd.ExecuteNonQuery();
                return Ok(new ResponseModel { Success = rows > 0, Message = rows > 0 ? "Cập nhật thành công" : "Không tìm thấy bản ghi" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("xoa/{id}")]
        public IActionResult XoaPhim(int id)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("DELETE FROM [Phim] WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                return Ok(new ResponseModel { Success = true, Message = "Xóa (soft-delete) thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }
    }
}
