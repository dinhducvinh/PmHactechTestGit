using KiemThuApiShop.Core;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemGetListProducts(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LIST-01", "Product", "Lấy danh sách sản phẩm phân trang cơ bản",
            "index = 0, count = 10, không cần token.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", 0), ("count", 10))),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-02", "Product", "Lấy danh sách thiếu index",
            "Chỉ truyền count.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("count", 10))),
            ThieuThamSo);

        Them(ds, "PRODUCT-LIST-03", "Product", "Lấy danh sách thiếu count",
            "Chỉ truyền index.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", 0))),
            ThieuThamSo);

        Them(ds, "PRODUCT-LIST-04", "Product", "Lấy danh sách với index/count sai kiểu",
            "Gửi index = abc.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", "abc"), ("count", 10))),
            SaiKieu);

        Them(ds, "PRODUCT-LIST-05", "Product", "Lấy danh sách với index/count không hợp lệ",
            "Gửi count = 0.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", 0), ("count", 0))),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIST-06", "Product", "Lọc danh sách theo category_id hợp lệ",
            "Dùng category từ sanpham_seed.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/get_list_products", Obj(("category_id", SoId(sp.DmId, "dm_id")), ("index", 0), ("count", 10)));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-07", "Product", "Lọc danh sách theo category_id không tồn tại",
            "Gửi category_id = 999999.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("category_id", 999999), ("index", 0), ("count", 10))),
            KhongCoDuLieu);

        Them(ds, "PRODUCT-LIST-08", "Product", "Lọc danh sách theo brand_id hợp lệ",
            "Dùng brand từ sanpham_seed hoặc thuonghieu_seed.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var brand = sp.ThuongHieuId ?? ctx.LayThuongHieuNeuCo()?.ThuongHieuId
                    ?? throw new BoQuaKiemThuException("Thiếu brand hợp lệ trong seed.");
                return Req(HttpMethod.Post, "/api/get_list_products", Obj(("brand_id", SoId(brand, "thuonghieu_id")), ("index", 0), ("count", 10)));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-09", "Product", "Tìm danh sách theo keyword có kết quả",
            "Dùng một phần tên sản phẩm mồi.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var keyword = sp.TenSp.Length > 12 ? sp.TenSp[..12] : sp.TenSp;
                return Req(HttpMethod.Post, "/api/get_list_products", Obj(("keyword", keyword), ("index", 0), ("count", 10)));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-10", "Product", "Tìm danh sách theo keyword không có kết quả",
            "Gửi keyword ngẫu nhiên.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("keyword", "keyword_khong_ton_tai_987654"), ("index", 0), ("count", 10))),
            KhongCoDuLieu);

        Them(ds, "PRODUCT-LIST-11", "Product", "Lọc danh sách theo khoảng giá hợp lệ",
            "Gửi price_min/price_max hợp lệ.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("price_min", 1000), ("price_max", 100000000), ("index", 0), ("count", 10))),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-12", "Product", "Lọc danh sách với price_min lớn hơn price_max",
            "Gửi khoảng giá sai.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("price_min", 1000000), ("price_max", 1000), ("index", 0), ("count", 10))),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIST-13", "Product", "Sắp xếp danh sách theo price_asc",
            "order = price_asc.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("order", "price_asc"), ("index", 0), ("count", 10))),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-14", "Product", "Sắp xếp danh sách theo price_desc",
            "order = price_desc.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("order", "price_desc"), ("index", 0), ("count", 10))),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-15", "Product", "Sắp xếp danh sách theo created_desc",
            "order = created_desc.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("order", "created_desc"), ("index", 0), ("count", 10))),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LIST-16", "Product", "Sắp xếp danh sách với order không hợp lệ",
            "order = invalid_order.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("order", "invalid_order"), ("index", 0), ("count", 10))),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIST-17", "Product", "Lấy trang đầu sản phẩm count = 5",
            "index = 0, count = 5.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", 0), ("count", 5))),
            Ok,
            KiemTraMangToiDa(5));

        Them(ds, "PRODUCT-LIST-18", "Product", "Lấy trang tiếp theo sản phẩm count = 5",
            "index = 5, count = 5.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", 5), ("count", 5))),
            Ok,
            KiemTraMangToiDa(5));

        Them(ds, "PRODUCT-LIST-19", "Product", "Lọc sản phẩm theo product_size_id hợp lệ",
            "Dùng size/variant nếu hệ thống có dữ liệu.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("product_size_id", 1), ("index", 0), ("count", 10))),
            Ok,
            DataLaMang("id"));
    }

    private static void ThemGetCommentsProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-COMMENT-GET-01", "Product", "Lấy bình luận của sản phẩm đã có comment",
            "Dùng binhluan_sp_seed.",
            async ctx =>
            {
                var bl = ctx.KhoSeed.LayBinhLuanBatKy()
                    ?? throw new BoQuaKiemThuException("Thiếu binhluan_sp_seed.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(bl.SpId, "sp_id")), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-COMMENT-GET-02", "Product", "Lấy bình luận sản phẩm chưa có comment",
            "Dùng sản phẩm không có dòng trong binhluan_sp_seed.",
            async ctx =>
            {
                var sp = ctx.KhoSeed.LaySanPhamChuaCoBinhLuan()
                    ?? throw new BoQuaKiemThuException("Thiếu product chưa có bình luận.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(sp.SpId, "sp_id")), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            KhongCoDuLieu);

        Them(ds, "PRODUCT-COMMENT-GET-03", "Product", "Lấy bình luận không gửi token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(sp.SpId, "sp_id")), ("index", 0), ("count", 10)));
            },
            SaiToken);

        Them(ds, "PRODUCT-COMMENT-GET-04", "Product", "Lấy bình luận với token không hợp lệ",
            "Gửi token sai.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(sp.SpId, "sp_id")), ("index", 0), ("count", 10)), ctx.TokenSaiDinhDang);
            },
            SaiToken);

        Them(ds, "PRODUCT-COMMENT-GET-05", "Product", "Lấy bình luận thiếu product_id",
            "Chỉ truyền index/count.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);

        Them(ds, "PRODUCT-COMMENT-GET-06", "Product", "Lấy bình luận thiếu index",
            "Chỉ truyền product_id/count.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(sp.SpId, "sp_id")), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            ThieuThamSo);

        Them(ds, "PRODUCT-COMMENT-GET-07", "Product", "Lấy bình luận thiếu count",
            "Chỉ truyền product_id/index.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(sp.SpId, "sp_id")), ("index", 0)), await ctx.YeuCauTokenHopLeAsync());
            },
            ThieuThamSo);

        Them(ds, "PRODUCT-COMMENT-GET-08", "Product", "Lấy bình luận với product_id sai kiểu",
            "product_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", "abc"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        Them(ds, "PRODUCT-COMMENT-GET-09", "Product", "Lấy bình luận với index/count sai kiểu",
            "index = abc.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(sp.SpId, "sp_id")), ("index", "abc"), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            SaiKieu);

        Them(ds, "PRODUCT-COMMENT-GET-10", "Product", "Lấy bình luận với product_id không hợp lệ",
            "product_id = -1.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", -1), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-GET-11", "Product", "Lấy bình luận với index/count không hợp lệ",
            "count = 0.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(sp.SpId, "sp_id")), ("index", 0), ("count", 0)), await ctx.YeuCauTokenHopLeAsync());
            },
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-GET-12", "Product", "Lấy bình luận product_id không tồn tại",
            "product_id = 999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", 999999), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            KhongCoSanPham);

        Them(ds, "PRODUCT-COMMENT-GET-13", "Product", "Lấy trang đầu bình luận count = 5",
            "Sản phẩm có nhiều bình luận.",
            async ctx =>
            {
                var bl = ctx.KhoSeed.LayBinhLuanBatKy()
                    ?? throw new BoQuaKiemThuException("Thiếu bình luận seed.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(bl.SpId, "sp_id")), ("index", 0), ("count", 5)), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            KiemTraMangToiDa(5));

        Them(ds, "PRODUCT-COMMENT-GET-14", "Product", "Lấy trang tiếp theo bình luận count = 5",
            "index = 5, count = 5.",
            async ctx =>
            {
                var bl = ctx.KhoSeed.LayBinhLuanBatKy()
                    ?? throw new BoQuaKiemThuException("Thiếu bình luận seed.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(bl.SpId, "sp_id")), ("index", 5), ("count", 5)), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            KiemTraMangToiDa(5));

        Them(ds, "PRODUCT-COMMENT-GET-15", "Product", "Lấy bình luận với index vượt tổng số comment",
            "Gửi index rất lớn.",
            async ctx =>
            {
                var bl = ctx.KhoSeed.LayBinhLuanBatKy()
                    ?? throw new BoQuaKiemThuException("Thiếu bình luận seed.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", SoId(bl.SpId, "sp_id")), ("index", 9999), ("count", 10)), await ctx.YeuCauTokenHopLeAsync());
            },
            KhongCoDuLieu);
    }
}
