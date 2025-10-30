using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyRapChieuPhim_Admin.code;
using System.Data;

namespace QuanLyRapChieuPhim_Admin.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class VeController : ControllerBase
    {
        [HttpPost("checkin")]
        public IActionResult CheckIn([FromBody] dynamic body)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = new SqlCommand("sp_QuanTri_CheckInVé", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MãVé", (string)body.MãVé);

                using var adapter = new SqlDataAdapter(cmd);
                DataTable dt = new();
                adapter.Fill(dt);

                return Ok(new ResponseModel { Success = true, Message = "Check-in thành công", Data = dt });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Success = false, Message = ex.Message });
            }
        }
    }
}
