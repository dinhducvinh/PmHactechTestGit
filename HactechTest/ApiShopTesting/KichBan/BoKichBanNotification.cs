using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
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
                var taiKhoan = LayTaiKhoanUuTienCoThongBao(ctx) ?? await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/get_notification",
                    Obj(("index", 1), ("count", 20), ("group", 0)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(taiKhoan));

                request.Tam["taiKhoan"] = taiKhoan;
                return request;
            },
            Ok,
            KiemTraResponseGetNotification(),
            DongBoThongBaoTuResponseAsync);
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
                    Obj(("notification_id", SoIdBatBuoc(thongBao.NotificationIdServer, "thongbao_seed.notification_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(taiKhoan));

                request.Tam["thongBao"] = thongBao;
                return request;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var thongBao = (ThongBaoSeed)request.Tam["thongBao"]!;
                thongBao.DaDoc = true;
                thongBao.TrangThai = "da_doc";
                thongBao.DocLuc = DateTimeOffset.Now;
                thongBao.GhiChu = "Đã đọc bởi testcase NOTIFICATION-READ-02.";
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "NOTIFICATION-READ-03", "Notification", "Đánh dấu đã đọc thông báo không tồn tại",
            "Token hợp lệ, notification_id không tồn tại trên server.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/set_read_notification",
                Obj(("notification_id", 999999999)),
                await ctx.YeuCauTokenHopLeAsync()),
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

            if (!TienIchJson.CoTruong(response.Json, "last_update"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu trường last_update."));
            }

            if (!TienIchJson.CoTruong(response.Json, "badge"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu trường badge."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static async Task DongBoThongBaoTuResponseAsync(PhanHoiApi response, YeuCauApi request, NguCanhKiemThu ctx)
    {
        if (!LaMaThanhCong(response) || response.Data is not JsonArray)
        {
            return;
        }

        var taiKhoan = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoan"]!;
        var coThayDoi = false;
        foreach (var item in LayObjectThongBao(response.Data))
        {
            var notificationId = DocNotificationIdServer(item);
            if (string.IsNullOrWhiteSpace(notificationId))
            {
                continue;
            }

            UpsertThongBaoSeed(ctx, taiKhoan, notificationId, item);
            coThayDoi = true;
        }

        if (coThayDoi)
        {
            await ctx.KhoSeed.LuuAsync();
        }
    }

    private static TaiKhoanSignupThanhCongSeed? LayTaiKhoanUuTienCoThongBao(NguCanhKiemThu ctx)
    {
        var thongBao = ctx.KhoSeed.DuLieu.ThongBaoSeed
            .Where(x => !string.IsNullOrWhiteSpace(x.NotificationIdServer))
            .OrderBy(x => x.DaDoc == true ? 1 : 0)
            .ThenBy(x => x.ThongBaoSeedId)
            .FirstOrDefault();

        return thongBao is null
            ? null
            : LayTaiKhoanTheoServerId(ctx, thongBao.TaiKhoanIdServer);
    }

    private static ThongBaoSeed LayThongBaoChuaDoc(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.ThongBaoSeed
            .Where(x => !string.IsNullOrWhiteSpace(x.NotificationIdServer))
            .Where(x => x.DaDoc != true || string.Equals(x.TrangThai, "dang_luu", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ThongBaoSeedId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu thông báo chưa đọc trong thongbao_seed. Hãy bấm Kiểm tra seed hoặc tạo nghiệp vụ phát sinh notification trước.");
    }

    private static long LayNotificationIdDangCoHoacMacDinh(NguCanhKiemThu ctx)
    {
        var thongBao = ctx.KhoSeed.DuLieu.ThongBaoSeed
            .Where(x => !string.IsNullOrWhiteSpace(x.NotificationIdServer))
            .OrderBy(x => x.ThongBaoSeedId)
            .FirstOrDefault();

        return thongBao is null
            ? 999999999
            : SoIdBatBuoc(thongBao.NotificationIdServer, "thongbao_seed.notification_id_server");
    }

    private static void UpsertThongBaoSeed(NguCanhKiemThu ctx, TaiKhoanSignupThanhCongSeed taiKhoan, string notificationId, JsonObject item)
    {
        var daDocTuServer = TienIchJson.DocBool(item, "read", "is_read", "seen", "da_doc");
        var thongBao = ctx.KhoSeed.DuLieu.ThongBaoSeed.FirstOrDefault(x => x.NotificationIdServer == notificationId);
        if (thongBao is null)
        {
            var daDoc = daDocTuServer ?? false;
            ctx.KhoSeed.DuLieu.ThongBaoSeed.Add(new ThongBaoSeed
            {
                ThongBaoSeedId = TaoIdThongBaoMoi(ctx),
                NotificationIdServer = notificationId,
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                Title = TienIchJson.DocChuoi(item, "title"),
                Content = TienIchJson.DocChuoi(item, "content", "message"),
                ObjectIdServer = TienIchJson.DocChuoi(item, "object_id", "objectId", "target_id"),
                NotificationType = TienIchJson.DocChuoi(item, "type", "notification_type"),
                DaDoc = daDoc,
                TrangThai = daDoc ? "da_doc" : "dang_luu",
                LayLuc = DateTimeOffset.Now,
                GhiChu = "Đồng bộ bởi testcase NOTIFICATION-GET-02."
            });
            return;
        }

        var daDocHienTai = daDocTuServer ?? thongBao.DaDoc ?? false;
        thongBao.TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer;
        thongBao.Title = TienIchJson.DocChuoi(item, "title");
        thongBao.Content = TienIchJson.DocChuoi(item, "content", "message");
        thongBao.ObjectIdServer = TienIchJson.DocChuoi(item, "object_id", "objectId", "target_id");
        thongBao.NotificationType = TienIchJson.DocChuoi(item, "type", "notification_type");
        thongBao.DaDoc = daDocHienTai;
        thongBao.TrangThai = daDocHienTai ? "da_doc" : "dang_luu";
        thongBao.LayLuc = DateTimeOffset.Now;
    }

    private static IEnumerable<JsonObject> LayObjectThongBao(JsonNode? node)
    {
        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item is JsonObject obj)
                {
                    yield return obj;
                }
            }
        }
        else if (node is JsonObject obj)
        {
            yield return obj;
        }
    }

    private static string? DocNotificationIdServer(JsonNode? node)
    {
        return TienIchJson.DocChuoi(node, "id", "notification_id", "notificationId");
    }

    private static int TaoIdThongBaoMoi(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.ThongBaoSeed.Count == 0
            ? 1
            : ctx.KhoSeed.DuLieu.ThongBaoSeed.Max(x => x.ThongBaoSeedId) + 1;
    }
}
