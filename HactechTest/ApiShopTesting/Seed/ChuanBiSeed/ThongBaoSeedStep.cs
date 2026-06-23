using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task DongBoThongBaoSeedAsync()
    {
        try
        {
            await TaoThongBaoSeedConThieuAsync();

            var taiKhoan = LayTaiKhoanDaDangKySanSang().Take(10).ToList();
            var coThayDoi = false;
            foreach (var tk in taiKhoan)
            {
                string token;
                try
                {
                    token = await HelperTC.LayTokenCuaTaiKhoanAsync(_nguCanh, tk);
                }
                catch (BoQuaKiemThuException)
                {
                    continue;
                }

                var body = new Dictionary<string, object?> { ["index"] = 0, ["count"] = 20 };
                var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/notification/get_notification", body, token));
                if (response.MaSoSanh != "1000")
                {
                    continue;
                }

                foreach (var item in LayObjectTuNode(response.Data))
                {
                    var notificationId = DocNotificationIdServer(item);
                    if (notificationId is not > 0)
                    {
                        continue;
                    }

                    var daDoc = DocBoolTuObject(item, "read", "is_read", "da_doc");
                    var hienCo = _nguCanh.CapNhatDB.DuLieu.ThongBaoSeed.FirstOrDefault(x => x.NotificationIdServer == notificationId.Value);
                    if (hienCo is null)
                    {
                        _nguCanh.CapNhatDB.DuLieu.ThongBaoSeed.Add(new ThongBaoSeed
                        {
                            NotificationIdServer = notificationId.Value,
                            TaiKhoanIdServer = tk.TaiKhoanIdServer,
                            Title = item["title"]?.ToString(),
                            Content = item["content"]?.ToString(),
                            ObjectIdServer = DocIntTuObject(item, "object_id", "object_id_server"),
                            NotificationType = DocChuoiTuObject(item, "type", "notification_type"),
                            DaDoc = daDoc,
                            TrangThai = daDoc == true ? "da_doc" : "dang_luu",
                            LayLuc = DateTimeOffset.Now
                        });
                    }
                    else
                    {
                        hienCo.TaiKhoanIdServer = tk.TaiKhoanIdServer;
                        hienCo.Title = item["title"]?.ToString();
                        hienCo.Content = item["content"]?.ToString();
                        hienCo.ObjectIdServer = DocIntTuObject(item, "object_id", "object_id_server");
                        hienCo.NotificationType = DocChuoiTuObject(item, "type", "notification_type");
                        hienCo.DaDoc = daDoc;
                        hienCo.TrangThai = daDoc == true ? "da_doc" : "dang_luu";
                        hienCo.LayLuc = DateTimeOffset.Now;
                    }

                    coThayDoi = true;
                }
            }

            if (coThayDoi)
            {
                await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.ThongBao);
            }
        }
        catch (HttpRequestException)
        {
            return;
        }
        catch (TaskCanceledException)
        {
            return;
        }
    }

    private async Task TaoThongBaoSeedConThieuAsync()
    {
        var taiKhoan = LayTaiKhoanDaDangKySanSang();
        if (taiKhoan.Count < 2)
        {
            return;
        }

        var tokenCache = new Dictionary<int, string>();
        var coThayDoi = false;
        var soLanThu = 0;
        while (_nguCanh.CapNhatDB.DuLieu.ThongBaoSeed.Count(ThongBaoDangLuu) < YeuCauDuLieuSeed.SoThongBaoMucTieu &&
               soLanThu < taiKhoan.Count * 2)
        {
            soLanThu++;
            var cap = ChonCapTaiKhoanNhanThongBao(taiKhoan);
            if (cap is null)
            {
                break;
            }

            var nguoiGui = cap.Value.NguoiGui;
            var nguoiNhan = cap.Value.NguoiNhan;
            if (!tokenCache.TryGetValue(nguoiGui.SoThuTu, out var token))
            {
                token = await LayTokenSeedAsync(nguoiGui, $"tạo thông báo seed cho tài khoản {nguoiGui.SoThuTu}");
                tokenCache[nguoiGui.SoThuTu] = token;
            }

            var thongTin = ChonThongTinObjectThongBao();
            var title = $"Thông báo seed {DateTimeOffset.Now:yyyyMMddHHmmssfff}";
            var body = new Dictionary<string, object?>
            {
                ["type"] = thongTin.Type,
                ["object_id"] = thongTin.ObjectId,
                ["title"] = title,
                ["user_id"] = IdChoBody(nguoiNhan.TaiKhoanIdServer)
            };

            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/notification/add_notification",
                body,
                token,
                $"tạo thông báo seed từ tài khoản {nguoiGui.SoThuTu} tới {nguoiNhan.SoThuTu}");

            var notificationId = DocNotificationIdServer(response.Data);
            if (notificationId is not > 0)
            {
                throw new LoiChuanBiKiemThuException("API /notification/add_notification thành công nhưng không trả data.id.");
            }

            UpsertThongBaoSauTao(notificationId.Value, nguoiGui, nguoiNhan, thongTin.Type, thongTin.ObjectId, title);
            coThayDoi = true;
        }

        if (coThayDoi)
        {
            await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.ThongBao);
        }
    }

    private (TaiKhoanSignupThanhCongSeed NguoiGui, TaiKhoanSignupThanhCongSeed NguoiNhan)? ChonCapTaiKhoanNhanThongBao(
        IReadOnlyList<TaiKhoanSignupThanhCongSeed> taiKhoan)
    {
        foreach (var nguoiGui in taiKhoan)
        {
            var nguoiNhan = taiKhoan.FirstOrDefault(x =>
                x.TaiKhoanIdServer != nguoiGui.TaiKhoanIdServer &&
                !CoQuanHeChan(nguoiGui.TaiKhoanIdServer, x.TaiKhoanIdServer));
            if (nguoiNhan is not null)
            {
                return (nguoiGui, nguoiNhan);
            }
        }

        return null;
    }

    private (string Type, int ObjectId) ChonThongTinObjectThongBao()
    {
        var donHang = _nguCanh.CapNhatDB.DuLieu.DonHangSeed
            .Where(x => x.DonHangIdServer is > 0 && x.TrangThai != "da_xoa")
            .OrderBy(x => x.DonHangIdServer)
            .FirstOrDefault();
        if (donHang?.DonHangIdServer is > 0)
        {
            return ("order", donHang.DonHangIdServer.Value);
        }

        var sanPham = _nguCanh.CapNhatDB.DuLieu.SanPhamSeed
            .Where(x => x.SanPhamIdServer is > 0)
            .OrderBy(x => x.ThuTuNoiBo)
            .FirstOrDefault();
        if (sanPham?.SanPhamIdServer is > 0)
        {
            return ("product", sanPham.SanPhamIdServer.Value);
        }

        var tinNhan = _nguCanh.CapNhatDB.DuLieu.TinNhanSeed
            .Where(x => x.MessageIdServer is > 0)
            .OrderBy(x => x.TinNhanSeedId)
            .FirstOrDefault();
        if (tinNhan?.MessageIdServer is > 0)
        {
            return ("message", tinNhan.MessageIdServer.Value);
        }

        return ("system", Math.Max(1, _nguCanh.CapNhatDB.DuLieu.ThongBaoSeed.Count + 1));
    }

    private void UpsertThongBaoSauTao(
        int notificationId,
        TaiKhoanSignupThanhCongSeed nguoiGui,
        TaiKhoanSignupThanhCongSeed nguoiNhan,
        string notificationType,
        int objectId,
        string title)
    {
        var thongBao = _nguCanh.CapNhatDB.DuLieu.ThongBaoSeed
            .FirstOrDefault(x => x.NotificationIdServer == notificationId);
        if (thongBao is null)
        {
            thongBao = new ThongBaoSeed
            {
                ThongBaoSeedId = IdTiepTheo(_nguCanh.CapNhatDB.DuLieu.ThongBaoSeed, x => x.ThongBaoSeedId),
                NotificationIdServer = notificationId
            };
            _nguCanh.CapNhatDB.DuLieu.ThongBaoSeed.Add(thongBao);
        }

        thongBao.TaiKhoanGuiIdServer = nguoiGui.TaiKhoanIdServer;
        thongBao.TaiKhoanNhanIdServer = nguoiNhan.TaiKhoanIdServer;
        thongBao.NotificationType = notificationType;
        thongBao.ObjectIdServer = objectId;
        thongBao.Title = title;
        thongBao.DaDoc = false;
        thongBao.TrangThai = "dang_luu";
        thongBao.LayLuc = DateTimeOffset.Now;
        thongBao.GhiChu = "Tạo bằng API /notification/add_notification.";
    }

    private static bool ThongBaoDangLuu(ThongBaoSeed thongBao)
    {
        return thongBao.NotificationIdServer is > 0 &&
               thongBao.TaiKhoanNhanIdServer is > 0 &&
               string.Equals(thongBao.TrangThai, "dang_luu", StringComparison.OrdinalIgnoreCase);
    }

}



