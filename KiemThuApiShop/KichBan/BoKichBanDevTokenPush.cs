using KiemThuApiShop.Core;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanDevTokenPush(List<KichBanApi> ds)
    {
        ThemDevToken(ds);
        ThemPushSetting(ds);
    }

    private static void ThemDevToken(List<KichBanApi> ds)
    {
        Them(ds, "DEV-TOKEN-01", "DevTokenPush", "Đăng ký device token hợp lệ",
            "Token hợp lệ, devtype/devtoken đúng định dạng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/dev_tokens/set_devtoken",
                Obj(("devtype", "0"), ("devtoken", $"device_token_seed_{DateTimeOffset.Now:yyyyMMddHHmmss}")),
                await ctx.YeuCauTokenHopLeAsync()),
            Ok);

        Them(ds, "DEV-TOKEN-02", "DevTokenPush", "Đăng ký device token không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/dev_tokens/set_devtoken", Obj(("devtype", "0"), ("devtoken", "device_token_no_auth"))),
            SaiToken);

        Them(ds, "DEV-TOKEN-03", "DevTokenPush", "Đăng ký device token thiếu devtoken",
            "Có token và devtype nhưng thiếu devtoken.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/dev_tokens/set_devtoken",
                Obj(("devtype", "0")),
                await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSoHoacSaiGiaTri);

        Them(ds, "DEV-TOKEN-04", "DevTokenPush", "Đăng ký device token với token không hợp lệ",
            "Token sai/hết hạn, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/dev_tokens/set_devtoken",
                Obj(("devtype", "1"), ("devtoken", "device_token_bad_auth")),
                ctx.TokenSaiDinhDang),
            SaiToken);
    }

    private static void ThemPushSetting(List<KichBanApi> ds)
    {
        Them(ds, "PUSH-GET-01", "DevTokenPush", "Lấy cấu hình push setting",
            "Token hợp lệ của user đang tồn tại.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/get_push_setting", Obj(), await ctx.YeuCauTokenHopLeAsync()),
            Ok,
            DataCoTruong("like", "comment", "transaction", "announcement", "sound_on", "sound_default"));

        Them(ds, "PUSH-GET-02", "DevTokenPush", "Lấy push setting với token không hợp lệ",
            "Token sai/hết hạn.",
            ctx => Req(HttpMethod.Post, "/push_settings/get_push_setting", Obj(), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "PUSH-GET-03", "DevTokenPush", "Lấy push setting sau khi tắt like/comment",
            "Gọi set_push_setting để tắt like/comment rồi đọc lại.",
            async ctx =>
            {
                var token = await ctx.YeuCauTokenHopLeAsync();
                var setResponse = await ctx.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/push_settings/set_push_setting",
                    Obj(("like", 0), ("comment", 0)),
                    token));
                if (setResponse.MaSoSanh != "1000")
                {
                    throw new BoQuaKiemThuException("Không set được push setting like/comment trước khi đọc lại.");
                }

                return new YeuCauApi(HttpMethod.Post, "/push_settings/get_push_setting", Obj(), token);
            },
            Ok,
            (response, _, _) =>
            {
                if (!LaMaThanhCong(response))
                {
                    return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
                }

                var like = TienIchJson.DocBool(response.Data, "like");
                var comment = TienIchJson.DocBool(response.Data, "comment");
                if (like == false && comment == false)
                {
                    return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
                }

                return Task.FromResult(new KetQuaKiemTraThem(false,
                    $"Sau khi tắt, data.like và data.comment phải bằng 0/false. Thực tế like={like?.ToString() ?? "null"}, comment={comment?.ToString() ?? "null"}."));
            });

        Them(ds, "PUSH-GET-04", "DevTokenPush", "Lấy push setting không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/push_settings/get_push_setting", Obj()),
            SaiToken);

        Them(ds, "PUSH-SET-01", "DevTokenPush", "Cập nhật nhiều push setting",
            "Cập nhật nhiều field cấu hình.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("like", 0), ("comment", 1), ("transaction", 1), ("announcement", 1), ("sound_on", 0), ("sound_default", "default")),
                await ctx.YeuCauTokenHopLeAsync()),
            Ok);

        Them(ds, "PUSH-SET-02", "DevTokenPush", "Cập nhật một push setting",
            "Chỉ cập nhật announcement.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("announcement", 0)),
                await ctx.YeuCauTokenHopLeAsync()),
            Ok);

        Them(ds, "PUSH-SET-03", "DevTokenPush", "Cập nhật push setting với token không hợp lệ",
            "Token sai/hết hạn, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("like", 1), ("comment", 1)),
                ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "PUSH-SET-04", "DevTokenPush", "Cập nhật push setting giá trị không hợp lệ",
            "like = 2, sound_on = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("like", 2), ("sound_on", "abc")),
                await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);
    }
}
