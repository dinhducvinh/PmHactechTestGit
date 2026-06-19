using System.Text.Json.Serialization;

namespace HactechTest.ApiShopTesting.Seed;

public static class YeuCauDuLieuSeed
{
    public const int SoTaiKhoanDaDangKyToiThieu = 100;
    public const decimal SoDuViMacDinhSauSignup = 100000000m;
    public const int SoSavedSearchToiThieu = 2;
    public const int SoQuanHeChinhToiThieu = 2;
    public const int SoQuanHeChinhMucTieu = 3;
    public const int SoTinhThanhToiThieu = 5;
    public const int SoPhuongXaToiThieu = 28;
    public const int SoDiaChiTaiKhoanMucTieu = 50;
    public const int SoDanhMucToiThieu = 1;
    public const int SoSellerSanPhamMucTieu = 20;
    public const int SoSanPhamToiThieuMoiSeller = 3;
    public const int SoSanPhamToiThieu = SoSellerSanPhamMucTieu * SoSanPhamToiThieuMoiSeller;
    public const int SoGioHangMucTieu = 3;
    public const int SoDonHangMucTieu = 2;
    public const int SoLikeSanPhamMucTieu = 5;
    public const int SoTinNhanMucTieu = 5;
    public const int SoThongBaoMucTieu = 2;
}

public enum HangMucChuanBiDuLieuSeed
{
    TaiKhoanDaDangKy,
    TinhThanhPhuongXa,
    DanhMucVaThuongHieu,
    DiaChiTaiKhoan,
    Chan,
    TheoDoi,
    TimKiem,
    SanPham,
    GioHang,
    DonHang,
    LikeSanPham,
    TinNhan,
    ThongBao
}

public sealed class KeHoachChuanBiDuLieuSeed
{
    public KeHoachChuanBiDuLieuSeed(IEnumerable<HangMucChuanBiDuLieuSeed> hangMucCanChuanBi)
    {
        HangMucCanChuanBi = hangMucCanChuanBi.Distinct().ToArray();
    }

    public IReadOnlyList<HangMucChuanBiDuLieuSeed> HangMucCanChuanBi { get; }

    public bool CoHangMucCanChuanBi => HangMucCanChuanBi.Count > 0;

    public bool CanChuanBi(HangMucChuanBiDuLieuSeed hangMuc)
    {
        return HangMucCanChuanBi.Contains(hangMuc);
    }
}

public sealed class BoDuLieuSeed
{
    [JsonPropertyName("taikhoan_seed")]
    public List<TaiKhoanChuaDangKySeed> TaiKhoanChuaDangKySeed { get; set; } = [];

    [JsonPropertyName("taikhoan_signupthanhcong")]
    public List<TaiKhoanSignupThanhCongSeed> TaiKhoanSignupThanhCongSeed { get; set; } = [];

    [JsonPropertyName("wallet_seed")]
    public List<WalletSeed> WalletSeed { get; set; } = [];

    [JsonPropertyName("tk_timkiem_seed")]
    public List<TaiKhoanTimKiemSeed> TaiKhoanTimKiemSeed { get; set; } = [];

    [JsonPropertyName("tk_theodoi_seed")]
    public List<TaiKhoanTheoDoiSeed> TaiKhoanTheoDoiSeed { get; set; } = [];

    [JsonPropertyName("tk_chan_seed")]
    public List<TaiKhoanChanSeed> TaiKhoanChanSeed { get; set; } = [];

    [JsonPropertyName("Provinces_seed")]
    public List<TinhThanhSeed> TinhThanhSeed { get; set; } = [];

    [JsonPropertyName("Wards_seed")]
    public List<PhuongXaSeed> PhuongXaSeed { get; set; } = [];

    [JsonPropertyName("diachi_tk_seed")]
    public List<DiaChiTaiKhoanSeed> DiaChiTaiKhoanSeed { get; set; } = [];

    [JsonPropertyName("danhmuc_seed")]
    public List<DanhMucSeed> DanhMucSeed { get; set; } = [];

    [JsonPropertyName("thuonghieu_seed")]
    public List<ThuongHieuSeed> ThuongHieuSeed { get; set; } = [];

    [JsonPropertyName("sanpham_seed")]
    public List<SanPhamSeed> SanPhamSeed { get; set; } = [];

    [JsonPropertyName("giohang_seed")]
    public List<GioHangSeed> GioHangSeed { get; set; } = [];

    [JsonPropertyName("donhang_seed")]
    public List<DonHangSeed> DonHangSeed { get; set; } = [];

    [JsonPropertyName("donhang_sanpham_seed")]
    public List<DonHangSanPhamSeed> DonHangSanPhamSeed { get; set; } = [];

    [JsonPropertyName("tk_thich_sanpham_seed")]
    public List<TaiKhoanThichSanPhamSeed> TaiKhoanThichSanPhamSeed { get; set; } = [];

    [JsonPropertyName("report_seed")]
    public List<ReportSanPhamSeed> ReportSanPhamSeed { get; set; } = [];

    [JsonPropertyName("tinnhan_seed")]
    public List<TinNhanSeed> TinNhanSeed { get; set; } = [];

    [JsonPropertyName("thongbao_seed")]
    public List<ThongBaoSeed> ThongBaoSeed { get; set; } = [];
}

public sealed class TaiKhoanChuaDangKySeed
{
    [JsonPropertyName("tk_seed_id")]
    public int TaiKhoanSeedId { get; set; }

    [JsonIgnore]
    public int SoThuTu
    {
        get => TaiKhoanSeedId;
        set => TaiKhoanSeedId = value;
    }

    [JsonPropertyName("sdt")]
    public string SoDienThoai { get; set; } = "";

    [JsonPropertyName("mat_khau_hien_tai")]
    public string MatKhauHienTai { get; set; } = "";

    [JsonPropertyName("uuid")]
    public string UuidThietBi { get; set; } = "";

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class TaiKhoanSignupThanhCongSeed
{
    [JsonPropertyName("tk_id_server")]
    public int TaiKhoanIdServer { get; set; }

    [JsonPropertyName("sdt")]
    public string SoDienThoai { get; set; } = "";

    [JsonPropertyName("mat_khau_hien_tai")]
    public string MatKhauHienTai { get; set; } = "";

    [JsonPropertyName("dang_ky_luc")]
    public DateTimeOffset? DangKyLuc { get; set; }

    [JsonPropertyName("doi_mk_luc")]
    public DateTimeOffset? DoiMatKhauLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }

    [JsonIgnore]
    public int SoThuTu { get; set; }

    [JsonIgnore]
    public string UuidThietBi { get; set; } = "";
}

public sealed class WalletSeed
{
    [JsonPropertyName("wallet_id_server")]
    public string WalletIdServer { get; set; } = "";

    [JsonPropertyName("tk_id_server")]
    public int? TaiKhoanIdServer { get; set; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; } = YeuCauDuLieuSeed.SoDuViMacDinhSauSignup;

    [JsonPropertyName("available_balance")]
    public decimal? AvailableBalance { get; set; }

    [JsonPropertyName("pending_balance")]
    public decimal? PendingBalance { get; set; }

    [JsonPropertyName("trang_thai")]
    public string? TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }

    [JsonPropertyName("xac_minh_luc")]
    public DateTimeOffset? XacMinhLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class TaiKhoanTimKiemSeed
{
    [JsonPropertyName("tk_timkiem_seed_id")]
    public int TaiKhoanTimKiemSeedId { get; set; }

    [JsonPropertyName("tk_id_server")]
    public int? TaiKhoanIdServer { get; set; }

    [JsonPropertyName("saved_search_id_server")]
    public int? SavedSearchIdServer { get; set; }

    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = "";

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_luu";

    [JsonPropertyName("tao_boi_test")]
    public bool TaoBoiTest { get; set; } = true;

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }

    [JsonPropertyName("xoa_luc")]
    public DateTimeOffset? XoaLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class TaiKhoanTheoDoiSeed
{
    [JsonPropertyName("td_seed_id")]
    public int TheoDoiSeedId { get; set; }

    [JsonPropertyName("follower_tk_id_server")]
    public int? FollowerTaiKhoanIdServer { get; set; }

    [JsonPropertyName("followee_tk_id_server")]
    public int? FolloweeTaiKhoanIdServer { get; set; }

    [JsonPropertyName("theo_doi_luc")]
    public DateTimeOffset? TheoDoiLuc { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_theo_doi";

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class TaiKhoanChanSeed
{
    [JsonPropertyName("chan_seed_id")]
    public int ChanSeedId { get; set; }

    [JsonPropertyName("blocker_tk_id_server")]
    public int? BlockerTaiKhoanIdServer { get; set; }

    [JsonPropertyName("blocked_tk_id_server")]
    public int? BlockedTaiKhoanIdServer { get; set; }

    [JsonPropertyName("chan_luc")]
    public DateTimeOffset? ChanLuc { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_chan";

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class TinhThanhSeed
{
    [JsonPropertyName("id")]
    public int TinhThanhId { get; set; }

    [JsonPropertyName("name")]
    public string TenTinhThanh { get; set; } = "";
}

public sealed class PhuongXaSeed
{
    [JsonPropertyName("id")]
    public int PhuongXaId { get; set; }

    [JsonPropertyName("name")]
    public string TenPhuongXa { get; set; } = "";

    [JsonPropertyName("provinces_id")]
    public int TinhThanhId { get; set; }
}

public sealed class DiaChiTaiKhoanSeed
{
    [JsonPropertyName("diachi_seed_id")]
    public int DiaChiSeedId { get; set; }

    [JsonPropertyName("tk_id_server")]
    public int? TaiKhoanIdServer { get; set; }

    [JsonPropertyName("diachi_id_server")]
    public int? DiaChiIdServer { get; set; }

    [JsonPropertyName("ward_id")]
    public int PhuongXaId { get; set; }

    [JsonPropertyName("province_id")]
    public int TinhThanhId { get; set; }

    [JsonPropertyName("address")]
    public string DiaChi { get; set; } = "";

    [JsonPropertyName("full_address")]
    public string DiaChiDayDu { get; set; } = "";

    [JsonPropertyName("address_detail")]
    public string DiaChiChiTiet { get; set; } = "";

    [JsonPropertyName("lat")]
    public decimal ViDo { get; set; }

    [JsonPropertyName("lng")]
    public decimal KinhDo { get; set; }

    [JsonPropertyName("receiver_name")]
    public string TenNguoiNhan { get; set; } = "";

    [JsonPropertyName("phone")]
    public string SoDienThoaiNguoiNhan { get; set; } = "";

    [JsonPropertyName("is_default")]
    public bool LaMacDinh { get; set; }

    [JsonPropertyName("muc_dich_seed")]
    public string MucDichSeed { get; set; } = "ca_hai";

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "chua_tao";

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }

    [JsonPropertyName("xac_minh_luc")]
    public DateTimeOffset? XacMinhLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class DanhMucSeed
{
    [JsonPropertyName("dm_id_server")]
    public int DanhMucIdServer { get; set; }

    [JsonPropertyName("ten_danh_muc")]
    public string TenDanhMuc { get; set; } = "";

    [JsonPropertyName("dm_cha_id_server")]
    public int? DanhMucChaIdServer { get; set; }

    [JsonPropertyName("co_danh_muc_con")]
    public bool? CoDanhMucCon { get; set; }

    [JsonPropertyName("co_thuong_hieu")]
    public bool? CoThuongHieu { get; set; }

    [JsonPropertyName("co_kich_co")]
    public bool? CoKichCo { get; set; }

    [JsonPropertyName("yeu_cau_can_nang")]
    public bool? YeuCauCanNang { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("dong_bo_luc")]
    public DateTimeOffset? DongBoLuc { get; set; }
}

public sealed class ThuongHieuSeed
{
    [JsonPropertyName("thuonghieu_id_server")]
    public int ThuongHieuIdServer { get; set; }

    [JsonPropertyName("ten_thuong_hieu")]
    public string TenThuongHieu { get; set; } = "";

    [JsonPropertyName("dm_id_server")]
    public int? DanhMucIdServer { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("dong_bo_luc")]
    public DateTimeOffset? DongBoLuc { get; set; }
}

public sealed class SanPhamSeed
{
    [JsonIgnore]
    public int ThuTuNoiBo { get; set; }

    [JsonPropertyName("sp_id_server")]
    public int? SanPhamIdServer { get; set; }

    [JsonPropertyName("tk_id_server")]
    public int? TaiKhoanIdServer { get; set; }

    [JsonPropertyName("dm_id_server")]
    public int? DanhMucIdServer { get; set; }

    [JsonPropertyName("thuonghieu_id_server")]
    public int? ThuongHieuIdServer { get; set; }

    [JsonPropertyName("ship_from_id_server")]
    public int? DiaChiGuiHangIdServer { get; set; }

    [JsonPropertyName("ten_sp")]
    public string TenSanPham { get; set; } = "";

    [JsonPropertyName("gia")]
    public decimal Gia { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "san_sang";

    [JsonPropertyName("tao_boi_test")]
    public bool TaoBoiTest { get; set; } = true;

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }

    [JsonPropertyName("xac_minh_luc")]
    public DateTimeOffset? XacMinhLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class TaiKhoanThichSanPhamSeed
{
    [JsonPropertyName("thich_seed_id")]
    public int ThichSanPhamSeedId { get; set; }

    [JsonPropertyName("tk_id_server")]
    public int? TaiKhoanIdServer { get; set; }

    [JsonPropertyName("sp_id_server")]
    public int? SanPhamIdServer { get; set; }

    [JsonPropertyName("thich_luc")]
    public DateTimeOffset? ThichLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class ReportSanPhamSeed
{
    [JsonPropertyName("tk_id_server")]
    public int? TaiKhoanIdServer { get; set; }

    [JsonPropertyName("sp_id_server")]
    public int? SanPhamIdServer { get; set; }
}

public sealed class GioHangSeed
{
    [JsonPropertyName("cart_item_id_server")]
    public int? CartItemIdServer { get; set; }

    [JsonPropertyName("buyer_tk_id_server")]
    public int? BuyerTaiKhoanIdServer { get; set; }

    [JsonPropertyName("sp_id_server")]
    public int? SanPhamIdServer { get; set; }

    [JsonPropertyName("so_luong")]
    public int SoLuong { get; set; } = 1;

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_trong_gio";

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }

    [JsonPropertyName("cap_nhat_luc")]
    public DateTimeOffset? CapNhatLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class DonHangSeed
{
    [JsonPropertyName("donhang_id_server")]
    public int? DonHangIdServer { get; set; }

    [JsonPropertyName("buyer_tk_id_server")]
    public int? BuyerTaiKhoanIdServer { get; set; }

    [JsonPropertyName("seller_tk_id_server")]
    public int? SellerTaiKhoanIdServer { get; set; }

    [JsonPropertyName("diachi_id_server")]
    public int? DiaChiIdServer { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "pending";

    [JsonPropertyName("order_source")]
    public int OrderSource { get; set; } = 1;

    [JsonPropertyName("total_price")]
    public decimal? TotalPrice { get; set; }

    [JsonPropertyName("shipping_fee")]
    public decimal? ShippingFee { get; set; }

    [JsonPropertyName("final_price")]
    public decimal? FinalPrice { get; set; }

    [JsonPropertyName("loai_seed")]
    public string? LoaiSeed { get; set; }

    [JsonPropertyName("tao_luc")]
    public DateTimeOffset? TaoLuc { get; set; }

    [JsonPropertyName("cap_nhat_luc")]
    public DateTimeOffset? CapNhatLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class DonHangSanPhamSeed
{
    [JsonPropertyName("donhang_id_server")]
    public int? DonHangIdServer { get; set; }

    [JsonPropertyName("sp_id_server")]
    public int? SanPhamIdServer { get; set; }

    [JsonPropertyName("so_luong")]
    public int SoLuong { get; set; } = 1;

    [JsonPropertyName("don_gia")]
    public decimal? DonGia { get; set; }

    [JsonPropertyName("thanh_tien")]
    public decimal? ThanhTien { get; set; }
}

public sealed class TinNhanSeed
{
    [JsonPropertyName("tn_seed_id")]
    public int TinNhanSeedId { get; set; }

    [JsonPropertyName("conversation_id_server")]
    public int? ConversationIdServer { get; set; }

    [JsonPropertyName("message_id_server")]
    public int? MessageIdServer { get; set; }

    [JsonPropertyName("sender_tk_id_server")]
    public int? SenderTaiKhoanIdServer { get; set; }

    [JsonPropertyName("receiver_tk_id_server")]
    public int? ReceiverTaiKhoanIdServer { get; set; }

    [JsonPropertyName("product_id_server")]
    public int? SanPhamIdServer { get; set; }

    [JsonPropertyName("type_message")]
    public string TypeMessage { get; set; } = "text";

    [JsonPropertyName("noi_dung")]
    public string? NoiDung { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "da_gui";

    [JsonPropertyName("tao_boi_test")]
    public bool TaoBoiTest { get; set; } = true;

    [JsonPropertyName("gui_luc")]
    public DateTimeOffset? GuiLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}

public sealed class ThongBaoSeed
{
    [JsonPropertyName("tb_seed_id")]
    public int ThongBaoSeedId { get; set; }

    [JsonPropertyName("notification_id_server")]
    public int? NotificationIdServer { get; set; }

    [JsonPropertyName("tknhan_id_server")]
    public int? TaiKhoanNhanIdServer { get; set; }

    [JsonPropertyName("tkgui_id_server")]
    public int? TaiKhoanGuiIdServer { get; set; }

    [JsonIgnore]
    public int? TaiKhoanIdServer
    {
        get => TaiKhoanNhanIdServer;
        set => TaiKhoanNhanIdServer = value;
    }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("object_id_server")]
    public int? ObjectIdServer { get; set; }

    [JsonPropertyName("notification_type")]
    public string? NotificationType { get; set; }

    [JsonPropertyName("da_doc")]
    public bool? DaDoc { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_luu";

    [JsonPropertyName("lay_luc")]
    public DateTimeOffset? LayLuc { get; set; }

    [JsonPropertyName("doc_luc")]
    public DateTimeOffset? DocLuc { get; set; }

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}




