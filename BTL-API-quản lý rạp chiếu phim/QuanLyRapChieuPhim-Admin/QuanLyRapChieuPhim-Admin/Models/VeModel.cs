namespace QuanLyRapChieuPhim_Admin.Models
{
    public class VeModel
    {
        public int Id { get; set; }
        public int ĐặtChỗId { get; set; }
        public int GhếId { get; set; }
        public string MãVé { get; set; } = string.Empty;
        public string? MãQR { get; set; }
        public decimal Giá { get; set; }
        public string TrạngThái { get; set; } = string.Empty;
        public DateTime? DùngLúc { get; set; }
        public int? DùngBởi { get; set; }
    }
}
