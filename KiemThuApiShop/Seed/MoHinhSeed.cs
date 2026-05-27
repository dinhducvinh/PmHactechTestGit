using System.Text.Json.Serialization;

namespace KiemThuApiShop.Seed;

public sealed class DuLieuSeed
{
    [JsonPropertyName("taikhoan_seed")]
    public List<TaiKhoanSeed> TaiKhoanSeed { get; set; } = [];

    [JsonPropertyName("tk_timkiem_seed")]
    public List<TaiKhoanTimKiemSeed> TaiKhoanTimKiemSeed { get; set; } = [];

    [JsonPropertyName("tk_theodoi_seed")]
    public List<TaiKhoanTheoDoiSeed> TaiKhoanTheoDoiSeed { get; set; } = [];

    [JsonPropertyName("tk_chan_seed")]
    public List<TaiKhoanChanSeed> TaiKhoanChanSeed { get; set; } = [];
}

public sealed class TaiKhoanSeed
{
    [JsonPropertyName("tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("tk_id_server")]
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

public sealed class TaiKhoanTimKiemSeed
{
    [JsonPropertyName("tk_timkiem_seed_id")]
    public int TkTimKiemSeedId { get; set; }

    [JsonPropertyName("tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("tk_id_server")]
    public string? TkId { get; set; }

    [JsonPropertyName("saved_search_id_server")]
    public string? SavedSearchId { get; set; }

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
    public int TdSeedId { get; set; }

    [JsonPropertyName("follower_tk_seed_id")]
    public int TkSeedId { get; set; }

    [JsonPropertyName("follower_tk_id_server")]
    public string? TkId { get; set; }

    [JsonPropertyName("followee_tk_seed_id")]
    public int FolloweeTkSeedId { get; set; }

    [JsonPropertyName("followee_tk_id_server")]
    public string? FolloweeTkId { get; set; }

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

    [JsonPropertyName("blocker_tk_seed_id")]
    public int ChanTkSeedId { get; set; }

    [JsonPropertyName("blocker_tk_id_server")]
    public string? ChanTkId { get; set; }

    [JsonPropertyName("blocked_tk_seed_id")]
    public int BiChanTkSeedId { get; set; }

    [JsonPropertyName("blocked_tk_id_server")]
    public string? BiChanTkId { get; set; }

    [JsonPropertyName("chan_luc")]
    public DateTimeOffset? ChanLuc { get; set; }

    [JsonPropertyName("trang_thai")]
    public string TrangThai { get; set; } = "dang_chan";

    [JsonPropertyName("ghi_chu")]
    public string? GhiChu { get; set; }
}
