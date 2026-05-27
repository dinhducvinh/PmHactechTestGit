using KiemThuApiShop.Core;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanFollowBlock(List<KichBanApi> ds)
    {
        ThemSetUserFollow(ds);
        ThemGetListFollowed(ds);
        ThemGetListFollowing(ds);
        ThemSetUserBlock(ds);
        ThemGetListBlocks(ds);
    }

    private static void ThemSetUserFollow(List<KichBanApi> ds)
    {
        Them(ds, "FOLLOW-SET-01", "FollowBlock", "Follow/unfollow với token không hợp lệ",
            "Gọi /set_user_follow bằng token sai định dạng.",
            ctx => Req(HttpMethod.Post, "/set_user_follow", Obj(("followee_id", 999999999), ("action", "follow")), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "FOLLOW-SET-02", "FollowBlock", "Follow user hợp lệ",
            "Chọn cặp user chưa có quan hệ follow.",
            async ctx =>
            {
                var (follower, followee) = ChonCapChuaTheoDoi(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", SoIdBatBuoc(followee.TkId, "followee_tk_id_server")), ("action", "follow")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(follower));
                req.Tam["follower"] = follower;
                req.Tam["followee"] = followee;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var follower = (TaiKhoanSeed)request.Tam["follower"]!;
                var followee = (TaiKhoanSeed)request.Tam["followee"]!;
                ctx.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
                {
                    TkSeedId = follower.TkSeedId,
                    TkId = follower.TkId,
                    FolloweeTkSeedId = followee.TkSeedId,
                    FolloweeTkId = followee.TkId,
                    TheoDoiLuc = DateTimeOffset.Now,
                    TrangThai = "dang_theo_doi"
                });
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "FOLLOW-SET-03", "FollowBlock", "Follow lại user đã theo dõi",
            "Lấy quan hệ đang active trong tk_theodoi_seed và gọi follow lại.",
            async ctx =>
            {
                var follow = ctx.KhoSeed.LayTheoDoiDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follower = ctx.KhoSeed.LayTaiKhoanTheoSeedId(follow.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản follower trong seed.");
                return new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", SoIdBatBuoc(follow.FolloweeTkId, "followee_tk_id_server")), ("action", "follow")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(follower));
            },
            DaThucHienTruocDo);

        Them(ds, "FOLLOW-SET-04", "FollowBlock", "Follow user không tồn tại",
            "followee_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                Obj(("followee_id", 999999999), ("action", "follow")),
                await ctx.YeuCauTokenHopLeAsync()),
            KhongCoNguoiDung);

        Them(ds, "FOLLOW-SET-05", "FollowBlock", "Follow chính mình",
            "followee_id là id của current user.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", SoIdBatBuoc(tk.TkId, "tk_id_server")), ("action", "follow")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
            },
            SaiGiaTri);

        Them(ds, "FOLLOW-SET-06", "FollowBlock", "Unfollow user đang theo dõi",
            "Lấy quan hệ active trong tk_theodoi_seed và gọi unfollow.",
            async ctx =>
            {
                var follow = ctx.KhoSeed.LayTheoDoiDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follower = ctx.KhoSeed.LayTaiKhoanTheoSeedId(follow.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản follower trong seed.");
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_follow",
                    Obj(("followee_id", SoIdBatBuoc(follow.FolloweeTkId, "followee_tk_id_server")), ("action", "unfollow")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(follower));
                req.Tam["follow"] = follow;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var follow = (TaiKhoanTheoDoiSeed)request.Tam["follow"]!;
                follow.TrangThai = "bo_theo_doi";
                await ctx.KhoSeed.LuuAsync();
            });
    }

    private static void ThemGetListFollowed(List<KichBanApi> ds)
    {
        Them(ds, "FOLLOWED-LIST-01", "FollowBlock", "Lấy followers với token không hợp lệ",
            "Token sai, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/get_list_followed", Obj(("user_id", 999999999), ("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "FOLLOWED-LIST-02", "FollowBlock", "Lấy danh sách người theo dõi",
            "Dùng followee từ tk_theodoi_seed.",
            async ctx =>
            {
                var follow = ctx.KhoSeed.LayTheoDoiDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follower = ctx.KhoSeed.LayTaiKhoanTheoSeedId(follow.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản follower trong seed.");
                return new YeuCauApi(HttpMethod.Post, "/get_list_followed",
                    Obj(("user_id", SoIdBatBuoc(follow.FolloweeTkId, "followee_tk_id_server")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(follower));
            },
            Ok,
            DataLaMang());

        Them(ds, "FOLLOWED-LIST-03", "FollowBlock", "Lấy followers của user không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/get_list_followed",
                Obj(("user_id", 999999999), ("index", 0), ("count", 10)),
                await ctx.YeuCauTokenHopLeAsync()),
            KhongCoNguoiDung);

        Them(ds, "FOLLOWED-LIST-04", "FollowBlock", "Lấy followers của user đang bị block",
            "Dùng cặp active trong tk_chan_seed.",
            async ctx =>
            {
                var chan = ctx.KhoSeed.LayChanDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_chan_seed trạng thái dang_chan.");
                var blocker = ctx.KhoSeed.LayTaiKhoanTheoSeedId(chan.ChanTkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản blocker trong seed.");
                return new YeuCauApi(HttpMethod.Post, "/get_list_followed",
                    Obj(("user_id", SoIdBatBuoc(chan.BiChanTkId, "blocked_tk_id_server")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(blocker));
            },
            KhongCoQuyen);
    }

    private static void ThemGetListFollowing(List<KichBanApi> ds)
    {
        Them(ds, "FOLLOWING-LIST-01", "FollowBlock", "Lấy following với token không hợp lệ",
            "Token sai, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/get_list_following", Obj(("user_id", 999999999), ("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "FOLLOWING-LIST-02", "FollowBlock", "Lấy danh sách user đang follow",
            "Dùng follower từ tk_theodoi_seed.",
            async ctx =>
            {
                var follow = ctx.KhoSeed.LayTheoDoiDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_theodoi_seed trạng thái dang_theo_doi.");
                var follower = ctx.KhoSeed.LayTaiKhoanTheoSeedId(follow.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản follower trong seed.");
                return new YeuCauApi(HttpMethod.Post, "/get_list_following",
                    Obj(("user_id", SoIdBatBuoc(follower.TkId, "follower_tk_id_server")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(follower));
            },
            Ok,
            DataLaMang());

        Them(ds, "FOLLOWING-LIST-03", "FollowBlock", "Lấy following của user không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/get_list_following",
                Obj(("user_id", 999999999), ("index", 0), ("count", 10)),
                await ctx.YeuCauTokenHopLeAsync()),
            KhongCoNguoiDung);

        Them(ds, "FOLLOWING-LIST-04", "FollowBlock", "Lấy following của user đang bị block",
            "Dùng cặp active trong tk_chan_seed.",
            async ctx =>
            {
                var chan = ctx.KhoSeed.LayChanDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_chan_seed trạng thái dang_chan.");
                var blocker = ctx.KhoSeed.LayTaiKhoanTheoSeedId(chan.ChanTkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản blocker trong seed.");
                return new YeuCauApi(HttpMethod.Post, "/get_list_following",
                    Obj(("user_id", SoIdBatBuoc(chan.BiChanTkId, "blocked_tk_id_server")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(blocker));
            },
            KhongCoQuyen);
    }

    private static void ThemSetUserBlock(List<KichBanApi> ds)
    {
        Them(ds, "BLOCK-SET-01", "FollowBlock", "Block/unblock với token không hợp lệ",
            "Gọi /set_user_block bằng token sai định dạng.",
            ctx => Req(HttpMethod.Post, "/set_user_block", Obj(("user_id", 999999999), ("type", 0)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "BLOCK-SET-02", "FollowBlock", "Block user hợp lệ",
            "Chọn cặp user chưa có quan hệ block.",
            async ctx =>
            {
                var (blocker, blocked) = ChonCapChuaChan(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", SoIdBatBuoc(blocked.TkId, "blocked_tk_id_server")), ("type", 0)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(blocker));
                req.Tam["blocker"] = blocker;
                req.Tam["blocked"] = blocked;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var blocker = (TaiKhoanSeed)request.Tam["blocker"]!;
                var blocked = (TaiKhoanSeed)request.Tam["blocked"]!;
                ctx.KhoSeed.DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
                {
                    ChanTkSeedId = blocker.TkSeedId,
                    ChanTkId = blocker.TkId,
                    BiChanTkSeedId = blocked.TkSeedId,
                    BiChanTkId = blocked.TkId,
                    ChanLuc = DateTimeOffset.Now,
                    TrangThai = "dang_chan"
                });
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "BLOCK-SET-03", "FollowBlock", "Block lại user đã chặn",
            "Lấy quan hệ active trong tk_chan_seed và gọi block lại.",
            async ctx =>
            {
                var chan = ctx.KhoSeed.LayChanDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_chan_seed trạng thái dang_chan.");
                var blocker = ctx.KhoSeed.LayTaiKhoanTheoSeedId(chan.ChanTkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản blocker trong seed.");
                return new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", SoIdBatBuoc(chan.BiChanTkId, "blocked_tk_id_server")), ("type", 0)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(blocker));
            },
            DaThucHienTruocDo);

        Them(ds, "BLOCK-SET-04", "FollowBlock", "Block user không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/set_user_block",
                Obj(("user_id", 999999999), ("type", 0)),
                await ctx.YeuCauTokenHopLeAsync()),
            KhongCoNguoiDung);

        Them(ds, "BLOCK-SET-05", "FollowBlock", "Block chính mình",
            "user_id là id của current user.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                return new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", SoIdBatBuoc(tk.TkId, "tk_id_server")), ("type", 0)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
            },
            SaiGiaTri);

        Them(ds, "BLOCK-SET-06", "FollowBlock", "Unblock user đang bị chặn",
            "Lấy quan hệ active trong tk_chan_seed và gọi unblock.",
            async ctx =>
            {
                var chan = ctx.KhoSeed.LayChanDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_chan_seed trạng thái dang_chan.");
                var blocker = ctx.KhoSeed.LayTaiKhoanTheoSeedId(chan.ChanTkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản blocker trong seed.");
                var req = new YeuCauApi(HttpMethod.Post, "/set_user_block",
                    Obj(("user_id", SoIdBatBuoc(chan.BiChanTkId, "blocked_tk_id_server")), ("type", 1)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(blocker));
                req.Tam["chan"] = chan;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var chan = (TaiKhoanChanSeed)request.Tam["chan"]!;
                chan.TrangThai = "bo_chan";
                await ctx.KhoSeed.LuuAsync();
            });
    }

    private static void ThemGetListBlocks(List<KichBanApi> ds)
    {
        Them(ds, "BLOCK-LIST-01", "FollowBlock", "Lấy danh sách block với token không hợp lệ",
            "Token sai, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/get_list_blocks", Obj(("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "BLOCK-LIST-02", "FollowBlock", "Lấy danh sách user đã block",
            "Dùng current user có quan hệ block trong tk_chan_seed.",
            async ctx =>
            {
                var chan = ctx.KhoSeed.LayChanDangHoatDong()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_chan_seed trạng thái dang_chan.");
                var blocker = ctx.KhoSeed.LayTaiKhoanTheoSeedId(chan.ChanTkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản blocker trong seed.");
                return new YeuCauApi(HttpMethod.Post, "/get_list_blocks",
                    Obj(("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(blocker));
            },
            Ok,
            DataLaMang());
    }

    private static (TaiKhoanSeed Follower, TaiKhoanSeed Followee) ChonCapChuaTheoDoi(NguCanhKiemThu ctx)
    {
        var taiKhoan = ctx.KhoSeed.DuLieu.TaiKhoanSeed
            .Where(x => x.TrangThai == "san_sang" && x.TrangThaiDangKy == "da_dang_ky" && !string.IsNullOrWhiteSpace(x.TkId))
            .ToList();
        var capDaCo = ctx.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Select(x => (x.TkSeedId, x.FolloweeTkSeedId))
            .ToHashSet();
        var capChan = ctx.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .Select(x => (x.ChanTkSeedId, x.BiChanTkSeedId))
            .ToHashSet();

        foreach (var follower in taiKhoan)
        {
            foreach (var followee in taiKhoan)
            {
                if (follower.TkSeedId == followee.TkSeedId ||
                    capDaCo.Contains((follower.TkSeedId, followee.TkSeedId)) ||
                    capChan.Contains((follower.TkSeedId, followee.TkSeedId)) ||
                    capChan.Contains((followee.TkSeedId, follower.TkSeedId)))
                {
                    continue;
                }

                return (follower, followee);
            }
        }

        throw new BoQuaKiemThuException("Không tìm được cặp tài khoản chưa follow nhau.");
    }

    private static (TaiKhoanSeed Blocker, TaiKhoanSeed Blocked) ChonCapChuaChan(NguCanhKiemThu ctx)
    {
        var taiKhoan = ctx.KhoSeed.DuLieu.TaiKhoanSeed
            .Where(x => x.TrangThai == "san_sang" && x.TrangThaiDangKy == "da_dang_ky" && !string.IsNullOrWhiteSpace(x.TkId))
            .ToList();
        var capDaCo = ctx.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Select(x => (x.ChanTkSeedId, x.BiChanTkSeedId))
            .ToHashSet();
        var capTheoDoi = ctx.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .Select(x => (x.TkSeedId, x.FolloweeTkSeedId))
            .ToHashSet();

        foreach (var blocker in taiKhoan)
        {
            foreach (var blocked in taiKhoan)
            {
                if (blocker.TkSeedId == blocked.TkSeedId ||
                    capDaCo.Contains((blocker.TkSeedId, blocked.TkSeedId)) ||
                    capTheoDoi.Contains((blocker.TkSeedId, blocked.TkSeedId)) ||
                    capTheoDoi.Contains((blocked.TkSeedId, blocker.TkSeedId)))
                {
                    continue;
                }

                return (blocker, blocked);
            }
        }

        throw new BoQuaKiemThuException("Không tìm được cặp tài khoản chưa block nhau.");
    }
}
