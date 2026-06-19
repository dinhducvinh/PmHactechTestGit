using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task TaoTinNhanSeedAsync()
    {
        var taiKhoan = LayTaiKhoanDaDangKySanSang();
        if (taiKhoan.Count < 2)
        {
            return;
        }

        var tokenCache = new Dictionary<int, string>();
        var coThayDoi = false;
        for (var i = 0; i < taiKhoan.Count && _nguCanh.CapNhatDB.DuLieu.TinNhanSeed.Count(x => x.TrangThai == "da_gui") < YeuCauDuLieuSeed.SoTinNhanMucTieu; i++)
        {
            var sender = taiKhoan[i];
            var receiver = taiKhoan.FirstOrDefault(x =>
                x.SoThuTu != sender.SoThuTu &&
                !CoQuanHeChan(sender.TaiKhoanIdServer, x.TaiKhoanIdServer));
            if (receiver is null || receiver.TaiKhoanIdServer is not > 0)
            {
                continue;
            }

            if (!tokenCache.TryGetValue(sender.SoThuTu, out var token))
            {
                token = await LayTokenSeedAsync(sender, $"tạo tin nhắn seed từ tài khoản {sender.SoThuTu}");

                tokenCache[sender.SoThuTu] = token;
            }

            var noiDung = $"Tin nhắn seed {DateTimeOffset.Now:yyyyMMddHHmmssfff}";
            var body = new Dictionary<string, object?>
            {
                ["to_id"] = IdChoBody(receiver.TaiKhoanIdServer),
                ["message"] = noiDung,
                ["type_message"] = "text"
            };

            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/conversation/send_message",
                body,
                token,
                $"tạo tin nhắn seed từ tài khoản {sender.SoThuTu} tới {receiver.SoThuTu}");

            var conversationId = DocIdSau(response.Data, "conversation_id");
            var messageId = DocIdSau(response.Data, "message_id");
            if (conversationId is not > 0 && messageId is not > 0)
            {
                continue;
            }

            _nguCanh.CapNhatDB.DuLieu.TinNhanSeed.Add(new TinNhanSeed
            {
                TinNhanSeedId = IdTiepTheo(_nguCanh.CapNhatDB.DuLieu.TinNhanSeed, x => x.TinNhanSeedId),
                ConversationIdServer = conversationId,
                MessageIdServer = messageId,
                SenderTaiKhoanIdServer = sender.TaiKhoanIdServer,
                ReceiverTaiKhoanIdServer = receiver.TaiKhoanIdServer,
                TypeMessage = "text",
                NoiDung = noiDung,
                TrangThai = "da_gui",
                TaoBoiTest = true,
                GuiLuc = DateTimeOffset.Now,
                GhiChu = "Tạo bằng API /conversation/send_message"
            });
            coThayDoi = true;
        }

        if (coThayDoi)
        {
            await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.TinNhan);
        }
    }
}
