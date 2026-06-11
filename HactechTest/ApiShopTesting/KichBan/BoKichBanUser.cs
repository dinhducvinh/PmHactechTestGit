using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

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
        Them(ds, "USER-GET-INFO-01", "User", "Láº¥y há»“ sÆ¡ chÃ­nh mÃ¬nh báº±ng token há»£p lá»‡",
            "Láº¥y má»™t tÃ i khoáº£n Ä‘Ã£ Ä‘Äƒng kÃ½, Ä‘Äƒng nháº­p láº¥y token rá»“i gá»i API vá»›i body rá»—ng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/get_user_info", new Dictionary<string, object?>(), await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            KiemTraHoSoChinhMinh());

        Them(ds, "USER-GET-INFO-02", "User", "Láº¥y há»“ sÆ¡ báº±ng token khÃ´ng há»£p lá»‡",
            "Gá»i API báº±ng token sai Ä‘á»‹nh dáº¡ng/háº¿t háº¡n.",
            ctx => Req(HttpMethod.Post, "/users/get_user_info", new Dictionary<string, object?>(), ctx.TokenSaiDinhDang),
            Tap("9998", "HTTP_401", "HTTP_403", "1013", "1005"));

        Them(ds, "USER-GET-INFO-03", "User", "User A xem há»“ sÆ¡ public cá»§a User B",
            "Chá»n 2 tÃ i khoáº£n Ä‘Ã£ Ä‘Äƒng kÃ½, loáº¡i cÃ¡c cáº·p Ä‘ang block nhau á»Ÿ cáº£ hai chiá»u.",
            async ctx =>
            {
                var (nguoiXem, nguoiDuocXem) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "KhÃ´ng tÃ¬m Ä‘Æ°á»£c cáº·p tÃ i khoáº£n Ä‘Ã£ Ä‘Äƒng kÃ½ khÃ´ng bá»‹ block nhau Ä‘á»ƒ xem há»“ sÆ¡ public.");
                var token = await LayTokenCuaTaiKhoanAsync(ctx, nguoiXem);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/users/get_user_info",
                    Obj(("user_id", IdBatBuoc(nguoiDuocXem.TaiKhoanIdServer, "tk_id_server user B"))),
                    token);
            },
            Ok,
            KiemTraHoSoPublicNguoiKhac());

        Them(ds, "USER-GET-INFO-04", "User", "Xem user Ä‘ang Ä‘Æ°á»£c current user theo dÃµi",
            "Láº¥y dá»¯ liá»‡u tá»« tk_theodoi_seed.",
            async ctx =>
            {
                var capTheoDoi = LayCapTaiKhoanDangTheoDoi(ctx, "Thiáº¿u dá»¯ liá»‡u tk_theodoi_seed Ä‘á»ƒ kiá»ƒm tra data.followed = true.");
                var token = await LayTokenCuaTaiKhoanAsync(ctx, capTheoDoi.TaiKhoanTheoDoi);
                return new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", IdBatBuoc(capTheoDoi.TaiKhoanDuocTheoDoi.TaiKhoanIdServer, "followee_tk_id_server"))), token);
            },
            Ok,
            DataBoolBang("followed", true));

        Them(ds, "USER-GET-INFO-05", "User", "Xem user Ä‘Ã£ bá»‹ current user cháº·n",
            "Láº¥y dá»¯ liá»‡u tá»« tk_chan_seed.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiáº¿u dá»¯ liá»‡u tk_chan_seed Ä‘á»ƒ kiá»ƒm tra data.is_blocked = true.");
                var token = await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan);
                return new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", IdBatBuoc(capChan.TaiKhoanBiChan.TaiKhoanIdServer, "blocked_tk_id_server"))), token);
            },
            OkHoacKhongCoNguoiDung,
            DataBoolBang("is_blocked", true));

        Them(ds, "USER-GET-INFO-06", "User", "Láº¥y há»“ sÆ¡ user_id khÃ´ng tá»“n táº¡i",
            "Gá»­i user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/get_user_info", Obj(("user_id", 999999999)), await YeuCauTokenHopLeAsync(ctx)),
            Tap("1013", "1000", "9994"));
    }

    private static void ThemCapNhatThongTinNguoiDung(List<KichBanApi> ds)
    {
        Them(ds, "USER-SET-INFO-01", "User", "Cáº­p nháº­t há»“ sÆ¡ vá»›i field há»£p lá»‡",
            "Láº¥y má»™t tÃ i khoáº£n Ä‘Ã£ Ä‘Äƒng kÃ½, Ä‘Äƒng nháº­p láº¥y token rá»“i gá»­i nhiá»u field há»£p lá»‡.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/users/set_user_info",
                Obj(
                    ("email", $"test_{DateTimeOffset.Now:HHmmss}@example.com"),
                    ("username", $"tester_{DateTimeOffset.Now:HHmmss}"),
                    ("status", "TÃ i khoáº£n seed dÃ¹ng Ä‘á»ƒ kiá»ƒm thá»­ API"),
                    ("avatar", "https://example.com/avatar.jpg"),
                    ("firstname", "Test"),
                    ("lastname", "API"),
                    ("address", "HÃ  Ná»™i"),
                    ("cover_image", "https://example.com/cover.jpg"),
                    ("cover_image_web", "https://example.com/cover-web.jpg")),
                await YeuCauTokenHopLeAsync(ctx)),
            Ok);

        Them(ds, "USER-SET-INFO-02", "User", "Cáº­p nháº­t há»“ sÆ¡ vá»›i field sai Ä‘á»‹nh dáº¡ng",
            "Gá»­i email sai format hoáº·c field sai kiá»ƒu dá»¯ liá»‡u.",
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
                "email",
                "phonenumber",
                "firstname",
                "lastname",
                "address",
                "city");

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

            foreach (var truongRiengTu in new[] { "email", "phonenumber", "firstname", "lastname", "address", "city" })
            {
                if (response.Data is JsonObject data && data.ContainsKey(truongRiengTu))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"Há»“ sÆ¡ public khÃ´ng Ä‘Æ°á»£c tráº£ field riÃªng tÆ° `{truongRiengTu}`."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static KetQuaKiemTraThem KiemTraCoTruong(PhanHoiApi response, params string[] truongBatBuoc)
    {
        if (response.Data is not JsonObject data)
        {
            return new KetQuaKiemTraThem(false, "Response thiáº¿u data object.");
        }

        foreach (var truong in truongBatBuoc)
        {
            if (!data.ContainsKey(truong))
            {
                return new KetQuaKiemTraThem(false, $"data thiáº¿u trÆ°á»ng `{truong}`.");
            }
        }

        return KetQuaKiemTraThem.ThanhCong;
    }
}




