using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> ConversationSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> ConversationKhongCoQuyen = Tap("1009");

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
                        ("to_id", IdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("message", "Tin nhắn kiểm thử token sai"),
                        ("type_message", "text")),
                    ctx.TokenSaiDinhDang);
            },
            ConversationSaiToken);

        Them(ds, "CONVERSATION-SEND-02", "Conversation", "Gửi tin nhắn text hợp lệ",
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
                        ("to_id", IdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("message", noiDung),
                        ("type_message", "text")),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiGui));
                req.Tam["nguoiGui"] = nguoiGui;
                req.Tam["nguoiNhan"] = nguoiNhan;
                req.Tam["noiDung"] = noiDung;
                req.Tam["typeMessage"] = "text";
                return req;
            },
            Ok,
            KiemTraGuiTinNhanTraVeId(),
            async (response, request, ctx) => await ctx.CapNhatDB.DongBoTinNhanTuResponseAsync(response, request));

        Them(ds, "CONVERSATION-SEND-03", "Conversation", "Gửi tin nhắn sản phẩm hợp lệ",
            "Token hợp lệ, to_id tồn tại, product_id lấy từ sanpham_seed; message có thể thiếu.",
            async ctx =>
            {
                var (nguoiGui, sanPham) = ChonCapTaiKhoanXemSanPhamKhongBiChan(ctx);
                var seller = LaySellerCuaSanPham(ctx, sanPham);
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", IdBatBuoc(seller.TaiKhoanIdServer, "seller tk_id_server")),
                        ("type_message", "text"),
                        ("product_id", IdBatBuoc(sanPham.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiGui));
                req.Tam["nguoiGui"] = nguoiGui;
                req.Tam["nguoiNhan"] = seller;
                req.Tam["typeMessage"] = "product_id";
                req.Tam["productId"] = sanPham.SanPhamIdServer;
                return req;
            },
            Ok,
            KiemTraGuiTinNhanTraVeId(),
            async (response, request, ctx) => await ctx.CapNhatDB.DongBoTinNhanTuResponseAsync(response, request));

        Them(ds, "CONVERSATION-SEND-04", "Conversation", "Gửi tin nhắn thiếu message và product_id",
            "Có to_id và type_message nhưng không gửi message/product_id.",
            async ctx =>
            {
                var (nguoiGui, nguoiNhan) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "Không tìm được cặp tài khoản đã đăng ký không bị block nhau để kiểm tra thiếu message/product_id.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", IdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("type_message", "text")),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiGui));
            },
            ThieuThamSo);

        Them(ds, "CONVERSATION-SEND-05", "Conversation", "Gửi tin nhắn tới user không tồn tại",
            "Token hợp lệ, to_id = 999999999.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/send_message",
                Obj(("to_id", 999999999), ("message", "Tin nhắn tới user không tồn tại"), ("type_message", "text")),
                await YeuCauTokenHopLeAsync(ctx)),
            KhongCoNguoiDung);

        Them(ds, "CONVERSATION-SEND-06", "Conversation", "Gửi tin nhắn tới user đang bị mình block",
            "Lấy một cặp trong tk_chan_seed, đăng nhập blocker rồi gửi tin nhắn tới blocked.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan để kiểm tra gửi tin nhắn khi bị block.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", IdBatBuoc(capChan.TaiKhoanBiChan.TaiKhoanIdServer, "blocked_tk_id_server")),
                        ("message", "Tin nhắn tới user đang bị block"),
                        ("type_message", "text")),
                    await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan));
            },
            ConversationKhongCoQuyen);

        Them(ds, "CONVERSATION-SEND-07", "Conversation", "Gửi tin nhắn tới user đã block mình",
            "Service hiện chỉ chặn chiều current user đã block to_id, nên chiều ngược vẫn có thể gửi thành công.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan để kiểm tra gửi tin nhắn khi bị block.");
                var noiDung = $"Tin nhan reverse block {DateTimeOffset.Now:yyyyMMddHHmmss}";
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", IdBatBuoc(capChan.TaiKhoanChan.TaiKhoanIdServer, "blocker_tk_id_server")),
                        ("message", noiDung),
                        ("type_message", "text")),
                    await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanBiChan));
                req.Tam["nguoiGui"] = capChan.TaiKhoanBiChan;
                req.Tam["nguoiNhan"] = capChan.TaiKhoanChan;
                req.Tam["noiDung"] = noiDung;
                req.Tam["typeMessage"] = "text";
                return req;
            },
            Ok,
            KiemTraGuiTinNhanTraVeId(),
            async (response, request, ctx) => await ctx.CapNhatDB.DongBoTinNhanTuResponseAsync(response, request));

        Them(ds, "CONVERSATION-SEND-08A", "Conversation", "Gửi tin nhắn cho chính mình",
            "Token hợp lệ nhưng to_id là id của chính current user.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", IdBatBuoc(taiKhoan.TaiKhoanIdServer, "current tk_id_server")),
                        ("message", "Tin nhắn gửi cho chính mình"),
                        ("type_message", "text")),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiGiaTri);

        Them(ds, "CONVERSATION-SEND-08B", "Conversation", "Gửi tin nhắn kèm product_id không tồn tại",
            "Token hợp lệ, to_id tồn tại nhưng product_id không tồn tại.",
            async ctx =>
            {
                var (nguoiGui, nguoiNhan) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "Không tìm được cặp tài khoản đã đăng ký không bị block nhau để gửi tin nhắn.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", IdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("message", "Tin nhắn kèm product không tồn tại"),
                        ("type_message", "text"),
                        ("product_id", 999999999)),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiGui));
            },
            SaiGiaTri);

        Them(ds, "CONVERSATION-SEND-09A", "Conversation", "Gửi tin nhắn thiếu to_id",
            "Token hợp lệ nhưng body thiếu to_id.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/send_message",
                Obj(("message", "Thiếu to_id"), ("type_message", "text")),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "CONVERSATION-SEND-09B", "Conversation", "Gửi tin nhắn thiếu type_message",
            "Token hợp lệ nhưng body thiếu type_message.",
            async ctx =>
            {
                var (nguoiGui, nguoiNhan) = ChonCapTaiKhoanKhongCoQuanHeChan(ctx,
                    "Không tìm được cặp tài khoản đã đăng ký không bị block nhau để kiểm tra thiếu type_message.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/send_message",
                    Obj(
                        ("to_id", IdBatBuoc(nguoiNhan.TaiKhoanIdServer, "receiver_tk_id_server")),
                        ("message", "Thiếu type_message")),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiGui));
            },
            ThieuThamSo);
    }

    private static void ThemLayDanhSachConversation(List<KichBanApi> ds)
    {
        Them(ds, "CONVERSATION-LIST-01", "Conversation", "Lấy danh sách conversation bằng token sai",
            "Token hết hạn/sai định dạng, body có index/count hợp lệ.",
            ctx => Req(HttpMethod.Post, "/conversation/get_list_conversation", Obj(("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
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
                    Obj(("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
                req.Tam["conversationId"] = tinNhan.ConversationIdServer;
                return req;
            },
            Ok,
            KiemTraDanhSachCoConversationSeed());

        Them(ds, "CONVERSATION-LIST-03", "Conversation", "Lấy danh sách conversation hết trang",
            "Token hợp lệ nhưng index rất lớn nên service trả data rỗng.",
            async ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                var taiKhoan = LayTaiKhoanTheoServerId(ctx, tinNhan.SenderTaiKhoanIdServer) ??
                               LayTaiKhoanTheoServerId(ctx, tinNhan.ReceiverTaiKhoanIdServer) ??
                               throw new BoQuaKiemThuException("Thiếu tài khoản liên quan tới tinnhan_seed.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_list_conversation",
                    Obj(("index", 999999), ("count", 20)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Ok,
            KiemTraDanhSachConversationRong());

        Them(ds, "CONVERSATION-LIST-04", "Conversation", "Lấy danh sách conversation thiếu index/count",
            "Body rỗng hoặc thiếu index/count.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/get_list_conversation",
                Obj(),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "CONVERSATION-LIST-05", "Conversation", "Lấy danh sách conversation index/count sai kiểu",
            "Gửi index/count dạng chuỗi.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/get_list_conversation",
                Obj(("index", "abc"), ("count", "xyz")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiKieu);

        Them(ds, "CONVERSATION-LIST-06", "Conversation", "Lấy danh sách conversation index/count sai giá trị",
            "Gửi index < 0 hoặc count < 1.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/get_list_conversation",
                Obj(("index", -1), ("count", 0)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static void ThemLayChiTietConversation(List<KichBanApi> ds)
    {
        Them(ds, "CONVERSATION-DETAIL-01", "Conversation", "Lấy nội dung conversation hợp lệ",
            "Dùng conversation_id_server từ tinnhan_seed để lấy danh sách tin nhắn.",
            async ctx =>
            {
                var tinNhan = LayTinNhanCoNoiDungDaGuiBatBuoc(ctx);
                var taiKhoan = LayTaiKhoanTheoServerId(ctx, tinNhan.SenderTaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản gửi tin nhắn trong taikhoan_signupthanhcong.");
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("conversation_id", IdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server")),
                        ("index", 0),
                        ("count", 20)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
                req.Tam["messageId"] = tinNhan.MessageIdServer;
                req.Tam["noiDung"] = tinNhan.NoiDung;
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
                        ("conversation_id", IdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server")),
                        ("index", 0),
                        ("count", 20)),
                    ctx.TokenSaiDinhDang);
            },
            ConversationSaiToken);

        Them(ds, "CONVERSATION-DETAIL-03", "Conversation", "Lấy nội dung conversation thiếu partner_id/conversation_id",
            "Token hợp lệ nhưng chỉ gửi index/count.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/get_conversation",
                Obj(("index", 0), ("count", 20)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "CONVERSATION-DETAIL-04", "Conversation", "Lấy nội dung conversation thiếu index/count",
            "Có conversation_id nhưng thiếu index/count.",
            async ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(("conversation_id", IdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server"))),
                    await YeuCauTokenHopLeAsync(ctx));
            },
            ThieuThamSo);

        Them(ds, "CONVERSATION-DETAIL-05", "Conversation", "Lấy nội dung với partner chưa có conversation",
            "Chọn cặp user tồn tại chưa có dòng tinnhan_seed giữa hai bên.",
            async ctx =>
            {
                var (nguoiDung, doiTac) = ChonCapTaiKhoanChuaCoConversation(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("partner_id", IdBatBuoc(doiTac.TaiKhoanIdServer, "partner tk_id_server")),
                        ("index", 0),
                        ("count", 20)),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiDung));
            },
            Ok,
            KiemTraChiTietConversationRong(mongDoiCanGui: true));

        Them(ds, "CONVERSATION-DETAIL-06", "Conversation", "Lấy nội dung với partner_id không tồn tại",
            "Token hợp lệ, partner_id = 999999999.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/get_conversation",
                Obj(("partner_id", 999999999), ("index", 0), ("count", 20)),
                await YeuCauTokenHopLeAsync(ctx)),
            KhongCoNguoiDung);

        Them(ds, "CONVERSATION-DETAIL-07", "Conversation", "Lấy nội dung với conversation_id không tồn tại",
            "Token hợp lệ, conversation_id = 999999999.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/get_conversation",
                Obj(("conversation_id", 999999999), ("index", 0), ("count", 20)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "CONVERSATION-DETAIL-08", "Conversation", "Lấy nội dung conversation có quan hệ block",
            "Chọn conversation trong tinnhan_seed giữa hai user có quan hệ block.",
            async ctx =>
            {
                var tinNhan = LayTinNhanCoQuanHeChanBatBuoc(ctx);
                var taiKhoan = LayTaiKhoanTheoServerId(ctx, tinNhan.SenderTaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản gửi tin nhắn trong taikhoan_signupthanhcong.");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("conversation_id", IdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server")),
                        ("index", 0),
                        ("count", 20)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Ok,
            KiemTraCanSendMessage(false));

        Them(ds, "CONVERSATION-DETAIL-09", "Conversation", "Lấy nội dung với partner chưa có conversation nhưng có block",
            "Chọn hai user chưa từng nhắn với nhau nhưng có quan hệ block trong tk_chan_seed.",
            async ctx =>
            {
                var (nguoiDung, doiTac) = ChonCapTaiKhoanChanChuaCoConversation(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("partner_id", IdBatBuoc(doiTac.TaiKhoanIdServer, "partner tk_id_server")),
                        ("index", 0),
                        ("count", 20)),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiDung));
            },
            Ok,
            KiemTraChiTietConversationRong(mongDoiCanGui: false));

        Them(ds, "CONVERSATION-DETAIL-10", "Conversation", "Lấy nội dung conversation index/count sai kiểu",
            "Có conversation_id hợp lệ nhưng index/count là chuỗi.",
            async ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("conversation_id", IdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server")),
                        ("index", "abc"),
                        ("count", "xyz")),
                    await YeuCauTokenHopLeAsync(ctx));
            },
            SaiKieu);

        Them(ds, "CONVERSATION-DETAIL-11", "Conversation", "Lấy nội dung conversation index/count sai giá trị",
            "Có conversation_id hợp lệ nhưng index < 0 hoặc count < 1.",
            async ctx =>
            {
                var tinNhan = LayTinNhanDaGuiBatBuoc(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/get_conversation",
                    Obj(
                        ("conversation_id", IdBatBuoc(tinNhan.ConversationIdServer, "tinnhan_seed.conversation_id_server")),
                        ("index", -1),
                        ("count", 0)),
                    await YeuCauTokenHopLeAsync(ctx));
            },
            SaiGiaTri);
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
                    Obj(("partner_id", IdBatBuoc(tinNhan.SenderTaiKhoanIdServer, "sender_tk_id_server"))),
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
                    Obj(("partner_id", IdBatBuoc(tinNhan.SenderTaiKhoanIdServer, "sender_tk_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, receiver));
            },
            Ok,
            KiemTraDataMangRong());

        Them(ds, "CONVERSATION-READ-03", "Conversation", "Đánh dấu đã đọc khi chưa có conversation",
            "Chọn cặp user tồn tại chưa có dòng tinnhan_seed giữa hai bên.",
            async ctx =>
            {
                var (nguoiDung, doiTac) = ChonCapTaiKhoanChuaCoConversation(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/set_read_message",
                    Obj(("partner_id", IdBatBuoc(doiTac.TaiKhoanIdServer, "partner tk_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, nguoiDung));
            },
            Ok,
            KiemTraDataMangRong());

        Them(ds, "CONVERSATION-READ-04", "Conversation", "Đánh dấu đã đọc thiếu partner_id",
            "Token hợp lệ nhưng body rỗng.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/set_read_message",
                Obj(),
                await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "CONVERSATION-READ-05", "Conversation", "Đánh dấu đã đọc với partner_id không tồn tại",
            "Token hợp lệ, partner_id = 999999999.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/conversation/set_read_message",
                Obj(("partner_id", 999999999)),
                await YeuCauTokenHopLeAsync(ctx)),
            KhongCoNguoiDung);

        Them(ds, "CONVERSATION-READ-06", "Conversation", "Đánh dấu đã đọc với partner_id là chính mình",
            "Token hợp lệ nhưng partner_id bằng id của current user.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/conversation/set_read_message",
                    Obj(("partner_id", IdBatBuoc(taiKhoan.TaiKhoanIdServer, "current tk_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraGuiTinNhanTraVeId()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var conversationId = DocIdSau(response.Data, "conversation_id");
            var messageId = DocIdSau(response.Data, "message_id");
            if (conversationId is not > 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu conversation_id."));
            }

            if (messageId is not > 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu message_id."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
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

            if (response.Json is not JsonObject root || !root.ContainsKey("num_new_message"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu num_new_message."));
            }

            var conversationId = request.Tam.TryGetValue("conversationId", out var id) ? id as int? : null;
            if (conversationId is not > 0)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var coConversation = mang.Any(item => CoIdTrongNode(item, conversationId.Value, "id", "conversation_id"));
            return Task.FromResult(coConversation
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"Không tìm thấy conversation_id_server `{conversationId}` trong danh sách conversation."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachConversationRong()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_list_conversation không phải mảng."));
            }

            if (mang.Count != 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data phải rỗng khi hết trang, thực tế có {mang.Count} item."));
            }

            var numNewMessage = response.Json?["num_new_message"]?.GetValue<int>();
            return Task.FromResult(numNewMessage is null or 0
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"num_new_message phải bằng 0 khi hết trang, thực tế là {numNewMessage}."));
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

            if (!data.ContainsKey("can_send_message"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data thiếu can_send_message."));
            }

            var messageId = request.Tam.TryGetValue("messageId", out var id) ? id as int? : null;
            if (messageId is not > 0)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var coTinNhan = messages.Any(item => CoIdTrongNode(item, messageId.Value, "message_id"));
            var noiDung = request.Tam.TryGetValue("noiDung", out var noiDungRaw) ? noiDungRaw as string : null;
            if (!coTinNhan && !string.IsNullOrWhiteSpace(noiDung))
            {
                coTinNhan = messages.Any(item =>
                    string.Equals(item?["message"]?.ToString(), noiDung, StringComparison.OrdinalIgnoreCase));
            }

            if (!coTinNhan && string.IsNullOrWhiteSpace(noiDung))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            return Task.FromResult(coTinNhan
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"Không tìm thấy message_id_server `{messageId}` trong messages."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraChiTietConversationRong(bool mongDoiCanGui)
    {
        return (response, _, _) =>
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

            if (messages.Count != 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"messages phải rỗng, thực tế có {messages.Count} item."));
            }

            var canSend = data["can_send_message"]?.GetValue<bool>();
            return Task.FromResult(canSend == mongDoiCanGui
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data.can_send_message phải bằng {mongDoiCanGui}, thực tế là {canSend?.ToString() ?? "null"}."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraCanSendMessage(bool mongDoi)
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_conversation không phải object."));
            }

            var canSend = data["can_send_message"]?.GetValue<bool>();
            return Task.FromResult(canSend == mongDoi
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data.can_send_message phải bằng {mongDoi}, thực tế là {canSend?.ToString() ?? "null"}."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDataMangRong()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data không phải mảng."));
            }

            return Task.FromResult(mang.Count == 0
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data phải rỗng, thực tế có {mang.Count} item."));
        };
    }

    private static bool CoIdTrongNode(JsonNode? node, int idCanTim, params string[] truongId)
    {
        foreach (var truong in truongId)
        {
            var raw = node?[truong]?.ToString();
            if (int.TryParse(raw, out var id) && id == idCanTim)
            {
                return true;
            }
        }

        return false;
    }

    private static int? DocIdSau(JsonNode? node, string tenTruong)
    {
        return node?[tenTruong]?.GetValue<int>();
    }

}







