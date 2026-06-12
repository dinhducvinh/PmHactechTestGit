using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanFollowBlock(List<KichBanApi> ds)
    {
        ThemFollowVaUnfollow(ds);
        ThemLayDanhSachNguoiTheoDoi(ds);
        ThemLayDanhSachDangTheoDoi(ds);
        ThemBlockVaUnblock(ds);
        ThemLayDanhSachDangBlock(ds);
    }

    private static void ThemFollowVaUnfollow(List<KichBanApi> ds)
    {
        Them(ds, "FOLLOW-SET-01", "FollowBlock", "Follow/unfollow với token không hợp lệ",
            "Gọi /set_user_follow bằng token sai định dạng.",
            ctx => Req(HttpMethod.Post, "/set_user_follow", Obj(("followee_id", 999999999), ("action", "follow")), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "FOLLOW-SET-02", "FollowBlock", "Follow user hợp lệ",
            "Chọn cặp user chưa có quan hệ follow.",
            async ctx =>
            {
                var (taiKhoanTheoDoi, taiKhoanDuocTheoDoi) = ChonCapTaiKhoanChuaTheoDoi(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", IdBatBuoc(taiKhoanDuocTheoDoi.TaiKhoanIdServer, "followee_tk_id_server")), ("action", "follow")),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoanTheoDoi));
                req.Tam["taiKhoanTheoDoi"] = taiKhoanTheoDoi;
                req.Tam["taiKhoanDuocTheoDoi"] = taiKhoanDuocTheoDoi;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var taiKhoanTheoDoi = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoanTheoDoi"]!;
                var taiKhoanDuocTheoDoi = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoanDuocTheoDoi"]!;
                await ctx.CapNhatDB.ThemQuanHeTheoDoiAsync(taiKhoanTheoDoi, taiKhoanDuocTheoDoi);
            });

        Them(ds, "FOLLOW-SET-03", "FollowBlock", "Unfollow user đang theo dõi",
            "Lấy quan hệ active trong tk_theodoi_seed và gọi unfollow.",
            async ctx =>
            {
                var capTheoDoi = LayCapTaiKhoanDangTheoDoi(ctx, "Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follow = capTheoDoi.QuanHeTheoDoi;
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", IdBatBuoc(follow.FolloweeTaiKhoanIdServer, "followee_tk_id_server")), ("action", "unfollow")),
                    await LayTokenCuaTaiKhoanAsync(ctx, capTheoDoi.TaiKhoanTheoDoi));
                req.Tam["follow"] = follow;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var follow = (TaiKhoanTheoDoiSeed)request.Tam["follow"]!;
                await ctx.CapNhatDB.XoaQuanHeTheoDoiDangHoatDongAsync(follow);
            });

        Them(ds, "FOLLOW-SET-04", "FollowBlock", "Follow user không tồn tại",
            "followee_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                Obj(("followee_id", 999999999), ("action", "follow")),
                await YeuCauTokenHopLeAsync(ctx)),
            KhongCoNguoiDung);

        Them(ds, "FOLLOW-SET-05", "FollowBlock", "Follow chính mình",
            "followee_id là id của current user.",
            async ctx =>
            {
                var tk = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", IdBatBuoc(tk.TaiKhoanIdServer, "tk_id_server")), ("action", "follow")),
                    await LayTokenCuaTaiKhoanAsync(ctx, tk));
            },
            SaiGiaTri);

        Them(ds, "FOLLOW-SET-06", "FollowBlock", "Follow lại user đã theo dõi",
            "Lấy quan hệ đang active trong tk_theodoi_seed và gọi follow lại.",
            async ctx =>
            {
                var capTheoDoi = LayCapTaiKhoanDangTheoDoi(ctx, "Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follow = capTheoDoi.QuanHeTheoDoi;
                return new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", IdBatBuoc(follow.FolloweeTaiKhoanIdServer, "followee_tk_id_server")), ("action", "follow")),
                    await LayTokenCuaTaiKhoanAsync(ctx, capTheoDoi.TaiKhoanTheoDoi));
            },
            DaThucHienTruocDo);

        Them(ds, "FOLLOW-SET-07", "FollowBlock", "Unfollow user chưa theo dõi",
            "Chọn cặp user chưa có quan hệ follow và gọi unfollow.",
            async ctx =>
            {
                var (taiKhoanTheoDoi, taiKhoanDuocTheoDoi) = ChonCapTaiKhoanChuaTheoDoi(ctx);
                return new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", IdBatBuoc(taiKhoanDuocTheoDoi.TaiKhoanIdServer, "followee_tk_id_server")), ("action", "unfollow")),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoanTheoDoi));
            },
            DaThucHienTruocDo);
    }

    private static void ThemLayDanhSachNguoiTheoDoi(List<KichBanApi> ds)
    {
        Them(ds, "FOLLOWED-LIST-01", "FollowBlock", "Lấy người theo dõi với token không hợp lệ",
            "Token sai, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/get_list_followed", Obj(("user_id", 999999999), ("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "FOLLOWED-LIST-02", "FollowBlock", "Lấy danh sách người theo dõi",
            "Dùng tài khoản được theo dõi từ tk_theodoi_seed.",
            async ctx =>
            {
                var capTheoDoi = LayCapTaiKhoanDangTheoDoi(ctx, "Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follow = capTheoDoi.QuanHeTheoDoi;
                return new YeuCauApi(HttpMethod.Post, "/get_list_followed",
                    Obj(("user_id", IdBatBuoc(follow.FolloweeTaiKhoanIdServer, "followee_tk_id_server")), ("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, capTheoDoi.TaiKhoanTheoDoi));
            },
            Ok,
            DataLaMang());

        Them(ds, "FOLLOWED-LIST-03", "FollowBlock", "Lấy người theo dõi của user không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/get_list_followed",
                Obj(("user_id", 999999999), ("index", 0), ("count", 10)),
                await YeuCauTokenHopLeAsync(ctx)),
            KhongCoNguoiDung);

        Them(ds, "FOLLOWED-LIST-04", "FollowBlock", "Lấy người theo dõi của user đang bị block",
            "Dùng cặp active trong tk_chan_seed.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan.");
                var chan = capChan.QuanHeChan;
                return new YeuCauApi(HttpMethod.Post, "/get_list_followed",
                    Obj(("user_id", IdBatBuoc(chan.BlockedTaiKhoanIdServer, "blocked_tk_id_server")), ("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan));
            },
            KhongCoQuyen);
    }

    private static void ThemLayDanhSachDangTheoDoi(List<KichBanApi> ds)
    {
        Them(ds, "FOLLOWING-LIST-01", "FollowBlock", "Lấy following với token không hợp lệ",
            "Token sai, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/get_list_following", Obj(("user_id", 999999999), ("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "FOLLOWING-LIST-02", "FollowBlock", "Lấy danh sách user đang follow",
            "Dùng tài khoản theo dõi từ tk_theodoi_seed.",
            async ctx =>
            {
                var capTheoDoi = LayCapTaiKhoanDangTheoDoi(ctx, "Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follow = capTheoDoi.QuanHeTheoDoi;
                return new YeuCauApi(HttpMethod.Post, "/get_list_following",
                    Obj(("user_id", IdBatBuoc(capTheoDoi.TaiKhoanTheoDoi.TaiKhoanIdServer, "follower_tk_id_server")), ("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, capTheoDoi.TaiKhoanTheoDoi));
            },
            Ok,
            DataLaMang());

        Them(ds, "FOLLOWING-LIST-03", "FollowBlock", "Lấy following của user không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/get_list_following",
                Obj(("user_id", 999999999), ("index", 0), ("count", 10)),
                await YeuCauTokenHopLeAsync(ctx)),
            KhongCoNguoiDung);

        Them(ds, "FOLLOWING-LIST-04", "FollowBlock", "Lấy following của user đang bị block",
            "Dùng cặp active trong tk_chan_seed.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan.");
                var chan = capChan.QuanHeChan;
                return new YeuCauApi(HttpMethod.Post, "/get_list_following",
                    Obj(("user_id", IdBatBuoc(chan.BlockedTaiKhoanIdServer, "blocked_tk_id_server")), ("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan));
            },
            KhongCoQuyen);
    }

    private static void ThemBlockVaUnblock(List<KichBanApi> ds)
    {
        Them(ds, "BLOCK-SET-01", "FollowBlock", "Block/unblock với token không hợp lệ",
            "Gọi /set_user_block bằng token sai định dạng.",
            ctx => Req(HttpMethod.Post, "/set_user_block", Obj(("user_id", 999999999), ("type", 0)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "BLOCK-SET-02", "FollowBlock", "Block user hợp lệ",
            "Chọn cặp user chưa có quan hệ block.",
            async ctx =>
            {
                var (taiKhoanChan, taiKhoanBiChan) = ChonCapTaiKhoanChuaChan(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", IdBatBuoc(taiKhoanBiChan.TaiKhoanIdServer, "blocked_tk_id_server")), ("type", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoanChan));
                req.Tam["taiKhoanChan"] = taiKhoanChan;
                req.Tam["taiKhoanBiChan"] = taiKhoanBiChan;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var taiKhoanChan = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoanChan"]!;
                var taiKhoanBiChan = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoanBiChan"]!;
                await ctx.CapNhatDB.ThemQuanHeChanAsync(taiKhoanChan, taiKhoanBiChan);
            });

        Them(ds, "BLOCK-SET-03", "FollowBlock", "Unblock user đang bị chặn",
            "Lấy quan hệ active trong tk_chan_seed và gọi unblock.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan.");
                var chan = capChan.QuanHeChan;
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", IdBatBuoc(chan.BlockedTaiKhoanIdServer, "blocked_tk_id_server")), ("type", 1)),
                    await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan));
                req.Tam["chan"] = chan;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var chan = (TaiKhoanChanSeed)request.Tam["chan"]!;
                await ctx.CapNhatDB.XoaQuanHeChanDangHoatDongAsync(chan);
            });

        Them(ds, "BLOCK-SET-04", "FollowBlock", "Block user không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/set_user_block",
                Obj(("user_id", 999999999), ("type", 0)),
                await YeuCauTokenHopLeAsync(ctx)),
            KhongCoNguoiDung);

        Them(ds, "BLOCK-SET-05", "FollowBlock", "Block chính mình",
            "user_id là id của current user.",
            async ctx =>
            {
                var tk = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", IdBatBuoc(tk.TaiKhoanIdServer, "tk_id_server")), ("type", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, tk));
            },
            SaiGiaTri);

        Them(ds, "BLOCK-SET-06", "FollowBlock", "Block lại user đã chặn",
            "Lấy quan hệ active trong tk_chan_seed và gọi block lại.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan.");
                var chan = capChan.QuanHeChan;
                return new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", IdBatBuoc(chan.BlockedTaiKhoanIdServer, "blocked_tk_id_server")), ("type", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan));
            },
            DaThucHienTruocDo);

        Them(ds, "BLOCK-SET-07", "FollowBlock", "Unblock user chưa bị chặn",
            "Chọn cặp user chưa có quan hệ block và gọi unblock.",
            async ctx =>
            {
                var (taiKhoanChan, taiKhoanBiChan) = ChonCapTaiKhoanChuaChan(ctx);
                return new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", IdBatBuoc(taiKhoanBiChan.TaiKhoanIdServer, "blocked_tk_id_server")), ("type", 1)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoanChan));
            },
            DaThucHienTruocDo);
    }

    private static void ThemLayDanhSachDangBlock(List<KichBanApi> ds)
    {
        Them(ds, "BLOCK-LIST-01", "FollowBlock", "Lấy danh sách block với token không hợp lệ",
            "Token sai, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/get_list_blocks", Obj(("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "BLOCK-LIST-02", "FollowBlock", "Lấy danh sách user đã block",
            "Dùng current user có quan hệ block trong tk_chan_seed.",
            async ctx =>
            {
                var capChan = LayCapTaiKhoanDangChan(ctx, "Thiếu tk_chan_seed trạng thái dang_chan.");
                return new YeuCauApi(HttpMethod.Post, "/get_list_blocks",
                    Obj(("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, capChan.TaiKhoanChan));
            },
            Ok,
            DataLaMang());
    }

}








