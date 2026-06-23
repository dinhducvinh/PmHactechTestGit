using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanUser(List<KichBanApi> ds)
    {
        ThemLayThongTinNguoiDung(ds);
        ThemCapNhatThongTinNguoiDung(ds);
    }

    private static void ThemLayThongTinNguoiDung(List<KichBanApi> ds)
    {
        Them(ds, "USER-GET-INFO-01", "User", "Lấy hồ sơ chính mình bằng token hợp lệ",
            "Lấy một tài khoản đã đăng ký, đăng nhập lấy token rồi gọi API với body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/get_user_info", new Dictionary<string, object?>(), await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            KiemTraHoSoChinhMinh());

        Them(ds, "USER-GET-INFO-02", "User", "Lấy hồ sơ bằng token không hợp lệ",
            "Gọi API bằng token sai định dạng/hết hạn.",
            ctx => Req(HttpMethod.Post, "/users/get_user_info", new Dictionary<string, object?>(), ctx.TokenSaiDinhDang),
            Tap("9998", "HTTP_401", "HTTP_403", "1013", "1005"));

        Them(ds, "USER-GET-INFO-03", "User", "User A xem hồ sơ public của User B",
            "Chọn 2 tài khoản đã đăng ký, loại các cặp đang block nhau ở cả hai chiều.",
            async ctx =>
            {
                var (nguoiXem, nguoiDuocXem) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "Không tìm được cặp tài khoản đã đăng ký không bị block nhau để xem hồ sơ public.");
                var token = await LayTokenCuaTaiKhoanAsync(ctx, nguoiXem);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/users/get_user_info",
                    Obj(("user_id", IdBatBuoc(nguoiDuocXem.TaiKhoanIdServer, "tk_id_server user B"))),
                    token);
            },
            Ok,
            KiemTraHoSoPublicNguoiKhac());

        Them(ds, "USER-GET-INFO-04", "User", "Xem user đang được current user theo dõi",
            "Lấy dữ liệu từ tk_theodoi_seed.",
            async ctx =>
            {
                var capTheoDoi = LayCapTaiKhoanDangTheoDoi(ctx, "Thiếu dữ liệu tk_theodoi_seed để kiểm tra data.followed = true.");
                var token = await LayTokenCuaTaiKhoanAsync(ctx, capTheoDoi.TaiKhoanTheoDoi);
                return new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", IdBatBuoc(capTheoDoi.TaiKhoanDuocTheoDoi.TaiKhoanIdServer, "followee_tk_id_server"))), token);
            },
            Ok,
            DataBoolBang("followed", true));

        Them(ds, "USER-GET-INFO-05", "User", "Xem user đã bị current user chặn",
            "Lấy dữ liệu từ tk_chan_seed.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu dữ liệu tk_chan_seed để kiểm tra data.is_blocked = true.");
                var token = await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan);
                return new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", IdBatBuoc(capChan.TaiKhoanBiChan.TaiKhoanIdServer, "blocked_tk_id_server"))), token);
            },
            OkHoacKhongCoNguoiDung,
            DataBoolBang("is_blocked", true));

        Them(ds, "USER-GET-INFO-06", "User", "Lấy hồ sơ user_id không tồn tại",
            "Gửi user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", 999999999)), await YeuCauTokenHopLeAsync(ctx)),
            Tap("1013", "1000", "9994"));
    }

    private static void ThemCapNhatThongTinNguoiDung(List<KichBanApi> ds)
    {
        Them(ds, "USER-SET-INFO-01", "User", "Cập nhật hồ sơ với field hợp lệ",
            "Lấy một tài khoản đã đăng ký, đăng nhập lấy token rồi gửi nhiều field hợp lệ.",
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
                await YeuCauTokenHopLeAsync(ctx)),
            Ok);

        Them(ds, "USER-SET-INFO-02", "User", "Cập nhật hồ sơ với field sai định dạng",
            "Gửi email sai format hoặc field sai kiểu dữ liệu.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/set_user_info",
                Obj(("email", "sai-email"), ("username", 12345)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraHoSoChinhMinh()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var ketQua = KiemTraCoTruong(response,
                "id",
                "username",
                "listing",
                "status",
                "avatar",
                "cover_image",
                "cover_image_web",
                "followed",
                "is_blocked",
                "online",
                "default_address",
                "firstname",
                "lastname",
                "address",
                "city");

            if (!ketQua.Dat)
            {
                return Task.FromResult(ketQua);
            }

            ketQua = KiemTraCoMotTrongCacTruong(response, "phonenumber", "phone_number", "phoneNumber");
            return Task.FromResult(ketQua);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraHoSoPublicNguoiKhac()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var ketQua = KiemTraCoTruong(response,
                "id",
                "username",
                "status",
                "avatar",
                "cover_image",
                "cover_image_web",
                "listing",
                "online");

            if (!ketQua.Dat)
            {
                return Task.FromResult(ketQua);
            }

            foreach (var truongRiengTu in new[] { "email", "phonenumber", "phone_number", "phoneNumber", "firstname", "lastname", "address", "city" })
            {
                if (response.Data is JsonObject data && data.ContainsKey(truongRiengTu))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"Hồ sơ public không được trả field riêng tư `{truongRiengTu}`."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static KetQuaKiemTraThem KiemTraCoTruong(PhanHoiApi response, params string[] truongBatBuoc)
    {
        if (response.Data is not JsonObject data)
        {
            return new KetQuaKiemTraThem(false, "Response thiếu data object.");
        }

        foreach (var truong in truongBatBuoc)
        {
            if (!data.ContainsKey(truong))
            {
                return new KetQuaKiemTraThem(false, $"data thiếu trường `{truong}`. Field thực tế: {LietKeField(data)}.");
            }
        }

        return KetQuaKiemTraThem.ThanhCong;
    }

    private static KetQuaKiemTraThem KiemTraCoMotTrongCacTruong(PhanHoiApi response, params string[] cacTenTruong)
    {
        if (response.Data is not JsonObject data)
        {
            return new KetQuaKiemTraThem(false, "Response thiếu data object.");
        }

        if (cacTenTruong.Any(data.ContainsKey))
        {
            return KetQuaKiemTraThem.ThanhCong;
        }

        return new KetQuaKiemTraThem(
            false,
            $"data thiếu một trong các trường `{string.Join("`, `", cacTenTruong)}`. Field thực tế: {LietKeField(data)}.");
    }

    private static string LietKeField(JsonObject data)
    {
        return data.Count == 0
            ? "(rỗng)"
            : string.Join(", ", data.Select(x => x.Key));
    }
}




