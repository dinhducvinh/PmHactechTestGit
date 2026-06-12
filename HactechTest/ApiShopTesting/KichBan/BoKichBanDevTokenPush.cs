using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

using System.Text.Json.Nodes;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> DevTokenPushSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");

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
                await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            DataOkHoacKhongCanKiemTra());

        Them(ds, "DEV-TOKEN-02", "DevTokenPush", "Đăng ký device token không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/dev_tokens/set_devtoken", Obj(("devtype", "0"), ("devtoken", "device_token_no_auth"))),
            DevTokenPushSaiToken);

        Them(ds, "DEV-TOKEN-03", "DevTokenPush", "Đăng ký device token thiếu devtoken",
            "Có token và devtype nhưng thiếu devtoken.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/dev_tokens/set_devtoken",
                Obj(("devtype", "0")),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "DEV-TOKEN-04", "DevTokenPush", "Đăng ký device token với token không hợp lệ",
            "Token sai/hết hạn, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/dev_tokens/set_devtoken",
                Obj(("devtype", "1"), ("devtoken", "device_token_bad_auth")),
                ctx.TokenSaiDinhDang),
            DevTokenPushSaiToken);
    }

    private static void ThemPushSetting(List<KichBanApi> ds)
    {
        Them(ds, "PUSH-GET-01", "DevTokenPush", "Lấy cấu hình push setting",
            "Token hợp lệ của user đang tồn tại.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/get_push_setting", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            DataCoTruong("like", "comment", "transaction", "announcement", "sound_on", "sound_default"));

        Them(ds, "PUSH-GET-02", "DevTokenPush", "Lấy push setting với token không hợp lệ",
            "Token sai/hết hạn.",
            ctx => Req(HttpMethod.Post, "/push_settings/get_push_setting", Obj(), ctx.TokenSaiDinhDang),
            DevTokenPushSaiToken);

        Them(ds, "PUSH-GET-03", "DevTokenPush", "Lấy push setting không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/push_settings/get_push_setting", Obj()),
            DevTokenPushSaiToken);

        Them(ds, "PUSH-SET-01", "DevTokenPush", "Cập nhật nhiều push setting",
            "Cập nhật nhiều field cấu hình bằng giá trị string 0/1.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("like", "0"), ("comment", "1"), ("transaction", "1"), ("announcement", "1"), ("sound_on", "0"), ("sound_default", "string")),
                await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            DataOkHoacKhongCanKiemTra(),
            KiemTraPushSettingSauCapNhatAsync(("like", "0"), ("comment", "1"), ("transaction", "1"), ("announcement", "1"), ("sound_on", "0")));

        Them(ds, "PUSH-SET-02", "DevTokenPush", "Cập nhật một push setting",
            "Chỉ cập nhật announcement, các field còn lại giữ nguyên.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("announcement", "0")),
                await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            DataOkHoacKhongCanKiemTra(),
            KiemTraPushSettingSauCapNhatAsync(("announcement", "0")));

        Them(ds, "PUSH-SET-03", "DevTokenPush", "Cập nhật push setting với token không hợp lệ",
            "Token sai/hết hạn, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("like", "1"), ("comment", "1")),
                ctx.TokenSaiDinhDang),
            DevTokenPushSaiToken);

        Them(ds, "PUSH-SET-04", "DevTokenPush", "Cập nhật push setting giá trị không hợp lệ",
            "like = 2, sound_on = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/push_settings/set_push_setting",
                Obj(("like", "2"), ("sound_on", "abc")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataOkHoacKhongCanKiemTra()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is null)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonValue)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var data = response.Data.ToString().Trim('"');
            return string.Equals(data, "OK", StringComparison.OrdinalIgnoreCase)
                ? Task.FromResult(KetQuaKiemTraThem.ThanhCong)
                : Task.FromResult(new KetQuaKiemTraThem(false, $"data mong đợi là OK, thực tế là {response.Data}."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task> KiemTraPushSettingSauCapNhatAsync(
        params (string TenTruong, string GiaTriMongDoi)[] truongCanKiemTra)
    {
        return async (_, request, ctx) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return;
            }

            var response = await ctx.Api.GuiAsync(new YeuCauApi(
                HttpMethod.Post,
                "/push_settings/get_push_setting",
                Obj(),
                request.Token));

            if (!LaMaThanhCong(response))
            {
                throw new LoiChuanBiKiemThuException($"Đã cập nhật push setting nhưng không gọi lại được /push_settings/get_push_setting để xác minh. Mã thực tế: {response.MaSoSanh}.");
            }

            foreach (var (tenTruong, giaTriMongDoi) in truongCanKiemTra)
            {
                var giaTriThucTe = response.Data?[tenTruong]?.ToString();
                if (!CungGiaTriPushSetting(giaTriThucTe, giaTriMongDoi))
                {
                    throw new LoiChuanBiKiemThuException($"Push setting `{tenTruong}` chưa cập nhật đúng. Mong đợi {giaTriMongDoi}, thực tế {giaTriThucTe ?? "null"}.");
                }
            }
        };
    }

    private static bool CungGiaTriPushSetting(string? giaTriThucTe, string giaTriMongDoi)
    {
        if (string.Equals(giaTriThucTe, giaTriMongDoi, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (bool.TryParse(giaTriThucTe, out var boolThucTe) && int.TryParse(giaTriMongDoi, out var soMongDoi))
        {
            return boolThucTe == (soMongDoi != 0);
        }

        if (int.TryParse(giaTriThucTe, out var soThucTe) && int.TryParse(giaTriMongDoi, out var soMongDoi2))
        {
            return soThucTe == soMongDoi2;
        }

        return false;
    }
}




