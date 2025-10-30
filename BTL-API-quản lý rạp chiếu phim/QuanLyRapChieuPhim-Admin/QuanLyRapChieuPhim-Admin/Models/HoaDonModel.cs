namespace QuanLyRapChieuPhim_Admin.Models
{
    public class HoaDonModel
    {
        public int Id { get; set; }
        public int ĐặtChỗId { get; set; }
        public string SốHóaĐơn { get; set; } = string.Empty;
        public decimal TạmTính { get; set; }
        public decimal TiềnThuế { get; set; }
        public decimal TiềnGiảm { get; set; }
        public decimal ThànhTiền { get; set; }
        public string TrạngThái { get; set; } = "Chờ";
        public DateTime NgàyLập { get; set; }
        public DateTime HạnThanhToán { get; set; }
        public DateTime? NgàyThanhToán { get; set; }
        public string? GhiChú { get; set; }
    }
}
