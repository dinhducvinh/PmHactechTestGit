using HactechTest.ApiShopTesting.Core;


namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task DongBoThongBaoSeedAsync()
    {
        try
        {
            var taiKhoan = LayTaiKhoanDaDangKySanSang().Take(10).ToList();
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

                var body = new Dictionary<string, object?> { ["index"] = 1, ["count"] = 20, ["group"] = 0 };
                var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/notification/get_notification", body, token));
                if (response.MaSoSanh != "1000")
                {
                    continue;
                }

                foreach (var item in LayObjectTuNode(response.Data))
                {
                    var notificationId = item["id"]?.GetValue<int>();
                    if (notificationId is not > 0)
                    {
                        continue;
                    }

                    var daDoc = item["read"]?.GetValue<bool>();
                    var hienCo = _nguCanh.CapNhatDB.DuLieu.ThongBaoSeed.FirstOrDefault(x => x.NotificationIdServer == notificationId.Value);
                    if (hienCo is null)
                    {
                        _nguCanh.CapNhatDB.DuLieu.ThongBaoSeed.Add(new ThongBaoSeed
                        {
                            NotificationIdServer = notificationId.Value,
                            TaiKhoanIdServer = tk.TaiKhoanIdServer,
                            Title = item["title"]?.ToString(),
                            Content = item["content"]?.ToString(),
                            ObjectIdServer = item["object_id"]?.GetValue<int>(),
                            NotificationType = item["type"]?.ToString(),
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
                        hienCo.ObjectIdServer = item["object_id"]?.GetValue<int>();
                        hienCo.NotificationType = item["type"]?.ToString();
                        hienCo.DaDoc = daDoc;
                        hienCo.TrangThai = daDoc == true ? "da_doc" : "dang_luu";
                        hienCo.LayLuc = DateTimeOffset.Now;
                    }
                }
            }

            await _nguCanh.CapNhatDB.LuuAsync();
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
}



