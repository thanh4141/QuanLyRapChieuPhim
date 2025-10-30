namespace QuanLyRapChieuPhim_Admin.Models
{
    public class DatChoModel
    {
        public int Id { get; set; }
        public int KháchId { get; set; }
        public int SuấtChiếuId { get; set; }
        public string MãĐặtChỗ { get; set; } = string.Empty;
        public string TrạngThái { get; set; } = string.Empty;
        public decimal TổngTiền { get; set; }
        public DateTime NgàyĐặt { get; set; }
        public DateTime HếtHạn { get; set; }
        public string? GhiChú { get; set; }

        // Mở rộng
        public string? KháchEmail { get; set; }
        public string? Phim { get; set; }
        public string? PhòngChiếu { get; set; }
    }
}
