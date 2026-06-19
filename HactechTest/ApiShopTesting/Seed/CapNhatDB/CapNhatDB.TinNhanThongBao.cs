using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    internal async Task DongBoTinNhanTuResponseAsync(PhanHoiApi response, YeuCauApi request)
    {
        if (!LaMaThanhCong(response))
        {
            return;
        }

        var conversationId = DocIdSau(response.Data, "conversation_id");
        var messageId = DocIdSau(response.Data, "message_id");
        if (conversationId is not > 0 || messageId is not > 0)
        {
            return;
        }

        var nguoiGui = (TaiKhoanSignupThanhCongSeed)request.Tam["nguoiGui"]!;
        var nguoiNhan = (TaiKhoanSignupThanhCongSeed)request.Tam["nguoiNhan"]!;
        var typeMessage = (string)request.Tam["typeMessage"]!;
        var noiDung = request.Tam.TryGetValue("noiDung", out var noiDungRaw) ? noiDungRaw as string : null;
        var productId = request.Tam.TryGetValue("productId", out var productIdRaw) && productIdRaw is int productIdValue
            ? productIdValue
            : (int?)null;

        var tinNhanDaCo = DuLieu.TinNhanSeed.FirstOrDefault(x => x.MessageIdServer == messageId.Value);
        if (tinNhanDaCo is null)
        {
            DuLieu.TinNhanSeed.Add(new TinNhanSeed
            {
                ConversationIdServer = conversationId.Value,
                MessageIdServer = messageId.Value,
                SenderTaiKhoanIdServer = nguoiGui.TaiKhoanIdServer,
                ReceiverTaiKhoanIdServer = nguoiNhan.TaiKhoanIdServer,
                SanPhamIdServer = productId,
                TypeMessage = typeMessage,
                NoiDung = noiDung,
                TrangThai = "da_gui",
                TaoBoiTest = true,
                GuiLuc = DateTimeOffset.Now,
                GhiChu = "Dong bo boi testcase conversation send"
            });
        }
        else
        {
            tinNhanDaCo.ConversationIdServer = conversationId.Value;
            tinNhanDaCo.SenderTaiKhoanIdServer = nguoiGui.TaiKhoanIdServer;
            tinNhanDaCo.ReceiverTaiKhoanIdServer = nguoiNhan.TaiKhoanIdServer;
            tinNhanDaCo.SanPhamIdServer = productId;
            tinNhanDaCo.TypeMessage = typeMessage;
            tinNhanDaCo.NoiDung = noiDung;
            tinNhanDaCo.TrangThai = "da_gui";
            tinNhanDaCo.GuiLuc = DateTimeOffset.Now;
        }

        await LuuAsync(BangDuLieuSeed.TinNhan);
    }

    internal async Task DongBoThongBaoTuResponseAsync(PhanHoiApi response, YeuCauApi request)
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
            if (notificationId is not > 0)
            {
                continue;
            }

            UpsertThongBaoSeed(taiKhoan, notificationId.Value, item);
            coThayDoi = true;
        }

        if (coThayDoi)
        {
            await LuuAsync(BangDuLieuSeed.ThongBao);
        }
    }

    internal async Task LuuThongBaoSauAddNotificationAsync(PhanHoiApi response, YeuCauApi request)
    {
        if (!LaMaThanhCong(response))
        {
            return;
        }

        var notificationId = DocNotificationIdServer(response.Data);
        if (notificationId is not > 0)
        {
            return;
        }

        var nguoiGui = request.Tam.TryGetValue("nguoiGui", out var nguoiGuiRaw)
            ? nguoiGuiRaw as TaiKhoanSignupThanhCongSeed
            : null;
        var nguoiNhan = request.Tam.TryGetValue("nguoiNhan", out var nguoiNhanRaw)
            ? nguoiNhanRaw as TaiKhoanSignupThanhCongSeed
            : null;
        var dataObj = response.Data as JsonObject;

        var thongBao = DuLieu.ThongBaoSeed.FirstOrDefault(x => x.NotificationIdServer == notificationId.Value);
        if (thongBao is null)
        {
            thongBao = new ThongBaoSeed
            {
                NotificationIdServer = notificationId.Value
            };
            DuLieu.ThongBaoSeed.Add(thongBao);
        }

        thongBao.TaiKhoanNhanIdServer = nguoiNhan?.TaiKhoanIdServer ?? thongBao.TaiKhoanNhanIdServer;
        thongBao.TaiKhoanGuiIdServer = nguoiGui?.TaiKhoanIdServer ?? thongBao.TaiKhoanGuiIdServer;
        thongBao.NotificationType = LayTamChuoi(request, "notificationType")
            ?? (dataObj is null ? null : DocChuoiTuObject(dataObj, "type", "notification_type"))
            ?? thongBao.NotificationType;
        thongBao.ObjectIdServer = LayTamInt(request, "objectId")
            ?? DocIntTuObject(response.Data, "object_id", "object_id_server")
            ?? thongBao.ObjectIdServer;
        thongBao.Title = LayTamChuoi(request, "title")
            ?? (dataObj is null ? null : DocChuoiTuObject(dataObj, "title"))
            ?? thongBao.Title;
        thongBao.DaDoc = false;
        thongBao.TrangThai = "dang_luu";
        thongBao.LayLuc = DateTimeOffset.Now;
        thongBao.GhiChu = "Tao bang API /notification/add_notification.";

        await LuuAsync(BangDuLieuSeed.ThongBao);
    }

    internal async Task DanhDauThongBaoDaDocAsync(ThongBaoSeed thongBao)
    {
        thongBao.DaDoc = true;
        thongBao.TrangThai = "da_doc";
        thongBao.DocLuc = DateTimeOffset.Now;
        thongBao.GhiChu = "Đã đọc bởi testcase NOTIFICATION-READ-02.";
        await LuuAsync(BangDuLieuSeed.ThongBao);
    }

    private void UpsertThongBaoSeed(TaiKhoanSignupThanhCongSeed taiKhoan, int notificationId, JsonObject item)
    {
        var daDocTuServer = DocBoolTuObject(item, "read", "is_read", "da_doc");
        var tkguiIdServer = DocIntTuObject(
            item,
            "tkgui_id_server",
            "sender_tk_id_server",
            "sender_id",
            "from_user_id",
            "created_by");
        var thongBao = DuLieu.ThongBaoSeed.FirstOrDefault(x => x.NotificationIdServer == notificationId);
        if (thongBao is null)
        {
            var daDoc = daDocTuServer ?? false;
            DuLieu.ThongBaoSeed.Add(new ThongBaoSeed
            {
                NotificationIdServer = notificationId,
                TaiKhoanNhanIdServer = taiKhoan.TaiKhoanIdServer,
                TaiKhoanGuiIdServer = tkguiIdServer,
                Title = item["title"]?.ToString(),
                Content = item["content"]?.ToString(),
                ObjectIdServer = DocIntTuObject(item, "object_id", "object_id_server"),
                NotificationType = DocChuoiTuObject(item, "type", "notification_type"),
                DaDoc = daDoc,
                TrangThai = daDoc ? "da_doc" : "dang_luu",
                LayLuc = DateTimeOffset.Now,
                GhiChu = "Đồng bộ bởi testcase NOTIFICATION-GET-02."
            });
            return;
        }

        var daDocHienTai = daDocTuServer ?? thongBao.DaDoc ?? false;
        thongBao.TaiKhoanNhanIdServer = taiKhoan.TaiKhoanIdServer;
        thongBao.TaiKhoanGuiIdServer = tkguiIdServer ?? thongBao.TaiKhoanGuiIdServer;
        thongBao.Title = item["title"]?.ToString();
        thongBao.Content = item["content"]?.ToString();
        thongBao.ObjectIdServer = DocIntTuObject(item, "object_id", "object_id_server");
        thongBao.NotificationType = DocChuoiTuObject(item, "type", "notification_type");
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

    private static int? LayTamInt(YeuCauApi request, string ten)
    {
        return request.Tam.TryGetValue(ten, out var raw) && raw is int giaTri
            ? giaTri
            : null;
    }

    private static string? LayTamChuoi(YeuCauApi request, string ten)
    {
        return request.Tam.TryGetValue(ten, out var raw) && raw is string giaTri
            ? giaTri
            : null;
    }

}
