namespace QuanLyRapChieuPhim_Admin.Models
{
    public class ThanhToanModel
    {
        public int Id { get; set; }
        public int HóaĐơnId { get; set; }
        public string PhươngThức { get; set; } = string.Empty; // TiềnMặt, Thẻ, ChuyểnKhoản, VíĐiệnTử
        public decimal SốTiền { get; set; }
        public string? MãGiaoDịch { get; set; }
        public string TrạngThái { get; set; } = "Chờ";
        public DateTime NgàyThanhToán { get; set; }
        public string? GhiChú { get; set; }
    }
}
