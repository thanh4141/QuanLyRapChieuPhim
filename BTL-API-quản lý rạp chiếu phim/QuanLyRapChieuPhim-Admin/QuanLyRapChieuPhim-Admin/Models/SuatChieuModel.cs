namespace QuanLyRapChieuPhim_Admin.Models
{
    public class SuatChieuModel
    {
        public int Id { get; set; }
        public int PhimId { get; set; }
        public int PhòngChiếuId { get; set; }
        public DateTime NgàyChiếu { get; set; }
        public TimeSpan GiờBắtĐầu { get; set; }
        public TimeSpan GiờKếtThúc { get; set; }
        public decimal GiáCơBản { get; set; }
        public bool KíchHoạt { get; set; }
        public DateTime TạoLúc { get; set; }
        public DateTime SửaLúc { get; set; }

        // Thông tin mở rộng (khi join)
        public string? TênPhim { get; set; }
        public string? TênPhòng { get; set; }
    }
}
