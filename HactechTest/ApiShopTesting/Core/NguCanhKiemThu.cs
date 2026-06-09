using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.Core;

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

    public TaiKhoanChuaDangKySeed YeuCauTaiKhoanChuaDangKy()
    {
        return KhoSeed.LayTaiKhoanChuaDangKy()
            ?? throw new BoQuaKiemThuException("Thiếu tài khoản chưa đăng ký trong taikhoan_seed.");
    }

    public async Task<TaiKhoanSignupThanhCongSeed> YeuCauTaiKhoanDaDangKyAsync()
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

    public async Task<string> YeuCauTokenCuaTaiKhoanAsync(TaiKhoanSignupThanhCongSeed taiKhoan) // truyền vào 1 đối tượng
    {
        var token = await DangNhapLayTokenAsync(taiKhoan);
        return token
            ?? throw new BoQuaKiemThuException($"Không lấy được token hợp lệ cho tài khoản seed {taiKhoan.SoThuTu}. Đây là lỗi chuẩn bị test nếu API login chưa sẵn sàng.");
    }

    public async Task<TaiKhoanSignupThanhCongSeed?> DangKyTaiKhoanMoiAsync()
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

        var taiKhoanDaDangKy = TaoTaiKhoanSauDangKy(taiKhoan, response.Data, matKhauDungDeDangKy);
        if (string.IsNullOrWhiteSpace(taiKhoanDaDangKy.TaiKhoanIdServer))
        {
            return null;
        }

        KhoSeed.DuLieu.TaiKhoanChuaDangKySeed.Remove(taiKhoan);
        KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed.Add(taiKhoanDaDangKy);
        await KhoSeed.LuuAsync();
        return taiKhoanDaDangKy;
    }

    public async Task<string?> DangNhapLayTokenAsync(TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        var body = new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.SoDienThoai,
            ["password"] = taiKhoan.MatKhauHienTai,
            ["devtoken"] = taiKhoan.UuidThietBi
        };

        var response = await Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/login", body));
        return response.MaSoSanh == "1000"
            ? TienIchJson.DocChuoi(response.Data, "token", "access_token", "jwt_token")
            : null;
    }

    public async Task<string?> TaoOtpResetMatKhauAsync(TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        var body = new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.SoDienThoai
        };
        var response = await Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/create_code_reset_password", body));

        return response.MaSoSanh == "1000"
            ? TienIchJson.DocChuoi(response.Data, "otp", "reset_code", "code")
            : null;
    }

    public async Task<string> YeuCauOtpResetMatKhauAsync(TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        return await TaoOtpResetMatKhauAsync(taiKhoan)
            ?? throw new BoQuaKiemThuException("Không lấy được OTP từ /auth/create_code_reset_password. Nếu server không trả OTP ra response thì cần chuẩn bị OTP thủ công trong môi trường test.");
    }

    public async Task XacThucOtpResetMatKhauAsync(TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        var otp = await YeuCauOtpResetMatKhauAsync(taiKhoan);
        var body = new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.SoDienThoai,
            ["reset_code"] = otp
        };
        var response = await Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/check_code_reset_password", body));
        if (response.MaSoSanh != "1000")
        {
            throw new BoQuaKiemThuException("Không xác thực được OTP trước khi chạy reset password.");
        }
    }

    public static Dictionary<string, object?> TaoBodyDangKy(TaiKhoanChuaDangKySeed taiKhoan)
    {
        return new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.SoDienThoai,
            ["password"] = taiKhoan.MatKhauHienTai,
            ["uuid"] = taiKhoan.UuidThietBi
        };
    }

    public static TaiKhoanSignupThanhCongSeed TaoTaiKhoanSauDangKy(TaiKhoanChuaDangKySeed taiKhoan, JsonNode? data, string matKhauDaDungDeDangKy)
    {
        return new TaiKhoanSignupThanhCongSeed
        {
            TaiKhoanIdServer = TienIchJson.DocChuoi(data, "id", "user_id", "userId") ?? "",
            SoDienThoai = taiKhoan.SoDienThoai,
            MatKhauHienTai = matKhauDaDungDeDangKy,
            DangKyLuc = DateTimeOffset.Now,
            GhiChu = taiKhoan.GhiChu,
            SoThuTu = taiKhoan.SoThuTu,
            UuidThietBi = taiKhoan.UuidThietBi
        };
    }

    public async Task CapNhatMatKhauAsync(TaiKhoanSignupThanhCongSeed taiKhoan, string matKhauMoi)
    {
        taiKhoan.MatKhauHienTai = matKhauMoi;
        taiKhoan.DoiMatKhauLuc = DateTimeOffset.Now;
        await KhoSeed.LuuAsync();
    }
}




