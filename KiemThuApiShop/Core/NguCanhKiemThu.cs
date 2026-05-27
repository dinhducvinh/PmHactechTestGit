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
}
