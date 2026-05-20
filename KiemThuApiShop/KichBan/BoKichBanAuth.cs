using KiemThuApiShop.Core;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanAuth(List<KichBanApi> ds)
    {
        ThemSignup(ds);
        ThemLogin(ds);
        ThemAuthMe(ds);
        ThemOtpReset(ds);
        ThemResetPassword(ds);
        ThemChangePassword(ds);
        ThemChangeInfoAfterSignup(ds);
        ThemLogout(ds);
    }

    private static void ThemSignup(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-SIGNUP-01", "Auth", "Đăng ký thành công với số điện thoại chưa đăng ký",
            "Lấy số chưa đăng ký từ taikhoan_seed và gọi /auth/signup.",
            ctx =>
            {
                var tk = ctx.YeuCauTaiKhoanChuaDangKy();
                var req = new YeuCauApi(HttpMethod.Post, "/auth/signup", NguCanhKiemThu.TaoBodyDangKy(tk));
                req.Tam["taiKhoan"] = tk;
                req.Tam["matKhauDaDungDeDangKy"] = tk.MatKhauHienTai;
                return Task.FromResult(req);
            },
            Ok,
            DataCoTruong("id"),
            async (response, request, ctx) =>
            {
                var tk = (TaiKhoanSeed)request.Tam["taiKhoan"]!;
                var matKhauDaDungDeDangKy = (string)request.Tam["matKhauDaDungDeDangKy"]!;
                NguCanhKiemThu.CapNhatTaiKhoanSauDangKy(tk, response.Data, matKhauDaDungDeDangKy);
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "AUTH-SIGNUP-02", "Auth", "Đăng ký bằng số điện thoại đã tồn tại",
            "Dùng tài khoản đã đăng ký trong taikhoan_seed.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(HttpMethod.Post, "/auth/signup", NguCanhKiemThu.TaoBodyDangKy(tk));
            },
            Tap("1004", "9996"));

        Them(ds, "AUTH-SIGNUP-03", "Auth", "Đăng ký với số điện thoại sai định dạng",
            "Gửi số điện thoại thiếu đầu 0 hoặc sai độ dài.",
            _ => Req(HttpMethod.Post, "/auth/signup", Obj(("phone_number", "12345"), ("password", "Test123456"), ("uuid", "device-invalid-phone"))),
            SaiGiaTri);

        Them(ds, "AUTH-SIGNUP-04", "Auth", "Đăng ký với mật khẩu sai quy định",
            "Gửi mật khẩu quá ngắn hoặc không đúng format.",
            _ => Req(HttpMethod.Post, "/auth/signup", Obj(("phone_number", "0912345678"), ("password", "123"), ("uuid", "device-invalid-password"))),
            SaiGiaTri);

        Them(ds, "AUTH-SIGNUP-05", "Auth", "Đăng ký thiếu số điện thoại và mật khẩu",
            "Gửi body thiếu các trường bắt buộc.",
            _ => Req(HttpMethod.Post, "/auth/signup", Obj(("uuid", "device-missing-fields"))),
            ThieuThamSo);
    }

    private static void ThemLogin(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-LOGIN-01", "Auth", "Đăng nhập thành công",
            "Dùng số điện thoại đã đăng ký và mật khẩu hiện tại trong taikhoan_seed.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(HttpMethod.Post, "/auth/login", Obj(("phone_number", tk.Sdt), ("password", tk.MatKhauHienTai), ("devtoken", tk.Uuid)));
            },
            Ok,
            DataCoTruong("token"));

        Them(ds, "AUTH-LOGIN-02", "Auth", "Đăng nhập bằng số điện thoại chưa đăng ký",
            "Dùng tài khoản chưa đăng ký từ taikhoan_seed.",
            ctx =>
            {
                var tk = ctx.YeuCauTaiKhoanChuaDangKy();
                return Req(HttpMethod.Post, "/auth/login", Obj(("phone_number", tk.Sdt), ("password", tk.MatKhauHienTai), ("devtoken", tk.Uuid)));
            },
            Tap("9995"));

        Them(ds, "AUTH-LOGIN-03", "Auth", "Đăng nhập với số điện thoại sai định dạng",
            "Gửi input số điện thoại invalid.",
            _ => Req(HttpMethod.Post, "/auth/login", Obj(("phone_number", "abc"), ("password", "Test123456"), ("devtoken", "device-login-invalid-phone"))),
            SaiGiaTri);

        Them(ds, "AUTH-LOGIN-04", "Auth", "Đăng nhập thiếu số điện thoại và mật khẩu",
            "Gửi body thiếu field bắt buộc.",
            _ => Req(HttpMethod.Post, "/auth/login", Obj(("devtoken", "device-login-missing"))),
            ThieuThamSo);
    }

    private static void ThemAuthMe(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-ME-01", "Auth", "Lấy thông tin current user bằng token hợp lệ",
            "Login trước để lấy token rồi gọi GET /auth/me.",
            async ctx => new YeuCauApi(HttpMethod.Get, "/auth/me", token: await ctx.YeuCauTokenHopLeAsync()),
            Ok,
            DataCoTruong("username"));

        Them(ds, "AUTH-ME-02", "Auth", "Lấy current user không gửi Authorization",
            "Gọi API không kèm token.",
            _ => Req(HttpMethod.Get, "/auth/me"),
            SaiToken);

        Them(ds, "AUTH-ME-03", "Auth", "Lấy current user bằng token sai định dạng",
            "Gửi token chuỗi ngẫu nhiên.",
            ctx => Req(HttpMethod.Get, "/auth/me", token: ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "AUTH-ME-04", "Auth", "Lấy current user bằng token hết hạn giả lập",
            "Gửi JWT hết hạn/sai chữ ký.",
            ctx => Req(HttpMethod.Get, "/auth/me", token: ctx.TokenHetHanGiaLap),
            SaiToken);
    }

    private static void ThemOtpReset(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-OTP-CREATE-01", "Auth", "Tạo OTP reset password cho số đã đăng ký",
            "Gọi /auth/create_code_reset_password bằng số đã đăng ký.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(HttpMethod.Post, "/auth/create_code_reset_password", Obj(("phone_number", tk.Sdt)));
            },
            Ok);

        Them(ds, "AUTH-OTP-CREATE-02", "Auth", "Tạo OTP với số điện thoại sai định dạng",
            "Gửi số điện thoại quá ngắn/sai quy tắc.",
            _ => Req(HttpMethod.Post, "/auth/create_code_reset_password", Obj(("phone_number", "012"))),
            SaiGiaTri);

        Them(ds, "AUTH-OTP-CREATE-03", "Auth", "Tạo OTP cho số chưa đăng ký",
            "Dùng tài khoản chưa đăng ký từ seed.",
            ctx =>
            {
                var tk = ctx.YeuCauTaiKhoanChuaDangKy();
                return Req(HttpMethod.Post, "/auth/create_code_reset_password", Obj(("phone_number", tk.Sdt)));
            },
            Tap("9995"));

        Them(ds, "AUTH-OTP-CHECK-01", "Auth", "Xác thực OTP đúng",
            "Tạo OTP trước rồi gọi check bằng mã đúng.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var otp = await ctx.YeuCauOtpResetMatKhauAsync(tk);
                return new YeuCauApi(HttpMethod.Post, "/auth/check_code_reset_password", Obj(("phone_number", tk.Sdt), ("reset_code", otp)));
            },
            Ok);

        Them(ds, "AUTH-OTP-CHECK-02", "Auth", "Xác thực OTP sai",
            "Tạo OTP rồi gửi mã sai.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                _ = await ctx.YeuCauOtpResetMatKhauAsync(tk);
                return new YeuCauApi(HttpMethod.Post, "/auth/check_code_reset_password", Obj(("phone_number", tk.Sdt), ("reset_code", "000000")));
            },
            Tap("9993"));

        Them(ds, "AUTH-OTP-CHECK-03", "Auth", "Dùng lại OTP đã sử dụng",
            "Check OTP đúng một lần rồi dùng lại OTP cũ.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var otp = await ctx.YeuCauOtpResetMatKhauAsync(tk);
                var first = new YeuCauApi(HttpMethod.Post, "/auth/check_code_reset_password", Obj(("phone_number", tk.Sdt), ("reset_code", otp)));
                _ = await ctx.Api.GuiAsync(first);
                return new YeuCauApi(HttpMethod.Post, "/auth/check_code_reset_password", Obj(("phone_number", tk.Sdt), ("reset_code", otp)));
            },
            Tap("9993"));

        Them(ds, "AUTH-OTP-CHECK-04", "Auth", "Xác thực OTP hết hạn hoặc không tồn tại",
            "Gửi mã OTP không còn tồn tại trong server.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(HttpMethod.Post, "/auth/check_code_reset_password", Obj(("phone_number", tk.Sdt), ("reset_code", "123456")));
            },
            Tap("9993"));
    }

    private static void ThemResetPassword(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-RESET-01", "Auth", "Reset password sau khi OTP đã verify",
            "Chuẩn bị OTP verified rồi gọi reset password.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                await ctx.XacThucOtpResetMatKhauAsync(tk);
                var matKhauMoi = $"Reset{DateTimeOffset.Now:HHmmss}";
                var req = new YeuCauApi(HttpMethod.Post, "/auth/reset_password", Obj(("phone_number", tk.Sdt), ("password", matKhauMoi)));
                req.Tam["taiKhoan"] = tk;
                req.Tam["matKhauMoi"] = matKhauMoi;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                await ctx.CapNhatMatKhauAsync((TaiKhoanSeed)request.Tam["taiKhoan"]!, (string)request.Tam["matKhauMoi"]!);
            });

        Them(ds, "AUTH-RESET-02", "Auth", "Reset password với mật khẩu mới sai quy tắc",
            "Chuẩn bị verified flag rồi gửi password invalid.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                await ctx.XacThucOtpResetMatKhauAsync(tk);
                return new YeuCauApi(HttpMethod.Post, "/auth/reset_password", Obj(("phone_number", tk.Sdt), ("password", "123")));
            },
            SaiGiaTri);

        Them(ds, "AUTH-RESET-03", "Auth", "Reset password thiếu mật khẩu mới",
            "Gửi body thiếu password.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(HttpMethod.Post, "/auth/reset_password", Obj(("phone_number", tk.Sdt)));
            },
            ThieuThamSo);
    }

    private static void ThemChangePassword(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-CHANGE-PASS-01", "Auth", "Đổi mật khẩu thành công",
            "Login lấy token, gửi mật khẩu cũ đúng và mật khẩu mới hợp lệ.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var token = await ctx.YeuCauTokenCuaTaiKhoanAsync(tk);
                var matKhauMoi = $"Change{DateTimeOffset.Now:HHmmss}";
                var req = new YeuCauApi(HttpMethod.Post, "/auth/change_password", Obj(("password", tk.MatKhauHienTai), ("new_password", matKhauMoi)), token);
                req.Tam["taiKhoan"] = tk;
                req.Tam["matKhauMoi"] = matKhauMoi;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                await ctx.CapNhatMatKhauAsync((TaiKhoanSeed)request.Tam["taiKhoan"]!, (string)request.Tam["matKhauMoi"]!);
            });

        Them(ds, "AUTH-CHANGE-PASS-02", "Auth", "Đổi mật khẩu với mật khẩu cũ sai",
            "Login lấy token rồi gửi password cũ không đúng.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var token = await ctx.YeuCauTokenCuaTaiKhoanAsync(tk);
                return new YeuCauApi(HttpMethod.Post, "/auth/change_password", Obj(("password", "SaiMatKhau123"), ("new_password", "NewPass123")), token);
            },
            SaiGiaTri);

        Them(ds, "AUTH-CHANGE-PASS-03", "Auth", "Xác nhận mật khẩu không trùng mật khẩu mới",
            "Case này thuộc validation phía client vì đặc tả API không có field confirm password.",
            _ => Req(HttpMethod.Post, "/auth/change_password", Obj()),
            SaiGiaTri,
            lyDoBoQua: "Không tự động hóa ở tầng API: endpoint không có field confirm_password theo đặc tả.");

        Them(ds, "AUTH-CHANGE-PASS-04", "Auth", "Đổi mật khẩu với mật khẩu mới sai định dạng",
            "Gửi new_password quá ngắn/trùng mật khẩu cũ.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var token = await ctx.YeuCauTokenCuaTaiKhoanAsync(tk);
                return new YeuCauApi(HttpMethod.Post, "/auth/change_password", Obj(("password", tk.MatKhauHienTai), ("new_password", "123")), token);
            },
            SaiGiaTri);

        Them(ds, "AUTH-CHANGE-PASS-05", "Auth", "Đổi mật khẩu thiếu field bắt buộc",
            "Gửi body thiếu password hoặc new_password.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/auth/change_password", Obj(("password", "abc")), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);
    }

    private static void ThemChangeInfoAfterSignup(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-CHANGE-INFO-01", "Auth", "Cập nhật username/avatar sau signup thành công",
            "Dùng token hợp lệ và username/avatar hợp lệ.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/auth/change_info_after_signup",
                Obj(("username", $"user_test_{DateTimeOffset.Now:HHmmss}"), ("avatar", "https://example.com/avatar.jpg")),
                await ctx.YeuCauTokenHopLeAsync()),
            Ok);

        Them(ds, "AUTH-CHANGE-INFO-02", "Auth", "Cập nhật info với token trống hoặc quá ngắn",
            "Gửi token invalid theo độ dài.",
            _ => Req(HttpMethod.Post, "/auth/change_info_after_signup", Obj(("username", "user_test")), token: "a"),
            Tap("1004", "9998", "HTTP_401", "HTTP_403"));

        Them(ds, "AUTH-CHANGE-INFO-03", "Auth", "Cập nhật info với token cũ/hết hạn",
            "Gửi JWT hết hạn hoặc không hợp lệ.",
            ctx => Req(HttpMethod.Post, "/auth/change_info_after_signup", Obj(("username", "user_test")), token: ctx.TokenHetHanGiaLap),
            SaiToken);

        Them(ds, "AUTH-CHANGE-INFO-04", "Auth", "Cập nhật info với username không hợp lệ",
            "Gửi username sai format/độ dài.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/auth/change_info_after_signup", Obj(("username", "a")), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "AUTH-CHANGE-INFO-05", "Auth", "Username không hợp lệ đến mức tài khoản bị khóa",
            "Case cần rule khóa tài khoản riêng trên server.",
            _ => Req(HttpMethod.Post, "/auth/change_info_after_signup", Obj()),
            SaiGiaTri,
            lyDoBoQua: "Cần rule/server hook để giả lập tài khoản bị khóa theo username; chưa đủ điều kiện tự động hóa chỉ bằng API public.");

        Them(ds, "AUTH-CHANGE-INFO-06", "Auth", "Avatar không hợp lệ do dung lượng quá lớn",
            "Case thuộc upload/client validation.",
            _ => Req(HttpMethod.Post, "/auth/change_info_after_signup", Obj()),
            SaiGiaTri,
            lyDoBoQua: "Endpoint chỉ nhận URL avatar, không nhận file; case dung lượng ảnh cần kiểm ở upload/client.");
    }

    private static void ThemLogout(List<KichBanApi> ds)
    {
        Them(ds, "AUTH-LOGOUT-01", "Auth", "Đăng xuất thành công",
            "Login lấy token hợp lệ rồi gọi /auth/logout.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/auth/logout", new Dictionary<string, object?>(), await ctx.YeuCauTokenHopLeAsync()),
            Ok);
    }
}
