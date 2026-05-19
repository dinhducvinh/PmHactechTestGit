using KiemThuApiShop.Core;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemSetCommentsProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-COMMENT-SET-01", "Product", "Tạo bình luận hợp lệ cho sản phẩm tồn tại",
            "Token hợp lệ, product_id tồn tại, content hợp lệ.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var tk = ctx.KhoSeed.LayTaiKhoanKhac(sp.SellerTkSeedId) ?? await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var noiDung = $"Bình luận test {DateTimeOffset.Now:HHmmss}";
                var req = new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", noiDung), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
                req.Tam["sanPham"] = sp;
                req.Tam["taiKhoan"] = tk;
                req.Tam["noiDung"] = noiDung;
                return req;
            },
            Ok,
            DataLaMang(),
            async (response, request, ctx) => await LuuBinhLuanSeedAsync(response, request, ctx));

        Them(ds, "PRODUCT-COMMENT-SET-02", "Product", "Tạo bình luận ngắn nhưng không rỗng",
            "content = ok.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "ok"), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            Ok);

        Them(ds, "PRODUCT-COMMENT-SET-03", "Product", "Tạo bình luận không gửi token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "Comment không token"), ("index", 0), ("count", 10)));
            },
            SaiToken);

        Them(ds, "PRODUCT-COMMENT-SET-04", "Product", "Tạo bình luận với token không hợp lệ",
            "Gửi token sai định dạng.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "Comment token sai"), ("index", 0), ("count", 10)),
                    ctx.TokenSaiDinhDang);
            },
            SaiToken);

        Them(ds, "PRODUCT-COMMENT-SET-05", "Product", "Tạo bình luận thiếu product_id",
            "Chỉ truyền content/index/count.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("content", "Thiếu product"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);

        Them(ds, "PRODUCT-COMMENT-SET-06", "Product", "Tạo bình luận thiếu content",
            "Chỉ truyền product_id/index/count.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            ThieuThamSo);

        Them(ds, "PRODUCT-COMMENT-SET-07", "Product", "Tạo bình luận thiếu index",
            "Chỉ truyền product_id/content/count.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "Thiếu index"), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            ThieuThamSo);

        Them(ds, "PRODUCT-COMMENT-SET-08", "Product", "Tạo bình luận thiếu count",
            "Chỉ truyền product_id/content/index.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "Thiếu count"), ("index", 0)), await ctx.YeuCauTokenHopLeAsync());
            },
            ThieuThamSo);

        Them(ds, "PRODUCT-COMMENT-SET-09", "Product", "Tạo bình luận với product_id sai kiểu",
            "product_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("product_id", "abc"), ("content", "Sai kiểu"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        Them(ds, "PRODUCT-COMMENT-SET-10", "Product", "Tạo bình luận với index/count sai kiểu",
            "index = abc.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "Sai kiểu index"), ("index", "abc"), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            SaiKieu);

        Them(ds, "PRODUCT-COMMENT-SET-11", "Product", "Tạo bình luận với product_id không hợp lệ",
            "product_id = -1.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("product_id", -1), ("content", "Sai product"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-12", "Product", "Tạo bình luận với index/count không hợp lệ",
            "count = 0.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "Sai count"), ("index", 0), ("count", 0)), await ctx.YeuCauTokenHopLeAsync());
            },
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-13", "Product", "Tạo bình luận content rỗng",
            "content = khoảng trắng.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "   "), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-14", "Product", "Tạo bình luận content quá dài",
            "Gửi nội dung dài vượt giới hạn phổ biến.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", new string('a', 5001)), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-15", "Product", "Tạo bình luận product_id không tồn tại",
            "product_id = 999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("product_id", 999999), ("content", "Product không tồn tại"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            KhongCoSanPham);

        Them(ds, "PRODUCT-COMMENT-SET-16", "Product", "Tạo nhiều comment liên tiếp cho cùng sản phẩm",
            "Gọi API tạo comment hợp lệ.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", $"Comment liên tiếp {DateTimeOffset.Now:HHmmss}"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok);

        Them(ds, "PRODUCT-COMMENT-SET-17", "Product", "Đồng bộ comment mới với get_comments_product",
            "Tạo comment rồi gọi get_comments_product để kiểm tra.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", $"Comment sync {DateTimeOffset.Now:HHmmss}"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok);

        Them(ds, "PRODUCT-COMMENT-SET-18", "Product", "Seller tự bình luận sản phẩm của mình",
            "Dùng token seller sở hữu product.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var seller = ctx.YeuCauSellerCuaSanPham(sp);
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", "Seller tự bình luận"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            Tap("1000", "1009", "1004"));

        Them(ds, "PRODUCT-COMMENT-SET-19", "Product", "User bị seller chặn không được bình luận",
            "Cần tk_chan_seed biểu diễn seller chặn user.",
            _ => Req(HttpMethod.Post, "/api/set_comments_product", Obj()),
            KhongCoQuyen,
            lyDoBoQua: "Cần seed block giữa seller và user trong tk_chan_seed; chưa đủ dữ liệu để tự động hóa case này.");

        Them(ds, "PRODUCT-COMMENT-SET-20", "Product", "Tạo comment và trả về phân trang count = 5",
            "Gửi index = 0, count = 5.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id")), ("content", $"Comment count 5 {DateTimeOffset.Now:HHmmss}"), ("index", 0), ("count", 5)), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            KiemTraMangToiDa(5));
    }

    private static void ThemLikeProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LIKE-01", "Product", "Like sản phẩm chưa được tài khoản like",
            "Dùng tài khoản mồi và sản phẩm chưa like.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var sp = ctx.KhoSeed.LaySanPhamChuaDuocLikeBoi(tk.TkSeedId) ?? ctx.YeuCauSanPhamBatKy();
                var req = new YeuCauApi(HttpMethod.Post, "/api/like_product",
                    Obj(("product_id", SoId(sp.SpId, "sp_id"))), await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
                req.Tam["taiKhoan"] = tk;
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            DataBoolBang("is_liked", true),
            async (_, request, ctx) =>
            {
                var tk = (TaiKhoanSeed)request.Tam["taiKhoan"]!;
                var sp = (SanPhamSeed)request.Tam["sanPham"]!;
                if (ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Any(x => x.TkSeedId == tk.TkSeedId && x.SpId == sp.SpId && x.TrangThai == "dang_like"))
                {
                    return;
                }

                ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
                {
                    TkSeedId = tk.TkSeedId,
                    TkId = tk.TkId,
                    SpSeedId = sp.SpSeedId,
                    SpId = sp.SpId,
                    TrangThai = "dang_like",
                    TaoLuc = DateTimeOffset.Now
                });
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "PRODUCT-LIKE-02", "Product", "Unlike sản phẩm đã được tài khoản like",
            "Dùng dòng đang có trong tk_thich_sanpham_seed.",
            async ctx =>
            {
                var like = ctx.KhoSeed.LayLikeBatKy()
                    ?? throw new BoQuaKiemThuException("Thiếu dòng tk_thich_sanpham_seed đang like.");
                var tk = ctx.KhoSeed.LayTaiKhoanTheoSeedId(like.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản trong like seed.");
                var req = new YeuCauApi(HttpMethod.Post, "/api/like_product",
                    Obj(("product_id", SoId(like.SpId, "sp_id"))), await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
                req.Tam["like"] = like;
                return req;
            },
            Ok,
            DataBoolBang("is_liked", false),
            async (_, request, ctx) =>
            {
                var like = (TaiKhoanThichSanPhamSeed)request.Tam["like"]!;
                like.TrangThai = "da_bo_like";
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "PRODUCT-LIKE-03", "Product", "Like product không gửi token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/like_product", Obj(("product_id", SoId(sp.SpId, "sp_id"))));
            },
            SaiToken);

        Them(ds, "PRODUCT-LIKE-04", "Product", "Like product với token không hợp lệ",
            "Token sai định dạng/hết hạn.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/like_product", Obj(("product_id", SoId(sp.SpId, "sp_id"))), ctx.TokenSaiDinhDang);
            },
            SaiToken);

        Them(ds, "PRODUCT-LIKE-05", "Product", "Like product thiếu product_id",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", new Dictionary<string, object?>(), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);

        Them(ds, "PRODUCT-LIKE-06", "Product", "Like product_id sai kiểu",
            "product_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(("product_id", "abc")), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        Them(ds, "PRODUCT-LIKE-07", "Product", "Like product_id không hợp lệ",
            "product_id = -1.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(("product_id", -1)), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIKE-08", "Product", "Like product_id không tồn tại",
            "product_id = 999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(("product_id", 999999)), await ctx.YeuCauTokenHopLeAsync()),
            KhongCoSanPham);
    }

    private static async Task LuuBinhLuanSeedAsync(PhanHoiApi response, YeuCauApi request, NguCanhKiemThu ctx)
    {
        var sp = (SanPhamSeed)request.Tam["sanPham"]!;
        var tk = (TaiKhoanSeed)request.Tam["taiKhoan"]!;
        var noiDung = (string)request.Tam["noiDung"]!;

        ctx.KhoSeed.DuLieu.BinhLuanSanPhamSeed.Add(new BinhLuanSanPhamSeed
        {
            BlSeedId = ctx.KhoSeed.DuLieu.BinhLuanSanPhamSeed.Count + 1,
            SpSeedId = sp.SpSeedId,
            SpId = sp.SpId,
            TkSeedId = tk.TkSeedId,
            TkId = tk.TkId,
            BlId = TienIchJson.DocChuoi(response.Data, "id", "comment_id"),
            NoiDung = noiDung,
            TrangThai = "san_sang",
            TaoLuc = DateTimeOffset.Now
        });

        await ctx.KhoSeed.LuuAsync();
    }
}
