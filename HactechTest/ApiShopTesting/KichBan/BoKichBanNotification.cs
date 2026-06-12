using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanNotification(List<KichBanApi> ds)
    {
        ThemGetNotification(ds);
        ThemSetReadNotification(ds);
    }

    private static void ThemGetNotification(List<KichBanApi> ds)
    {
        Them(ds, "NOTIFICATION-GET-01", "Notification", "Lấy thông báo với token không hợp lệ",
            "Gọi POST /notification/get_notification bằng token hết hạn/sai định dạng, body có index/count hợp lệ.",
            ctx => Req(
                HttpMethod.Post,
                "/notification/get_notification",
                Obj(("index", 1), ("count", 20), ("group", 0)),
                ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "NOTIFICATION-GET-02", "Notification", "Lấy danh sách thông báo",
            "Token hợp lệ, index/count hợp lệ. Nếu response có data thì đồng bộ notification vào thongbao_seed.",
            async ctx =>
            {
                var taiKhoan = LayTaiKhoanUuTienCoThongBao(ctx) ?? await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/get_notification",
                    Obj(("index", 1), ("count", 20), ("group", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));

                request.Tam["taiKhoan"] = taiKhoan;
                return request;
            },
            Ok,
            KiemTraResponseGetNotification(),
            async (response, request, ctx) => await ctx.CapNhatDB.DongBoThongBaoTuResponseAsync(response, request));
    }

    private static void ThemSetReadNotification(List<KichBanApi> ds)
    {
        Them(ds, "NOTIFICATION-READ-01", "Notification", "Đánh dấu đã đọc với token không hợp lệ",
            "Gọi POST /notification/set_read_notification bằng token hết hạn/sai định dạng.",
            ctx =>
            {
                var notificationId = LayNotificationIdDangCoHoacMacDinh(ctx);
                return Req(
                    HttpMethod.Post,
                    "/notification/set_read_notification",
                    Obj(("notification_id", notificationId)),
                    ctx.TokenSaiDinhDang);
            },
            SaiToken);

        Them(ds, "NOTIFICATION-READ-02", "Notification", "Đánh dấu thông báo đã đọc",
            "Token hợp lệ, notification_id_server tồn tại trong thongbao_seed. Sau khi thành công cập nhật seed sang da_doc.",
            async ctx =>
            {
                var thongBao = LayThongBaoChuaDoc(ctx);
                var taiKhoan = LayTaiKhoanTheoServerId(ctx, thongBao.TaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản nhận thông báo trong taikhoan_signupthanhcong.");

                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/set_read_notification",
                    Obj(("notification_id", IdBatBuoc(thongBao.NotificationIdServer, "thongbao_seed.notification_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));

                request.Tam["thongBao"] = thongBao;
                return request;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var thongBao = (ThongBaoSeed)request.Tam["thongBao"]!;
                await ctx.CapNhatDB.DanhDauThongBaoDaDocAsync(thongBao);
            });

        Them(ds, "NOTIFICATION-READ-03", "Notification", "Đánh dấu đã đọc thông báo không tồn tại",
            "Token hợp lệ, notification_id không tồn tại trên server.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/set_read_notification",
                Obj(("notification_id", 999999999)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseGetNotification()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_notification không phải mảng Notification[]."));
            }

            if (response.Json is not JsonObject root || !root.ContainsKey("last_update"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu trường last_update."));
            }

            if (!root.ContainsKey("badge"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu trường badge."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

}






