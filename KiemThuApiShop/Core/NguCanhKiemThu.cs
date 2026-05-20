using System.Text.Json.Nodes;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.Core;

public sealed class NguCanhKiemThu
{
    public NguCanhKiemThu(CauHinhChay cauHinh, MayKhachApi api, KhoDuLieuSeedSqlServer khoSeed)
    {
        CauHinh = cauHinh;
        Api = api;
        KhoSeed = khoSeed;
    }

    public CauHinhChay CauHinh { get; }
    public MayKhachApi Api { get; }
    public KhoDuLieuSeedSqlServer KhoSeed { get; }

    public string TokenSaiDinhDang => "abcxyz-token-sai-dinh-dang";

    public string TokenHetHanGiaLap =>
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEsImV4cCI6MX0.sai-chu-ky";

    public TaiKhoanSeed YeuCauTaiKhoanDaDangKy()
    {
        return KhoSeed.LayTaiKhoanDaDangKy()
            ?? throw new BoQuaKiemThuException("Thiếu tài khoản đã đăng ký trong taikhoan_seed. Hãy bật chuẩn bị dữ liệu hoặc chạy case signup thành công trước.");
    }

    public TaiKhoanSeed YeuCauTaiKhoanChuaDangKy()
    {
        return KhoSeed.LayTaiKhoanChuaDangKy()
            ?? throw new BoQuaKiemThuException("Thiếu tài khoản chưa đăng ký trong taikhoan_seed.");
    }

    public async Task<TaiKhoanSeed> YeuCauTaiKhoanDaDangKyAsync()
    {
        var taiKhoan = KhoSeed.LayTaiKhoanDaDangKy();
        if (taiKhoan is not null)
        {
            return taiKhoan;
        }

        taiKhoan = await DangKyTaiKhoanMoiAsync();
        return taiKhoan
            ?? throw new BoQuaKiemThuException("Không tạo được tài khoản đã đăng ký qua API /auth/signup. Kiểm tra base URL hoặc logic signup.");
    }

    public async Task<string> YeuCauTokenHopLeAsync()
    {
        var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync();
        return await YeuCauTokenCuaTaiKhoanAsync(taiKhoan);
    }

    public async Task<string> YeuCauTokenCuaTaiKhoanAsync(TaiKhoanSeed taiKhoan) // truyền vào 1 đối tượng
    {
        var token = await DangNhapLayTokenAsync(taiKhoan);
        return token
            ?? throw new BoQuaKiemThuException($"Không lấy được token hợp lệ cho tài khoản seed {taiKhoan.TkSeedId}. Đây là lỗi chuẩn bị test nếu API login chưa sẵn sàng.");
    }

    public async Task<TaiKhoanSeed?> DangKyTaiKhoanMoiAsync()
    {
        var taiKhoan = KhoSeed.LayTaiKhoanChuaDangKy();
        if (taiKhoan is null)
        {
            return null;
        }

        var matKhauDungDeDangKy = taiKhoan.MatKhauHienTai;
        var body = TaoBodyDangKy(taiKhoan);
        var response = await Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/signup", body));
        if (response.MaSoSanh != "1000")
        {
            return null;
        }

        CapNhatTaiKhoanSauDangKy(taiKhoan, response.Data, matKhauDungDeDangKy);
        await KhoSeed.LuuAsync();
        return taiKhoan;
    }

    public async Task<string?> DangNhapLayTokenAsync(TaiKhoanSeed taiKhoan)
    {
        var body = new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.Sdt,
            ["password"] = taiKhoan.MatKhauHienTai,
            ["devtoken"] = taiKhoan.Uuid
        };

        var response = await Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/login", body));
        return response.MaSoSanh == "1000"
            ? TienIchJson.DocChuoi(response.Data, "token", "access_token", "jwt_token")
            : null;
    }

    public async Task<string?> TaoOtpResetMatKhauAsync(TaiKhoanSeed taiKhoan)
    {
        var body = new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.Sdt
        };
        var response = await Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/create_code_reset_password", body));

        return response.MaSoSanh == "1000"
            ? TienIchJson.DocChuoi(response.Data, "otp", "reset_code", "code")
            : null;
    }

    public async Task<string> YeuCauOtpResetMatKhauAsync(TaiKhoanSeed taiKhoan)
    {
        return await TaoOtpResetMatKhauAsync(taiKhoan)
            ?? throw new BoQuaKiemThuException("Không lấy được OTP từ /auth/create_code_reset_password. Nếu server không trả OTP ra response thì cần chuẩn bị OTP thủ công trong môi trường test.");
    }

    public async Task XacThucOtpResetMatKhauAsync(TaiKhoanSeed taiKhoan)
    {
        var otp = await YeuCauOtpResetMatKhauAsync(taiKhoan);
        var body = new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.Sdt,
            ["reset_code"] = otp
        };
        var response = await Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/check_code_reset_password", body));
        if (response.MaSoSanh != "1000")
        {
            throw new BoQuaKiemThuException("Không xác thực được OTP trước khi chạy reset password.");
        }
    }

    public static Dictionary<string, object?> TaoBodyDangKy(TaiKhoanSeed taiKhoan)
    {
        return new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.Sdt,
            ["password"] = taiKhoan.MatKhauHienTai,
            ["uuid"] = taiKhoan.Uuid
        };
    }

    public static void CapNhatTaiKhoanSauDangKy(TaiKhoanSeed taiKhoan, JsonNode? data, string matKhauDaDungDeDangKy)
    {
        taiKhoan.TkId = TienIchJson.DocChuoi(data, "id", "user_id", "userId") ?? taiKhoan.TkId;
        taiKhoan.MatKhauHienTai = matKhauDaDungDeDangKy;
        taiKhoan.TrangThaiDangKy = "da_dang_ky";
        taiKhoan.TrangThai = "san_sang";
        taiKhoan.DangKyLuc = DateTimeOffset.Now;
    }

    public async Task CapNhatMatKhauAsync(TaiKhoanSeed taiKhoan, string matKhauMoi)
    {
        taiKhoan.MatKhauHienTai = matKhauMoi;
        taiKhoan.DoiMatKhauLuc = DateTimeOffset.Now;
        await KhoSeed.LuuAsync();
    }

    public SanPhamSeed YeuCauSanPhamBatKy()
    {
        return KhoSeed.LaySanPhamBatKy()
            ?? throw new BoQuaKiemThuException("Thiếu sản phẩm mồi trong sanpham_seed. Hãy bật chuẩn bị dữ liệu hoặc tạo sản phẩm mồi trước.");
    }

    public SanPhamSeed YeuCauSanPhamTheoLoai(string loaiSeed)
    {
        return KhoSeed.LaySanPhamTheoLoai(loaiSeed)
            ?? throw new BoQuaKiemThuException($"Thiếu sản phẩm mồi loại {loaiSeed} trong sanpham_seed.");
    }

    public TaiKhoanSeed YeuCauSellerCuaSanPham(SanPhamSeed sanPham)
    {
        return KhoSeed.LayTaiKhoanTheoSeedId(sanPham.SellerTkSeedId) // lúc này trả về 1 đối tượng(tk)
            ?? throw new BoQuaKiemThuException($"Thiếu seller seed cho sản phẩm {sanPham.SpId}.");
    }

    public TaiKhoanSeed YeuCauTaiKhoanKhacSeller(SanPhamSeed sanPham)
    {
        return KhoSeed.LayTaiKhoanKhac(sanPham.SellerTkSeedId)
            ?? throw new BoQuaKiemThuException("Cần ít nhất 2 tài khoản đã đăng ký để kiểm thử vai trò khác seller.");
    }

    public DanhMucSeed YeuCauDanhMucBatKy()
    {
        return KhoSeed.LayDanhMucBatKy()
            ?? throw new BoQuaKiemThuException("Thiếu danh mục trong danhmuc_seed. Hãy đồng bộ bằng API /api/get_categories.");
    }

    public ThuongHieuSeed? LayThuongHieuNeuCo()
    {
        return KhoSeed.LayThuongHieuBatKy();
    }

    public DiaChiTaiKhoanSeed YeuCauDiaChiCuaTaiKhoan(TaiKhoanSeed taiKhoan)
    {
        return KhoSeed.LayDiaChiTheoTaiKhoan(taiKhoan.TkSeedId)
            ?? throw new BoQuaKiemThuException($"Thiếu địa chỉ mồi cho tài khoản seed {taiKhoan.TkSeedId}.");
    }

    public static Dictionary<string, object?> TaoBodySanPhamHopLe(
        string tieuDe,
        DanhMucSeed danhMuc,
        ThuongHieuSeed? thuongHieu,
        DiaChiTaiKhoanSeed diaChi,
        bool dungVideo = false,
        bool nhieuVariant = false)
    {
        var body = new Dictionary<string, object?>
        {
            ["title"] = tieuDe,
            ["price"] = 150000,
            ["description"] = "Sản phẩm mồi dùng cho kiểm thử API shop",
            ["category_id"] = ChuyenSangIdSo(danhMuc.DmId, "dm_id"),
            ["ship_from_id"] = ChuyenSangIdSo(diaChi.DiaChiId, "diachi_id"),
            ["variants"] = nhieuVariant
                ? new object[]
                {
                    new Dictionary<string, object?> { ["size"] = "M", ["stock"] = 5, ["color"] = "Đen", ["weight"] = 1 },
                    new Dictionary<string, object?> { ["size"] = "L", ["stock"] = 3, ["color"] = "Trắng", ["weight"] = 1 }
                }
                : new object[]
                {
                    new Dictionary<string, object?> { ["size"] = "M", ["stock"] = 5, ["color"] = "Đen", ["weight"] = 1 }
                }
        };

        if (thuongHieu is not null && long.TryParse(thuongHieu.ThuongHieuId, out var brandId))
        {
            body["brand_id"] = brandId;
        }

        if (dungVideo)
        {
            body["videos"] = new object[]
            {
                new Dictionary<string, object?> { ["url"] = "https://example.com/video-test.mp4" }
            };
        }
        else
        {
            body["image_urls"] = new[]
            {
                "https://example.com/image-test-1.jpg",
                "https://example.com/image-test-2.jpg"
            };
        }

        return body;
    }

    private static long ChuyenSangIdSo(string giaTri, string tenTruong)
    {
        if (long.TryParse(giaTri, out var id))
        {
            return id;
        }

        throw new BoQuaKiemThuException($"{tenTruong} trong seed phải là số để tạo body API. Giá trị hiện tại: {giaTri}");
    }
}
