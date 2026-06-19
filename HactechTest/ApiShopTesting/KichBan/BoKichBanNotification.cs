using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> NotificationSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> NotificationUserKhongTonTai = Tap("1005", "1001", "HTTP_500");

    private static void ThemKichBanNotification(List<KichBanApi> ds)
    {
        ThemAddNotification(ds);
        ThemGetNotificationTheoTaiLieu(ds);
        ThemSetReadNotificationTheoTaiLieu(ds);
    }

    private static void ThemAddNotification(List<KichBanApi> ds)
    {
        Them(ds, "NOTIFICATION-ADD-01", "Notification", "Tao thong bao voi token khong hop le",
            "Goi POST /notification/add_notification bang token het han/sai dinh dang, body hop le.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/add_notification",
                    TaoBodyNotification("order", LayObjectIdNotification(ctx), "Thong bao test invalid token", IdBatBuoc(taiKhoan.TaiKhoanIdServer, "receiver tk_id_server")),
                    ctx.TokenSaiDinhDang);
            },
            NotificationSaiToken);

        Them(ds, "NOTIFICATION-ADD-02", "Notification", "Tao thong bao thanh cong cho user ton tai",
            "Token hop le, body hop le va user_id ton tai.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return await TaoRequestAddNotificationAsync(ctx, taiKhoan, taiKhoan, "NOTIFICATION-ADD-02");
            },
            Ok,
            KiemTraResponseAddNotification(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuThongBaoSauAddNotificationAsync(response, request));

        Them(ds, "NOTIFICATION-ADD-03", "Notification", "Tao thong bao cho user khac current user",
            "Token hop le cua user A, user_id trong body la user B va hai user khong bi chan.",
            async ctx =>
            {
                var cap = ChonCapTaiKhoanKhongCoQuanHeChan(ctx, "Khong tim duoc cap tai khoan khong bi chan de tao notification.");
                return await TaoRequestAddNotificationAsync(ctx, cap.TaiKhoanThuNhat, cap.TaiKhoanThuHai, "NOTIFICATION-ADD-03");
            },
            Ok,
            KiemTraResponseAddNotification(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuThongBaoSauAddNotificationAsync(response, request));

        Them(ds, "NOTIFICATION-ADD-04", "Notification", "Tao thong bao thieu field hoac sai kieu",
            "Token hop le nhung body thieu object_id/user_id hoac sai kieu du lieu.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/add_notification",
                    Obj(
                        ("type", "order"),
                        ("title", "Thong bao test thieu object_id"),
                        ("user_id", IdBatBuoc(taiKhoan.TaiKhoanIdServer, "receiver tk_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiKieu);

        Them(ds, "NOTIFICATION-ADD-05", "Notification", "Tao thong bao voi type hoac title rong",
            "Token hop le nhung type/title la chuoi rong.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/add_notification",
                    TaoBodyNotification("", LayObjectIdNotification(ctx), "Thong bao test", IdBatBuoc(taiKhoan.TaiKhoanIdServer, "receiver tk_id_server")),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiGiaTri);

        Them(ds, "NOTIFICATION-ADD-06", "Notification", "Tao thong bao cho user_id khong ton tai",
            "Token hop le, body hop le nhung user_id khong ton tai tren server.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/add_notification",
                    TaoBodyNotification("order", LayObjectIdNotification(ctx), "Thong bao test user khong ton tai", 999999999),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            NotificationUserKhongTonTai);
    }

    private static void ThemGetNotificationTheoTaiLieu(List<KichBanApi> ds)
    {
        Them(ds, "NOTIFICATION-GET-01", "Notification", "Lay thong bao voi token khong hop le",
            "Goi POST /notification/get_notification bang token het han/sai dinh dang, body co index/count hop le.",
            ctx => Req(
                HttpMethod.Post,
                "/notification/get_notification",
                Obj(("index", 0), ("count", 20)),
                ctx.TokenSaiDinhDang),
            NotificationSaiToken);

        Them(ds, "NOTIFICATION-GET-02", "Notification", "Lay danh sach thong bao",
            "Token hop le, index/count hop le. Neu response co data thi dong bo notification vao thongbao_seed.",
            async ctx =>
            {
                var taiKhoan = LayTaiKhoanUuTienCoThongBao(ctx) ?? await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/notification/get_notification",
                    Obj(("index", 0), ("count", 20)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));

                request.Tam["taiKhoan"] = taiKhoan;
                return request;
            },
            Ok,
            KiemTraResponseGetNotification(),
            async (response, request, ctx) => await ctx.CapNhatDB.DongBoThongBaoTuResponseAsync(response, request));
    }

    private static void ThemSetReadNotificationTheoTaiLieu(List<KichBanApi> ds)
    {
        Them(ds, "NOTIFICATION-READ-01", "Notification", "Danh dau da doc voi token khong hop le",
            "Goi POST /notification/set_read_notification bang token het han/sai dinh dang.",
            ctx =>
            {
                var notificationId = LayNotificationIdDangCoHoacMacDinh(ctx);
                return Req(
                    HttpMethod.Post,
                    "/notification/set_read_notification",
                    Obj(("notification_id", notificationId)),
                    ctx.TokenSaiDinhDang);
            },
            NotificationSaiToken);

        Them(ds, "NOTIFICATION-READ-02", "Notification", "Danh dau thong bao da doc",
            "Token hop le, notification_id_server ton tai trong thongbao_seed cua tai khoan nhan. Sau khi thanh cong cap nhat seed sang da_doc.",
            async ctx =>
            {
                var thongBao = LayThongBaoChuaDoc(ctx);
                var taiKhoan = LayTaiKhoanTheoServerId(ctx, thongBao.TaiKhoanNhanIdServer)
                    ?? throw new BoQuaKiemThuException("Thieu tai khoan nhan thong bao trong taikhoan_signupthanhcong.");

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

        Them(ds, "NOTIFICATION-READ-03", "Notification", "Danh dau da doc thong bao khong ton tai",
            "Token hop le, notification_id khong ton tai tren server.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/set_read_notification",
                Obj(("notification_id", 999999999)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static async Task<YeuCauApi> TaoRequestAddNotificationAsync(
        NguCanhKiemThu ctx,
        TaiKhoanSignupThanhCongSeed nguoiGui,
        TaiKhoanSignupThanhCongSeed nguoiNhan,
        string maTest)
    {
        var (notificationType, objectId) = ChonThongTinNotification(ctx);
        var title = $"{maTest} {DateTimeOffset.Now:yyyyMMddHHmmssfff}";
        var request = new YeuCauApi(
            HttpMethod.Post,
            "/notification/add_notification",
            TaoBodyNotification(notificationType, objectId, title, IdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver tk_id_server")),
            await LayTokenCuaTaiKhoanAsync(ctx, nguoiGui));

        request.Tam["nguoiGui"] = nguoiGui;
        request.Tam["nguoiNhan"] = nguoiNhan;
        request.Tam["notificationType"] = notificationType;
        request.Tam["objectId"] = objectId;
        request.Tam["title"] = title;
        return request;
    }

    private static Dictionary<string, object?> TaoBodyNotification(string type, int objectId, string title, int userId)
    {
        return Obj(
            ("type", type),
            ("object_id", objectId),
            ("title", title),
            ("user_id", userId));
    }

    private static (string Type, int ObjectId) ChonThongTinNotification(NguCanhKiemThu ctx)
    {
        var donHang = ctx.CapNhatDB.DuLieu.DonHangSeed
            .Where(x => x.DonHangIdServer is > 0 && x.TrangThai != "da_xoa")
            .OrderBy(x => x.DonHangIdServer)
            .FirstOrDefault();
        if (donHang?.DonHangIdServer is > 0)
        {
            return ("order", donHang.DonHangIdServer.Value);
        }

        var sanPham = ctx.CapNhatDB.DuLieu.SanPhamSeed
            .Where(x => x.SanPhamIdServer is > 0)
            .OrderBy(x => x.ThuTuNoiBo)
            .FirstOrDefault();
        if (sanPham?.SanPhamIdServer is > 0)
        {
            return ("product", sanPham.SanPhamIdServer.Value);
        }

        return ("system", 1);
    }

    private static int LayObjectIdNotification(NguCanhKiemThu ctx)
    {
        return ChonThongTinNotification(ctx).ObjectId;
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseAddNotification()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data cua add_notification khong phai object Notification."));
            }

            if (DocNotificationIdServer(data) is not > 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response add_notification thieu data.id."));
            }

            var daDoc = DocBoolTuObject(data, "read", "is_read", "da_doc");
            if (daDoc == true)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Notification moi tao khong duoc o trang thai read=true."));
            }

            var titleMongDoi = request.Tam.TryGetValue("title", out var titleRaw) ? titleRaw as string : null;
            var titleThucTe = DocChuoiTuObject(data, "title");
            if (!string.IsNullOrWhiteSpace(titleMongDoi) &&
                !string.IsNullOrWhiteSpace(titleThucTe) &&
                !string.Equals(titleMongDoi, titleThucTe, StringComparison.Ordinal))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response add_notification tra title khac request."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static void ThemGetNotification(List<KichBanApi> ds)
    {
        Them(ds, "NOTIFICATION-GET-01", "Notification", "Lấy thông báo với token không hợp lệ",
            "Gọi POST /notification/get_notification bằng token hết hạn/sai định dạng, body có index/count hợp lệ.",
            ctx => Req(
                HttpMethod.Post,
                "/notification/get_notification",
                Obj(("index", 0), ("count", 20)),
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
                    Obj(("index", 0), ("count", 20)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));

                request.Tam["taiKhoan"] = taiKhoan;
                return request;
            },
            Ok,
            KiemTraResponseGetNotification(),
            async (response, request, ctx) => await ctx.CapNhatDB.DongBoThongBaoTuResponseAsync(response, request));

        Them(ds, "NOTIFICATION-GET-03", "Notification", "Lấy thông báo thiếu index",
            "Token hợp lệ nhưng body thiếu index bắt buộc.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/get_notification",
                Obj(("count", 20)),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "NOTIFICATION-GET-04", "Notification", "Lấy thông báo thiếu count",
            "Token hợp lệ nhưng body thiếu count bắt buộc.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/get_notification",
                Obj(("index", 0)),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "NOTIFICATION-GET-05", "Notification", "Lấy thông báo sai kiểu tham số",
            "Token hợp lệ nhưng index/count không phải số nguyên.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/get_notification",
                Obj(("index", "abc"), ("count", "xyz")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiKieu);

        Them(ds, "NOTIFICATION-GET-06", "Notification", "Lấy thông báo sai giá trị tham số",
            "Token hợp lệ nhưng index < 0 hoặc count < 1.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/get_notification",
                Obj(("index", -1), ("count", 0)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
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

        Them(ds, "NOTIFICATION-READ-04", "Notification", "Đánh dấu đã đọc thiếu notification_id",
            "Token hợp lệ nhưng body thiếu notification_id bắt buộc.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/set_read_notification",
                Obj(),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "NOTIFICATION-READ-05", "Notification", "Đánh dấu đã đọc sai kiểu notification_id",
            "Token hợp lệ nhưng notification_id không phải số nguyên.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/notification/set_read_notification",
                Obj(("notification_id", "abc")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiKieu);
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






