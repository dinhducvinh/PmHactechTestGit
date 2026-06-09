using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> ConversationSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> ConversationKhongCoQuyen = Tap("1009", "9998", "HTTP_401", "HTTP_403");

    private static void ThemKichBanConversation(List<KichBanApi> ds)
    {
        ThemGuiTinNhan(ds);
        ThemLayDanhSachConversation(ds);
        ThemLayChiTietConversation(ds);
        ThemDanhDauDaDocTinNhan(ds);
    }

    private static void ThemGuiTinNhan(List<KichBanApi> ds)
    {
        Them(ds, "CONVERSATION-SEND-01", "Conversation", "Gửi tin nhắn bằng token sai",
            "Token hết hạn/sai định dạng, body có to_id/message/type_message hợp lệ.",
            ctx =>
            {
                var (nguoiGui, nguoiNhan) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "Không tìm được cặp tài khoản đã đăng ký không bị block nhau để gửi tin nhắn.");
                return Req(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", SoIdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("message", "Tin nhắn kiểm thử token sai"),
                        ("type_message", "text")),
                    ctx.TokenSaiDinhDang);
            },
            ConversationSaiToken);

        Them(ds, "CONVERSATION-SEND-02", "Conversation", "Gửi tin nhắn hợp lệ",
            "Đăng nhập user A, chọn user B không có quan hệ block, gửi tin nhắn text và đồng bộ vào tinnhan_seed.",
            async ctx =>
            {
                var (nguoiGui, nguoiNhan) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "Không tìm được cặp tài khoản đã đăng ký không bị block nhau để gửi tin nhắn.");
                var noiDung = $"Tin nhan testcase {DateTimeOffset.Now:yyyyMMddHHmmss}";
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", SoIdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("message", noiDung),
                        ("type_message", "text")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(nguoiGui));
                req.Tam["nguoiGui"] = nguoiGui;
                req.Tam["nguoiNhan"] = nguoiNhan;
                req.Tam["noiDung"] = noiDung;
                req.Tam["typeMessage"] = "text";
                return req;
            },
            Ok,
            KiemTraGuiTinNhanTraVeId(),
            DongBoTinNhanTuResponseAsync);

        Them(ds, "CONVERSATION-SEND-03", "Conversation", "Gửi tin nhắn tới user không tồn tại",
            "Token hợp lệ, to_id = 999999999.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/send_message",
                Obj(("to_id", 999999999), ("message", "Tin nhắn tới user không tồn tại"), ("type_message", "text")),
                await ctx.YeuCauTokenHopLeAsync()),
            KhongCoNguoiDung);

        Them(ds, "CONVERSATION-SEND-04A", "Conversation", "Gửi tin nhắn tới user đang bị mình block",
            "Lấy một cặp trong tk_chan_seed, đăng nhập blocker rồi gửi tin nhắn tới blocked.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan để kiểm tra gửi tin nhắn khi bị block.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", SoIdBatBuoc(capChan.TaiKhoanBiChan.TaiKhoanIdServer, "blocked_tk_id_server")),
                        ("message", "Tin nhắn tới user đang bị block"),
                        ("type_message", "text")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(capChan.TaiKhoanChan));
            },
            ConversationKhongCoQuyen);

        Them(ds, "CONVERSATION-SEND-04B", "Conversation", "Gửi tin nhắn tới user đã block mình",
            "Lấy một cặp trong tk_chan_seed, đăng nhập blocked rồi gửi tin nhắn tới blocker.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan để kiểm tra gửi tin nhắn khi bị block.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", SoIdBatBuoc(capChan.TaiKhoanChan.TaiKhoanIdServer, "blocker_tk_id_server")),
                        ("message", "Tin nhắn tới user đã block mình"),
                        ("type_message", "text")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(capChan.TaiKhoanBiChan));
            },
            ConversationKhongCoQuyen);

        Them(ds, "CONVERSATION-SEND-05A", "Conversation", "Gửi tin nhắn cho chính mình",
            "Token hợp lệ nhưng to_id là id của chính current user.",
            async ctx =>
            {
                var taiKhoan = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", SoIdBatBuoc(taiKhoan.TaiKhoanIdServer, "current tk_id_server")),
                        ("message", "Tin nhắn gửi cho chính mình"),
                        ("type_message", "text")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(taiKhoan));
            },
            SaiGiaTri);

        Them(ds, "CONVERSATION-SEND-05B", "Conversation", "Gửi tin nhắn kèm product_id không tồn tại",
            "Token hợp lệ, to_id tồn tại nhưng product_id không tồn tại.",
            async ctx =>
            {
                var (nguoiGui, nguoiNhan) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "Không tìm được cặp tài khoản đã đăng ký không bị block nhau để gửi tin nhắn.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", SoIdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("message", "Tin nhắn kèm product không tồn tại"),
                        ("type_message", "text"),
                        ("product_id", 999999999)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(nguoiGui));
            },
            SaiGiaTri);
    }

    private static void ThemLayDanhSachConversation(List<KichBanApi> ds)
    {
        Them(ds, "CONVERSATION-LIST-01", "Conversation", "Lấy danh sách conversation bằng token sai",
            "Token hết hạn/sai định dạng, body có index/count hợp lệ.",
            ctx => Req(HttpMethod.Post, "/conversation/get_list_conversation", Obj(("index", 1), ("count", 20)), ctx.TokenSaiDinhDang),
            ConversationSaiToken);

        Them(ds, "CONVERSATION-LIST-02", "Conversation", "Lấy danh sách conversation hợp lệ",
            "Đăng nhập một tài khoản có liên quan trong tinnhan_seed, gọi index/count hợp lệ.",
            async ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                var taiKhoan = LayTaiKhoanTheoServerId(ctx, tinNhan.SenderTaiKhoanIdServer) ??
                               LayTaiKhoanTheoServerId(ctx, tinNhan.ReceiverTaiKhoanIdServer) ??
                               throw new BoQuaKiemThuException("Thiếu tài khoản liên quan tới tinnhan_seed.");
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_list_conversation",
                    Obj(("index", 1), ("count", 20)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(taiKhoan));
                req.Tam["conversationId"] = tinNhan.ConversationIdServer;
                return req;
            },
            Ok,
            KiemTraDanhSachCoConversationSeed());
    }

    private static void ThemLayChiTietConversation(List<KichBanApi> ds)
    {
        Them(ds, "CONVERSATION-DETAIL-01", "Conversation", "Lấy nội dung conversation hợp lệ",
            "Dùng conversation_id_server từ tinnhan_seed để lấy danh sách tin nhắn.",
            async ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                var taiKhoan = LayTaiKhoanTheoServerId(ctx, tinNhan.SenderTaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản gửi tin nhắn trong taikhoan_signupthanhcong.");
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("conversation_id", SoIdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server")),
                        ("index", 1),
                        ("count", 20)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(taiKhoan));
                req.Tam["messageId"] = tinNhan.MessageIdServer;
                return req;
            },
            Ok,
            KiemTraChiTietCoTinNhanSeed());

        Them(ds, "CONVERSATION-DETAIL-02", "Conversation", "Lấy nội dung conversation bằng token sai",
            "Token hết hạn/sai định dạng, body hợp lệ.",
            ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                return Req(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("conversation_id", SoIdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server")),
                        ("index", 1),
                        ("count", 20)),
                    ctx.TokenSaiDinhDang);
            },
            ConversationSaiToken);

        Them(ds, "CONVERSATION-DETAIL-03", "Conversation", "Lấy nội dung conversation thiếu partner_id/conversation_id",
            "Token hợp lệ nhưng chỉ gửi index/count.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/get_conversation",
                Obj(("index", 1), ("count", 20)),
                await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);
    }

    private static void ThemDanhDauDaDocTinNhan(List<KichBanApi> ds)
    {
        Them(ds, "CONVERSATION-READ-01", "Conversation", "Đánh dấu đã đọc bằng token sai",
            "Token hết hạn/sai định dạng, body có partner_id hợp lệ.",
            ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                return Req(
                    HttpMethod.Post,
                    "/conversation/set_read_message",
                    Obj(("partner_id", SoIdBatBuoc(tinNhan.SenderTaiKhoanIdServer, "sender_tk_id_server"))),
                    ctx.TokenSaiDinhDang);
            },
            ConversationSaiToken);

        Them(ds, "CONVERSATION-READ-02", "Conversation", "Đánh dấu tin nhắn đã đọc",
            "Đăng nhập receiver của một dòng tinnhan_seed, partner_id là sender.",
            async ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                var receiver = LayTaiKhoanTheoServerId(ctx, tinNhan.ReceiverTaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản nhận tin nhắn trong taikhoan_signupthanhcong.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/set_read_message",
                    Obj(("partner_id", SoIdBatBuoc(tinNhan.SenderTaiKhoanIdServer, "sender_tk_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(receiver));
            },
            Ok);

        Them(ds, "CONVERSATION-READ-03", "Conversation", "Đánh dấu đã đọc với partner_id không tồn tại",
            "Token hợp lệ, partner_id = 999999999.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/set_read_message",
                Obj(("partner_id", 999999999)),
                await ctx.YeuCauTokenHopLeAsync()),
            KhongCoNguoiDung);
    }

    private static TinNhanSeed LayTinNhanDaGuiBatBuoc(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.TinNhanSeed
            .Where(x => x.TrangThai == "da_gui")
            .Where(x => !string.IsNullOrWhiteSpace(x.ConversationIdServer))
            .Where(x => !string.IsNullOrWhiteSpace(x.SenderTaiKhoanIdServer))
            .Where(x => !string.IsNullOrWhiteSpace(x.ReceiverTaiKhoanIdServer))
            .OrderByDescending(x => x.GuiLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.TinNhanSeedId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu dữ liệu tinnhan_seed trạng thái da_gui. Hãy bấm Kiểm tra seed để tạo tin nhắn mồi.");
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraGuiTinNhanTraVeId()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var conversationId = DocChuoiSau(response.Data, "conversation_id", "conversationId");
            var messageId = DocChuoiSau(response.Data, "message_id", "messageId", "id");
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu conversation_id."));
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu message_id."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static async Task DongBoTinNhanTuResponseAsync(PhanHoiApi response, YeuCauApi request, NguCanhKiemThu ctx)
    {
        if (!LaMaThanhCong(response))
        {
            return;
        }

        var conversationId = DocChuoiSau(response.Data, "conversation_id", "conversationId");
        var messageId = DocChuoiSau(response.Data, "message_id", "messageId", "id");
        if (string.IsNullOrWhiteSpace(conversationId) || string.IsNullOrWhiteSpace(messageId))
        {
            return;
        }

        var nguoiGui = (TaiKhoanSignupThanhCongSeed)request.Tam["nguoiGui"]!;
        var nguoiNhan = (TaiKhoanSignupThanhCongSeed)request.Tam["nguoiNhan"]!;
        var noiDung = (string)request.Tam["noiDung"]!;
        var typeMessage = (string)request.Tam["typeMessage"]!;

        var tinNhanDaCo = ctx.KhoSeed.DuLieu.TinNhanSeed.FirstOrDefault(x => x.MessageIdServer == messageId);
        if (tinNhanDaCo is null)
        {
            ctx.KhoSeed.DuLieu.TinNhanSeed.Add(new TinNhanSeed
            {
                TinNhanSeedId = TaoIdTinNhanMoi(ctx),
                ConversationIdServer = conversationId,
                MessageIdServer = messageId,
                SenderTaiKhoanIdServer = nguoiGui.TaiKhoanIdServer,
                ReceiverTaiKhoanIdServer = nguoiNhan.TaiKhoanIdServer,
                TypeMessage = typeMessage,
                NoiDung = noiDung,
                TrangThai = "da_gui",
                TaoBoiTest = true,
                GuiLuc = DateTimeOffset.Now,
                GhiChu = "Dong bo boi testcase CONVERSATION-SEND-02"
            });
        }
        else
        {
            tinNhanDaCo.ConversationIdServer = conversationId;
            tinNhanDaCo.SenderTaiKhoanIdServer = nguoiGui.TaiKhoanIdServer;
            tinNhanDaCo.ReceiverTaiKhoanIdServer = nguoiNhan.TaiKhoanIdServer;
            tinNhanDaCo.TypeMessage = typeMessage;
            tinNhanDaCo.NoiDung = noiDung;
            tinNhanDaCo.TrangThai = "da_gui";
            tinNhanDaCo.GuiLuc = DateTimeOffset.Now;
        }

        await ctx.KhoSeed.LuuAsync();
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachCoConversationSeed()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_list_conversation không phải mảng."));
            }

            if (!TienIchJson.CoTruong(response.Json, "num_new_message"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu num_new_message."));
            }

            var conversationId = request.Tam["conversationId"] as string;
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var coConversation = mang.Any(item => CoIdTrongNode(item, conversationId, "conversation_id", "conversationId", "id"));
            return Task.FromResult(coConversation
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"Không tìm thấy conversation_id_server `{conversationId}` trong danh sách conversation."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraChiTietCoTinNhanSeed()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_conversation không phải object."));
            }

            if (data["messages"] is not JsonArray messages)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data thiếu mảng messages."));
            }

            if (!TienIchJson.CoTruong(data, "can_send_message"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data thiếu can_send_message."));
            }

            var messageId = request.Tam["messageId"] as string;
            if (string.IsNullOrWhiteSpace(messageId))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var coTinNhan = messages.Any(item => CoIdTrongNode(item, messageId, "message_id", "messageId", "id"));
            return Task.FromResult(coTinNhan
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"Không tìm thấy message_id_server `{messageId}` trong messages."));
        };
    }

    private static bool CoIdTrongNode(JsonNode? node, string idCanTim, params string[] truongId)
    {
        return truongId.Any(truong =>
            string.Equals(TienIchJson.DocChuoi(node, truong), idCanTim, StringComparison.Ordinal));
    }

    private static string? DocChuoiSau(JsonNode? node, params string[] tenTruong)
    {
        var trucTiep = TienIchJson.DocChuoi(node, tenTruong);
        if (!string.IsNullOrWhiteSpace(trucTiep))
        {
            return trucTiep;
        }

        if (node is not JsonObject obj)
        {
            return null;
        }

        foreach (var tenNodeCon in new[] { "message", "conversation", "data" })
        {
            var tuNodeCon = TienIchJson.DocChuoi(obj[tenNodeCon], tenTruong);
            if (!string.IsNullOrWhiteSpace(tuNodeCon))
            {
                return tuNodeCon;
            }
        }

        return null;
    }

    private static int TaoIdTinNhanMoi(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.TinNhanSeed.Count == 0
            ? 1
            : ctx.KhoSeed.DuLieu.TinNhanSeed.Max(x => x.TinNhanSeedId) + 1;
    }
}
