namespace QuanLyRapChieuPhim_Admin.Models
{
    public class PhimModel
    {
        public int Id { get; set; }
        public string TênPhim { get; set; } = string.Empty;
        public string? MôTả { get; set; }
        public string ThểLoại { get; set; } = string.Empty;
        public int ThờiLượngPhút { get; set; }
        public string PhânLoại { get; set; } = string.Empty;
        public DateTime NgàyPhátHành { get; set; }
        public string ĐạoDiễn { get; set; } = string.Empty;
        public string? DiễnViên { get; set; }
        public string? ẢnhPoster { get; set; }
        public string? Trailer { get; set; }
        public bool KíchHoạt { get; set; }
        public DateTime TạoLúc { get; set; }
        public DateTime SửaLúc { get; set; }
    }
}
