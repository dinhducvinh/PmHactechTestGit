using KiemThuApiShop.Core;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanProduct(List<KichBanApi> ds)
    {
        ThemGetCategories(ds);
        ThemGetListBrands(ds);
        ThemGetProducts(ds);
        ThemGetListProducts(ds);
        ThemGetCommentsProduct(ds);
        ThemSetCommentsProduct(ds);
        ThemLikeProduct(ds);
        ThemAddProduct(ds);
        ThemUpdateProduct(ds);
        ThemDeleteProduct(ds);
        ThemGetUserListings(ds);
    }

    private static void ThemGetCategories(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-CATEGORY-01", "Product", "Lấy danh sách category với body rỗng",
            "Gọi /api/get_categories không truyền parent_id.",
            _ => Req(HttpMethod.Post, "/api/get_categories", new Dictionary<string, object?>()),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-CATEGORY-02", "Product", "Lấy category con theo parent_id hợp lệ",
            "Dùng parent_id có category con từ danhmuc_seed.",
            ctx =>
            {
                var dm = ctx.KhoSeed.LayDanhMucCoCon()
                    ?? throw new BoQuaKiemThuException("Thiếu danh mục có con trong danhmuc_seed.");
                return Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", SoId(dm.DmId, "dm_id"))));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-CATEGORY-03", "Product", "Lấy category con với parent_id không có con",
            "Dùng parent_id hợp lệ nhưng không có category con.",
            ctx =>
            {
                var dm = ctx.KhoSeed.LayDanhMucKhongCoCon()
                    ?? throw new BoQuaKiemThuException("Thiếu danh mục không có con trong danhmuc_seed.");
                return Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", SoId(dm.DmId, "dm_id"))));
            },
            KhongCoDuLieu);

        Them(ds, "PRODUCT-CATEGORY-04", "Product", "Lấy category với parent_id không tồn tại",
            "Gửi parent_id = 999999.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", 999999))),
            KhongCoDuLieu);

        Them(ds, "PRODUCT-CATEGORY-05", "Product", "Lấy category với parent_id sai kiểu",
            "Gửi parent_id = abc.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", "abc"))),
            SaiKieu);

        Them(ds, "PRODUCT-CATEGORY-06", "Product", "Lấy category với parent_id không hợp lệ",
            "Gửi parent_id = -1.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", -1))),
            SaiGiaTri);

        Them(ds, "PRODUCT-CATEGORY-07", "Product", "Kiểm tra danh sách category được sắp xếp theo sort",
            "Gọi danh sách category và kiểm tra thứ tự sort không giảm nếu response có sort.",
            _ => Req(HttpMethod.Post, "/api/get_categories", new Dictionary<string, object?>()),
            Ok,
            KiemTraMangSapXepTheoSort());
    }

    private static void ThemGetListBrands(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-BRAND-01", "Product", "Lấy brand với body rỗng",
            "Không truyền category_id/index/count.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", new Dictionary<string, object?>()),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-BRAND-02", "Product", "Lấy tất cả brand có phân trang",
            "category_id = 0, index = 0, count = 10.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 0), ("count", 10))),
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-BRAND-03", "Product", "Lấy brand theo category hợp lệ",
            "Dùng category có brand trong danhmuc_seed.",
            ctx =>
            {
                var dm = ctx.KhoSeed.LayDanhMucCoThuongHieu()
                    ?? throw new BoQuaKiemThuException("Thiếu category có brand trong danhmuc_seed.");
                return Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", SoId(dm.DmId, "dm_id")), ("index", 0), ("count", 10)));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-BRAND-04", "Product", "Lấy brand theo category hợp lệ nhưng không có brand",
            "Dùng category không có brand trong danhmuc_seed.",
            ctx =>
            {
                var dm = ctx.KhoSeed.LayDanhMucKhongCoThuongHieu()
                    ?? throw new BoQuaKiemThuException("Thiếu category không có brand trong danhmuc_seed.");
                return Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", SoId(dm.DmId, "dm_id")), ("index", 0), ("count", 10)));
            },
            KhongCoDuLieu);

        Them(ds, "PRODUCT-BRAND-05", "Product", "Lấy brand với category_id không tồn tại",
            "Gửi category_id = 999999.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 999999), ("index", 0), ("count", 10))),
            KhongCoDuLieu);

        Them(ds, "PRODUCT-BRAND-06", "Product", "Lấy brand với category_id sai kiểu",
            "Gửi category_id = abc.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", "abc"), ("index", 0), ("count", 10))),
            SaiKieu);

        Them(ds, "PRODUCT-BRAND-07", "Product", "Lấy brand với index/count sai kiểu",
            "Gửi index = abc.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", "abc"), ("count", 10))),
            SaiKieu);

        Them(ds, "PRODUCT-BRAND-08", "Product", "Lấy brand với index/count không hợp lệ",
            "Gửi count = 0.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 0), ("count", 0))),
            SaiGiaTri);

        Them(ds, "PRODUCT-BRAND-09", "Product", "Lấy trang đầu brand count = 5",
            "index = 0, count = 5.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 0), ("count", 5))),
            Ok,
            KiemTraMangToiDa(5));

        Them(ds, "PRODUCT-BRAND-10", "Product", "Lấy trang tiếp theo brand count = 5",
            "index = 5, count = 5.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 5), ("count", 5))),
            Ok,
            KiemTraMangToiDa(5));
    }

    private static void ThemGetProducts(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-DETAIL-01", "Product", "Lấy chi tiết sản phẩm đang tồn tại",
            "Token hợp lệ, id là product trong sanpham_seed.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", SoId(sp.SpId, "sp_id"))), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            DataCoTruong("id", "name"));

        Them(ds, "PRODUCT-DETAIL-02", "Product", "Lấy chi tiết product nhiều ảnh/nhiều variant",
            "Dùng product loại nhieu_variant.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamTheoLoai("nhieu_variant");
                return new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", SoId(sp.SpId, "sp_id"))), await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            DataCoTruong("id", "size"));

        Them(ds, "PRODUCT-DETAIL-03", "Product", "Chi tiết product đã được tài khoản like",
            "Dùng tk_thich_sanpham_seed.",
            async ctx =>
            {
                var like = ctx.KhoSeed.LayLikeBatKy()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_thich_sanpham_seed.");
                var tk = ctx.KhoSeed.LayTaiKhoanTheoSeedId(like.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản đã like.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", SoId(like.SpId, "sp_id"))), await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
            },
            Ok,
            DataBoolBang("is_liked", true));

        Them(ds, "PRODUCT-DETAIL-04", "Product", "Chi tiết product chưa được tài khoản like",
            "Dùng tài khoản trong tk_thich_sanpham_seed và product khác chưa like.",
            async ctx =>
            {
                var like = ctx.KhoSeed.LayLikeBatKy()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_thich_sanpham_seed.");
                var sp = ctx.KhoSeed.LaySanPhamChuaDuocLikeBoi(like.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu product chưa được tài khoản này like.");
                var tk = ctx.KhoSeed.LayTaiKhoanTheoSeedId(like.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản like.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", SoId(sp.SpId, "sp_id"))), await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
            },
            Ok,
            DataBoolBang("is_liked", false));

        Them(ds, "PRODUCT-DETAIL-05", "Product", "Seller xem product của mình có can_edit = true",
            "Dùng token seller sở hữu product.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var seller = ctx.YeuCauSellerCuaSanPham(sp);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", SoId(sp.SpId, "sp_id"))), await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            Ok,
            DataBoolBang("can_edit", true));

        Them(ds, "PRODUCT-DETAIL-06", "Product", "User khác seller xem product có can_edit = false",
            "Dùng token user không sở hữu product.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var userKhac = ctx.YeuCauTaiKhoanKhacSeller(sp);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", SoId(sp.SpId, "sp_id"))), await ctx.YeuCauTokenCuaTaiKhoanAsync(userKhac));
            },
            Ok,
            DataBoolBang("can_edit", false));

        Them(ds, "PRODUCT-DETAIL-07", "Product", "Lấy chi tiết product_id không tồn tại",
            "Gửi id = 999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", 999999)), await ctx.YeuCauTokenHopLeAsync()),
            KhongCoSanPham);

        Them(ds, "PRODUCT-DETAIL-08", "Product", "Lấy chi tiết thiếu id",
            "Gọi API với body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", new Dictionary<string, object?>(), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);

        Them(ds, "PRODUCT-DETAIL-09", "Product", "Lấy chi tiết id sai kiểu",
            "Gửi id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", "abc")), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        Them(ds, "PRODUCT-DETAIL-10", "Product", "Lấy chi tiết id không hợp lệ",
            "Gửi id = -1.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", -1)), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-DETAIL-11", "Product", "Lấy chi tiết không gửi token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Post, "/api/get_products", Obj(("id", SoId(sp.SpId, "sp_id"))));
            },
            SaiToken);
    }
}
