using KiemThuApiShop.Core;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanUser(List<KichBanApi> ds)
    {
        ThemGetUserInfo(ds);
        ThemSetUserInfo(ds);
    }

    private static void ThemGetUserInfo(List<KichBanApi> ds)
    {
        Them(ds, "USER-GET-INFO-01", "User", "Lấy hồ sơ chính mình bằng token hợp lệ",
            "Token hợp lệ, body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/get_user_info", new Dictionary<string, object?>(), await ctx.YeuCauTokenHopLeAsync()),
            Ok,
            DataCoTruong("id", "username"));

        Them(ds, "USER-GET-INFO-02", "User", "Lấy hồ sơ bằng token không hợp lệ",
            "Gọi API bằng token sai định dạng/hết hạn.",
            ctx => Req(HttpMethod.Post, "/users/get_user_info", new Dictionary<string, object?>(), ctx.TokenSaiDinhDang),
            SaiTokenHoacKhongCoNguoiDung);

        Them(ds, "USER-GET-INFO-03", "User", "User A xem hồ sơ public của User B",
            "Dùng 2 tài khoản đã đăng ký từ taikhoan_seed.",
            async ctx =>
            {
                var cap = ctx.KhoSeed.LayHaiTaiKhoanDaDangKy()
                    ?? throw new BoQuaKiemThuException("Cần ít nhất 2 tài khoản đã đăng ký để chạy case xem hồ sơ user khác.");
                var token = await ctx.YeuCauTokenCuaTaiKhoanAsync(cap.a);
                return new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", SoIdBatBuoc(cap.b.TkId, "tk_id_server user B"))), token);
            },
            Ok,
            DataCoTruong("id", "username"));

        Them(ds, "USER-GET-INFO-04", "User", "Xem user đang được current user theo dõi",
            "Lấy dữ liệu từ tk_theodoi_seed.",
            async ctx =>
            {
                var follow = ctx.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed.FirstOrDefault(x => x.TrangThai == "dang_theo_doi")
                    ?? throw new BoQuaKiemThuException("Thiếu dữ liệu tk_theodoi_seed để kiểm tra data.followed = true.");
                var tk = ctx.KhoSeed.LayTaiKhoanTheoSeedId(follow.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản đang follow trong taikhoan_seed.");
                var token = await ctx.YeuCauTokenCuaTaiKhoanAsync(tk);
                return new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", SoIdBatBuoc(follow.FolloweeTkId, "followee_tk_id_server"))), token);
            },
            Ok,
            DataBoolBang("followed", true));

        Them(ds, "USER-GET-INFO-05", "User", "Xem user đã bị current user chặn",
            "Lấy dữ liệu từ tk_chan_seed.",
            async ctx =>
            {
                var chan = ctx.KhoSeed.DuLieu.TaiKhoanChanSeed.FirstOrDefault(x => x.TrangThai == "dang_chan")
                    ?? throw new BoQuaKiemThuException("Thiếu dữ liệu tk_chan_seed để kiểm tra data.is_blocked = true.");
                var tk = ctx.KhoSeed.LayTaiKhoanTheoSeedId(chan.ChanTkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản chặn trong taikhoan_seed.");
                var token = await ctx.YeuCauTokenCuaTaiKhoanAsync(tk);
                return new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", SoIdBatBuoc(chan.BiChanTkId, "blocked_tk_id_server"))), token);
            },
            OkHoacKhongCoNguoiDung,
            DataBoolBang("is_blocked", true));

        Them(ds, "USER-GET-INFO-06", "User", "Lấy hồ sơ user_id không tồn tại",
            "Gửi user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", 999999999)), await ctx.YeuCauTokenHopLeAsync()),
            Tap("1013", "1000", "9994"));
    }

    private static void ThemSetUserInfo(List<KichBanApi> ds)
    {
        Them(ds, "USER-SET-INFO-01", "User", "Cập nhật hồ sơ với field hợp lệ",
            "Gửi một số field hợp lệ như email, username, status, avatar.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/set_user_info",
                Obj(
                    ("email", $"test_{DateTimeOffset.Now:HHmmss}@example.com"),
                    ("username", $"tester_{DateTimeOffset.Now:HHmmss}"),
                    ("status", "Tài khoản seed dùng để kiểm thử API"),
                    ("avatar", "https://example.com/avatar.jpg"),
                    ("firstname", "Test"),
                    ("lastname", "API"),
                    ("address", "Hà Nội"),
                    ("cover_image", "https://example.com/cover.jpg"),
                    ("cover_image_web", "https://example.com/cover-web.jpg")),
                await ctx.YeuCauTokenHopLeAsync()),
            Ok);

        Them(ds, "USER-SET-INFO-02", "User", "Cập nhật hồ sơ với field sai định dạng",
            "Gửi email sai format hoặc field sai kiểu dữ liệu.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/set_user_info",
                Obj(("email", "sai-email"), ("username", 12345)),
                await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);
    }
}
