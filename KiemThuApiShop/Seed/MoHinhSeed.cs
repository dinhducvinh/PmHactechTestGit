using System.Text.Json.Serialization;

namespace KiemThuApiShop.Seed;

public sealed class DuLieuSeed
{
    [JsonPropertyName("taikhoan_seed")]
    public List<TaiKhoanSeed> TaiKhoanSeed { get; set; } = [];

    [JsonPropertyName("danhmuc_seed")]
    public List<DanhMucSeed> DanhMucSeed { get; set; } = [];

    [JsonPropertyName("thuonghieu_seed")]
    public List<ThuongHieuSeed> ThuongHieuSeed { get; set; } = [];

    [JsonPropertyName("diachitk_seed")]
    public List<DiaChiTaiKhoanSeed> DiaChiTaiKhoanSeed { get; set; } = [];

    [JsonPropertyName("sanpham_seed")]
    public List<SanPhamSeed> SanPhamSeed { get; set; } = [];

    [JsonPropertyName("binhluan_sp_seed")]
    public List<BinhLuanSanPhamSeed> BinhLuanSanPhamSeed { get; set; } = [];

    [JsonPropertyName("tk_thich_sanpham_seed")]
    public List<TaiKhoanThichSanPhamSeed> TaiKhoanThichSanPhamSeed { get; set; } = [];

    [JsonPropertyName("tk_theodoi_seed")]
    public List<TaiKhoanTheoDoiSeed> TaiKhoanTheoDoiSeed { get; set; } = [];

    [JsonPropertyName("tk_chan_seed")]
    public List<TaiKhoanChanSeed> TaiKhoanChanSeed { get; set; } = [];
}

public sealed class TaiKhoanSeed
{
    [JsonPropertyName("tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("tk_id")]
    public string? TkId { get; set; }

    [JsonPropertyName("sdt")]
    public string Sdt { get; set; } = "";

    [JsonPropertyName("mat_khau_hien_tai")]
    public string MatKhauHienTai { get; set; } = "";

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = "";

    [JsonPropertyName("trang_thai_dang_ky")]
    public string TrangThaiDangKy { get; set; } = "chua_dang_ky";

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("dang_ky_luc")]
    public DateTimeOffset? DangKyLuc { get; set; }

    [JsonPropertyName("doi_mk_luc")]
    public DateTimeOffset? DoiMatKhauLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class DanhMucSeed
{
    [JsonPropertyName("dm_id")]
    public string DmId { get; set; } = "";

    [JsonPropertyName("ten_danh_muc")]
    public string TenDanhMuc { get; set; } = "";

    [JsonPropertyName("dm_cha_id")]
    public string? DmChaId { get; set; }

    [JsonPropertyName("co_danh_muc_con")]
    public bool CoDanhMucCon { get; set; }

    [JsonPropertyName("co_thuong_hieu")]
    public bool CoThuongHieu { get; set; }

    [JsonPropertyName("co_kich_co")]
    public bool CoKichCo { get; set; }

    [JsonPropertyName("yeu_cau_can_nang")]
    public bool YeuCauCanNang { get; set; }

    [JsonPropertyName("sort")]
    public int? Sort { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("dong_bo_luc")]
    public DateTimeOffset? DongBoLuc { get; set; }
}

public sealed class ThuongHieuSeed
{
    [JsonPropertyName("thuonghieu_id")]
    public string ThuongHieuId { get; set; } = "";

    [JsonPropertyName("ten_thuong_hieu")]
    public string TenThuongHieu { get; set; } = "";

    [JsonPropertyName("dm_id")]
    public string? DmId { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("dong_bo_luc")]
    public DateTimeOffset? DongBoLuc { get; set; }
}

public sealed class DiaChiTaiKhoanSeed
{
    [JsonPropertyName("tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("tk_id")]
    public string? TkId { get; set; }

    [JsonPropertyName("diachi_id")]
    public string DiaChiId { get; set; } = "";

    [JsonPropertyName("ten_nguoi_nhan")]
    public string TenNguoiNhan { get; set; } = "";

    [JsonPropertyName("sdt")]
    public string Sdt { get; set; } = "";

    [JsonPropertyName("diachi_daydu")]
    public string DiaChiDayDu { get; set; } = "";

    [JsonPropertyName("phuong_xa_id")]
    public int PhuongXaId { get; set; } = 1;

    [JsonPropertyName("vi_do")]
    public decimal ViDo { get; set; } = 21.0278m;

    [JsonPropertyName("kinh_do")]
    public decimal KinhDo { get; set; } = 105.8342m;

    [JsonPropertyName("mac_dinh")]
    public bool MacDinh { get; set; } = true;

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }
}

public sealed class SanPhamSeed
{
    [JsonPropertyName("sp_seed_id")]
    public int SpSeedId { get; set; }

    [JsonPropertyName("sp_id")]
    public string SpId { get; set; } = "";

    [JsonPropertyName("seller_tk_seed_id")]
    public int SellerTkSeedId { get; set; }

    [JsonPropertyName("seller_tk_id")]
    public string? SellerTkId { get; set; }

    [JsonPropertyName("dm_id")]
    public string DmId { get; set; } = "";

    [JsonPropertyName("thuonghieu_id")]
    public string? ThuongHieuId { get; set; }

    [JsonPropertyName("diachi_id")]
    public string? DiaChiId { get; set; }

    [JsonPropertyName("ten_sp")]
    public string TenSp { get; set; } = "";

    [JsonPropertyName("gia")]
    public decimal Gia { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("loai_seed")]
    public string LoaiSeed { get; set; } = "mac_dinh";

    [JsonPropertyName("tao_boi_test")]
    public bool TaoBoiTest { get; set; } = true;

    [JsonPropertyName("xac_minh_luc")]
    public DateTimeOffset? XacMinhLuc { get; set; }
}

public sealed class BinhLuanSanPhamSeed
{
    [JsonPropertyName("bl_seed_id")]
    public int BlSeedId { get; set; }

    [JsonPropertyName("sp_seed_id")]
    public int SpSeedId { get; set; }

    [JsonPropertyName("sp_id")]
    public string SpId { get; set; } = "";

    [JsonPropertyName("tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("tk_id")]
    public string? TkId { get; set; }

    [JsonPropertyName("bl_id")]
    public string? BlId { get; set; }

    [JsonPropertyName("noi_dung")]
    public string NoiDung { get; set; } = "";

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class TaiKhoanThichSanPhamSeed
{
    [JsonPropertyName("tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("tk_id")]
    public string? TkId { get; set; }

    [JsonPropertyName("sp_seed_id")]
    public int SpSeedId { get; set; }

    [JsonPropertyName("sp_id")]
    public string SpId { get; set; } = "";

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_like";

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }
}

public sealed class TaiKhoanTheoDoiSeed
{
    [JsonPropertyName("tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("tk_id")]
    public string? TkId { get; set; }

    [JsonPropertyName("followee_tk_seed_id")]
    public int FolloweeTkSeedId { get; set; }

    [JsonPropertyName("followee_tk_id")]
    public string? FolloweeTkId { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_theo_doi";
}

public sealed class TaiKhoanChanSeed
{
    [JsonPropertyName("chan_tk_seed_id")]
    public int ChanTkSeedId { get; set; }

    [JsonPropertyName("chan_tk_id")]
    public string? ChanTkId { get; set; }

    [JsonPropertyName("bi_chan_tk_seed_id")]
    public int BiChanTkSeedId { get; set; }

    [JsonPropertyName("bi_chan_tk_id")]
    public string? BiChanTkId { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_chan";
}
