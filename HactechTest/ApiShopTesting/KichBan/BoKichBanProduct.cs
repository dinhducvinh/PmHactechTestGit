using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> ProductOkHoacHetDuLieu = Tap("1000", "9994");
    private static readonly IReadOnlySet<string> ProductOkHoacKhongTonTai = Tap("1000", "9992");
    private static readonly IReadOnlySet<string> ProductKhongTonTai = Tap("9992");
    private static readonly IReadOnlySet<string> ProductKhongCoQuyen = Tap("1009");
    private static readonly IReadOnlySet<string> ProductSaiIdDuongDan = Tap("1002", "1003", "1004", "9992");
    private static readonly IReadOnlySet<string> ProductSaiTokenHoacNguoiDung = Tap("9998", "9995", "HTTP_401", "HTTP_403");

    private static void ThemKichBanProduct(List<KichBanApi> ds)
    {
        ThemGetCategories(ds);
        ThemGetListBrands(ds);
        ThemGetProducts(ds);
        ThemGetListProducts(ds);
        ThemGetCommentsProduct(ds);
        ThemSetCommentsProduct(ds);
        ThemLikeProduct(ds);
        ThemReportProduct(ds);
        ThemAddProduct(ds);
        ThemUpdateProduct(ds);
        ThemGetUserListings(ds);
        ThemDeleteProduct(ds);
    }

    private static void ThemGetCategories(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-CAT-01", "Product", "Lấy tất cả danh mục",
            "POST /api/get_categories với body rỗng.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj()),
            ProductOkHoacHetDuLieu,
            DataLaMang("id"));

        Them(ds, "PRODUCT-CAT-02", "Product", "Lấy danh mục theo parent_id hợp lệ",
            "Dùng parent_id lấy từ danhmuc_seed.",
            ctx =>
            {
                var dm = LayDanhMucCoCon(ctx);
                return Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", IdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server"))));
            },
            ProductOkHoacHetDuLieu,
            DataLaMang());

        Them(ds, "PRODUCT-CAT-03", "Product", "Lấy danh mục parent_id không có con",
            "parent_id lấy từ danh mục tồn tại nhưng không có category con.",
            ctx =>
            {
                var dm = LayDanhMucKhongCoCon(ctx);
                return Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", IdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server"))));
            },
            ProductOkHoacHetDuLieu,
            DataLaMang());

        Them(ds, "PRODUCT-CAT-04", "Product", "Lấy danh mục parent_id không tồn tại",
            "parent_id = 999999999.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", 999999999))),
            ProductOkHoacHetDuLieu,
            DataLaMang());

        Them(ds, "PRODUCT-CAT-05", "Product", "Lấy danh mục parent_id sai kiểu",
            "parent_id = abc.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", "abc"))),
            SaiKieuHoacSaiGiaTri);

        Them(ds, "PRODUCT-CAT-06", "Product", "Lấy danh mục parent_id không hợp lệ",
            "parent_id = -1.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", -1))),
            Tap("1004", "9994"));

        Them(ds, "PRODUCT-CAT-07", "Product", "Lấy danh mục kiểm tra sắp xếp",
            "Gọi danh sách category và chỉ kiểm tra response là mảng khi có data.",
            _ => Req(HttpMethod.Post, "/api/get_categories", Obj()),
            ProductOkHoacHetDuLieu,
            DataLaMang());
    }

    private static void ThemGetListBrands(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-BRAND-01", "Product", "Lấy tất cả thương hiệu",
            "POST /api/get_list_brands với body rỗng.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj()),
            ProductOkHoacHetDuLieu,
            DataLaMang());

        Them(ds, "PRODUCT-BRAND-02", "Product", "Lấy thương hiệu có phân trang",
            "category_id = 0, index = 0, count = 10.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 0), ("count", 10))),
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-BRAND-03", "Product", "Lấy thương hiệu theo category hợp lệ",
            "category_id lấy từ danhmuc_seed.",
            ctx =>
            {
                var dm = LayDanhMucCoThuongHieu(ctx);
                return Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", IdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server")), ("index", 0), ("count", 10)));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-BRAND-04", "Product", "Lấy thương hiệu category không có brand",
            "category_id tồn tại nhưng không có thương hiệu liên kết.",
            ctx =>
            {
                var dm = LayDanhMucKhongCoThuongHieu(ctx);
                return Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", IdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server")), ("index", 0), ("count", 10)));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-BRAND-05", "Product", "Lấy thương hiệu category không tồn tại",
            "category_id = 999999999.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 999999999), ("index", 0), ("count", 10))),
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-BRAND-06", "Product", "Lấy thương hiệu category sai kiểu",
            "category_id = abc.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", "abc"), ("index", 0), ("count", 10))),
            SaiKieuHoacSaiGiaTri);

        Them(ds, "PRODUCT-BRAND-07", "Product", "Lấy thương hiệu index/count sai kiểu",
            "index = abc, count = xyz.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", "abc"), ("count", "xyz"))),
            SaiKieuHoacSaiGiaTri);

        Them(ds, "PRODUCT-BRAND-08", "Product", "Lấy thương hiệu count không hợp lệ",
            "count = 0.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 0), ("count", 0))),
            SaiGiaTri);

        Them(ds, "PRODUCT-BRAND-09", "Product", "Lấy thương hiệu trang đầu",
            "index = 0, count = 5.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 0), ("count", 5))),
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(5));

        Them(ds, "PRODUCT-BRAND-10", "Product", "Lấy thương hiệu trang tiếp theo",
            "index = 5, count = 5.",
            _ => Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", 0), ("index", 5), ("count", 5))),
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(5));
    }

    private static void ThemGetProducts(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-DETAIL-01", "Product", "Lấy chi tiết sản phẩm hợp lệ",
            "Không kèm Authorization; id lấy từ sanpham_seed.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/get_products", Obj(("id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))));
            },
            Ok,
            KiemTraChiTietPublic());

        Them(ds, "PRODUCT-DETAIL-02", "Product", "Seller xem sản phẩm của mình",
            "Gửi token của seller sở hữu product, kỳ vọng server trả data.can_edit = true.",
            async ctx =>
            {
                var (sp, seller) = LaySanPhamKemSeller(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products",
                    Obj(("id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, seller));
            },
            Ok,
            DataBoolBang("can_edit", true));

        Them(ds, "PRODUCT-DETAIL-03", "Product", "User khác xem sản phẩm",
            "Dùng token của user khác seller, kỳ vọng can_edit = false nếu server trả trường này.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanXemSanPhamKhongBiChan(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products",
                    Obj(("id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            Ok,
            KiemTraCanEditNeuCo(false));

        Them(ds, "PRODUCT-DETAIL-04", "Product", "Lấy sản phẩm không tồn tại",
            "id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", 999999999)), await YeuCauTokenHopLeAsync(ctx)),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-DETAIL-05", "Product", "Lấy sản phẩm thiếu id",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-DETAIL-06", "Product", "Lấy sản phẩm khi có quan hệ block",
            "Current user và seller có quan hệ block theo tk_chan_seed, kỳ vọng server từ chối truy cập.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanBiChanVoiSanPham(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products",
                    Obj(("id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            ProductKhongCoQuyen);
    }

    private static void ThemGetListProducts(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LIST-01", "Product", "Lấy danh sách sản phẩm",
            "Không cần token, truyền index/count.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", 0), ("count", 10))),
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-LIST-02", "Product", "Lọc sản phẩm theo seed",
            "Dùng category, brand, keyword, khoảng giá từ sanpham_seed.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var body = Obj(
                    ("category_id", IdBatBuoc(sp.DanhMucIdServer, "sanpham_seed.dm_id_server")),
                    ("keyword", sp.TenSanPham),
                    ("price_min", 0),
                    ("price_max", Math.Max(sp.Gia + 10000m, 1m)),
                    ("order", "created_desc"),
                    ("index", 0),
                    ("count", 10));
                if (sp.ThuongHieuIdServer is > 0)
                {
                    body["brand_id"] = IdBatBuoc(sp.ThuongHieuIdServer, "sanpham_seed.thuonghieu_id_server");
                }

                return Req(HttpMethod.Post, "/api/get_list_products", body);
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-LIST-03", "Product", "Lọc sản phẩm không có dữ liệu",
            "keyword không khớp sản phẩm nào.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("keyword", $"khong_co_san_pham_{Guid.NewGuid():N}"), ("index", 0), ("count", 10))),
            Tap("9994"));

        Them(ds, "PRODUCT-LIST-04", "Product", "Lấy danh sách thiếu index/count",
            "Body rỗng.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj()),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIST-05", "Product", "Lấy danh sách index/count sai kiểu",
            "index = abc, count = xyz.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("index", "abc"), ("count", "xyz"))),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIST-06", "Product", "Lấy danh sách filter mâu thuẫn",
            "price_min > price_max và order lạ.",
            _ => Req(HttpMethod.Post, "/api/get_list_products", Obj(("price_min", 10000000), ("price_max", 1000), ("order", "invalid_order"), ("index", 0), ("count", 10))),
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));
    }

    private static void ThemGetCommentsProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-COMMENT-LIST-01", "Product", "Lấy bình luận sản phẩm",
            "product_id lấy từ sanpham_seed.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/get_comments_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("index", 0), ("count", 10)));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-COMMENT-LIST-02", "Product", "Lấy bình luận thiếu tham số",
            "Body rỗng.",
            _ => Req(HttpMethod.Post, "/api/get_comments_product", Obj()),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-LIST-03", "Product", "Lấy bình luận sản phẩm không tồn tại",
            "product_id = 999999999.",
            _ => Req(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", 999999999), ("index", 0), ("count", 10))),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-COMMENT-LIST-04", "Product", "Lấy bình luận product_id sai kiểu",
            "product_id = abc.",
            _ => Req(HttpMethod.Post, "/api/get_comments_product", Obj(("product_id", "abc"), ("index", 0), ("count", 10))),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-LIST-05", "Product", "Lấy bình luận hết trang dữ liệu",
            "product_id tồn tại, index rất lớn.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/get_comments_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("index", 999999), ("count", 10)));
            },
            Tap("9994"));
    }

    private static void ThemSetCommentsProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-COMMENT-SET-01", "Product", "Tạo bình luận sản phẩm",
            "Token hợp lệ, product tồn tại, user không block seller.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanXemSanPhamKhongBiChan(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("content", $"Binh luan test {DateTimeOffset.Now:HHmmss}"), ("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            Ok,
            DataLaMang());

        Them(ds, "PRODUCT-COMMENT-SET-02", "Product", "Tạo bình luận không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("content", "No token"), ("index", 0), ("count", 10)));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-COMMENT-SET-03", "Product", "Tạo bình luận sản phẩm không tồn tại",
            "product_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("product_id", 999999999), ("content", "Product không tồn tại"), ("index", 0), ("count", 10)),
                await YeuCauTokenHopLeAsync(ctx)),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-COMMENT-SET-04", "Product", "Tạo bình luận thiếu tham số",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-05", "Product", "Tạo bình luận sai kiểu",
            "product_id = abc, index/count sai kiểu.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("product_id", "abc"), ("content", "Sai kiểu"), ("index", "abc"), ("count", "xyz")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-06", "Product", "Tạo bình luận khi có quan hệ block",
            "Current user và seller có quan hệ block theo tk_chan_seed.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanBiChanVoiSanPham(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("content", $"Block comment {DateTimeOffset.Now:HHmmss}"), ("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            KhongCoQuyen);
    }

    private static void ThemLikeProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LIKE-01", "Product", "Like sản phẩm mới",
            "Chọn cặp user/product chưa có trong tk_thich_sanpham_seed.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanSanPhamChuaLike(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/api/like_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
                req.Tam["taiKhoan"] = user;
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            KiemTraLike(true),
            async (_, request, ctx) =>
            {
                var taiKhoan = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoan"]!;
                var sanPham = (SanPhamSeed)request.Tam["sanPham"]!;
                await ctx.CapNhatDB.ThemLikeSanPhamAsync(taiKhoan, sanPham);
            });

        Them(ds, "PRODUCT-LIKE-02", "Product", "Unlike sản phẩm đã like",
            "Dùng một dòng đang có trong tk_thich_sanpham_seed.",
            async ctx =>
            {
                var cap = LayLikeSanPhamDangLuu(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/api/like_product",
                    Obj(("product_id", IdBatBuoc(cap.Like.SanPhamIdServer, "tk_thich_sanpham_seed.sp_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
                req.Tam["like"] = cap.Like;
                return req;
            },
            ProductOkHoacKhongTonTai,
            KiemTraLike(false),
            async (response, request, ctx) =>
            {
                if (!LaMaThanhCong(response))
                {
                    return;
                }

                var like = (TaiKhoanThichSanPhamSeed)request.Tam["like"]!;
                await ctx.CapNhatDB.XoaLikeSanPhamAsync(like);
            });

        Them(ds, "PRODUCT-LIKE-03", "Product", "Like sản phẩm không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/like_product", Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LIKE-04", "Product", "Like sản phẩm token sai",
            "Authorization sai định dạng.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/like_product", Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))), ctx.TokenSaiDinhDang);
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LIKE-05", "Product", "Like sản phẩm thiếu product_id",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIKE-06", "Product", "Like sản phẩm product_id sai kiểu",
            "product_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(("product_id", "abc")), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIKE-07", "Product", "Like sản phẩm không tồn tại",
            "product_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(("product_id", 999999999)), await YeuCauTokenHopLeAsync(ctx)),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-LIKE-08", "Product", "Like sản phẩm khi có quan hệ block",
            "Current user và seller có quan hệ block theo tk_chan_seed.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanBiChanVoiSanPham(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/like_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            KhongCoQuyen);
    }

    private static void ThemReportProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-REPORT-01", "Product", "Báo cáo sản phẩm hợp lệ",
            "Token hợp lệ, product tồn tại, user không block seller và cặp user/product chưa có trong report_seed.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanSanPhamChuaReport(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/api/report_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("subject", "Nội dung cần kiểm tra"), ("details", "Báo cáo từ testcase tự động.")),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
                req.Tam["taiKhoan"] = user;
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var taiKhoan = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoan"]!;
                var sanPham = (SanPhamSeed)request.Tam["sanPham"]!;
                await ctx.CapNhatDB.ThemReportSanPhamAsync(taiKhoan, sanPham);
            });

        Them(ds, "PRODUCT-REPORT-02", "Product", "Báo cáo sản phẩm không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/report_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("subject", "No token"), ("details", "No token")));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-REPORT-03", "Product", "Báo cáo sản phẩm không tồn tại",
            "product_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/report_product",
                Obj(("product_id", 999999999), ("subject", "Không tồn tại"), ("details", "Không tồn tại")),
                await YeuCauTokenHopLeAsync(ctx)),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-REPORT-04", "Product", "Báo cáo sản phẩm thiếu tham số",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/report_product", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-REPORT-05", "Product", "Báo cáo sản phẩm sai kiểu",
            "product_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/report_product",
                Obj(("product_id", "abc"), ("subject", "Sai kiểu"), ("details", "Sai kiểu")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-REPORT-06", "Product", "Báo cáo sản phẩm khi có quan hệ block",
            "Current user và seller có quan hệ block theo tk_chan_seed.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanBiChanVoiSanPham(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/report_product",
                    Obj(("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("subject", "Blocked report"), ("details", "Báo cáo khi có quan hệ block.")),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            KhongCoQuyen);

        Them(ds, "PRODUCT-REPORT-07", "Product", "Báo cáo lại sản phẩm đã report",
            "Lấy cặp user/product trong report_seed để kiểm tra mã 1010.",
            async ctx =>
            {
                var cap = LayReportSanPhamDangLuu(ctx);
                var user = cap.TaiKhoan;
                var sp = cap.SanPham;
                var token = await LayTokenCuaTaiKhoanAsync(ctx, user);
                var body = Obj(
                    ("product_id", IdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")),
                    ("subject", "Duplicate report"),
                    ("details", "Báo cáo lặp lại từ testcase tự động."));
                return new YeuCauApi(HttpMethod.Post, "/api/report_product", body, token);
            },
            DaThucHienTruocDo);
    }

    private static void ThemAddProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-ADD-01", "Product", "Thêm sản phẩm hợp lệ",
            "Dùng seller có diachi_tk_seed, category/brand từ seed và variants không rỗng.",
            async ctx =>
            {
                var seller = LaySellerCoDiaChi(ctx);
                var duLieu = TaoDuLieuBodySanPhamMoi(ctx, seller);
                var req = new YeuCauApi(HttpMethod.Post, "/api/add_product", duLieu.Body, await LayTokenCuaTaiKhoanAsync(ctx, seller));
                req.Tam["seller"] = seller;
                req.Tam["duLieu"] = duLieu;
                return req;
            },
            Ok,
            sauKhiDat: async (response, request, ctx) =>
            {
                var seller = (TaiKhoanSignupThanhCongSeed)request.Tam["seller"]!;
                var duLieu = (DuLieuBodySanPham)request.Tam["duLieu"]!;
                var sanPhamIdServer = DocIdSanPham(response.Data)
                    ?? throw new LoiChuanBiKiemThuException(
                        "PRODUCT-ADD-01 đã thành công nhưng response thiếu product id nên không thể lưu sanpham_seed.");

                await ctx.CapNhatDB.ThemSanPhamSauAddProductAsync(
                    seller,
                    sanPhamIdServer,
                    duLieu.DanhMuc.DanhMucIdServer,
                    duLieu.ThuongHieu?.ThuongHieuIdServer,
                    duLieu.DiaChi.DiaChiIdServer,
                    response.Data?["name"]?.ToString() ?? duLieu.Ten,
                    duLieu.Gia);
            });

        Them(ds, "PRODUCT-ADD-02", "Product", "Thêm sản phẩm token thiếu/sai/hết hạn",
            "Gửi body hợp lệ nhưng Authorization sai định dạng.",
            ctx =>
            {
                var seller = LaySellerCoDiaChi(ctx);
                var duLieu = TaoDuLieuBodySanPhamMoi(ctx, seller);
                return Req(HttpMethod.Post, "/api/add_product", duLieu.Body, ctx.TokenSaiDinhDang);
            },
            SaiToken);

        Them(ds, "PRODUCT-ADD-03", "Product", "Thêm sản phẩm thiếu field",
            "Thiếu title/description/ship_from_id/variants.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/add_product", Obj(("price", 100000), ("category_id", IdBatBuoc(LayDanhMucSanSang(ctx).DanhMucIdServer, "danhmuc_seed.dm_id_server"))), await LayTokenCuaTaiKhoanAsync(ctx, LaySellerCoDiaChi(ctx))),
            ThieuThamSo);

        Them(ds, "PRODUCT-ADD-04", "Product", "Thêm sản phẩm sai kiểu",
            "price/category_id/ship_from_id/image_urls/variants sai kiểu.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/add_product",
                Obj(("title", "Sai kiểu"), ("price", "abc"), ("description", "Sai kiểu"), ("category_id", "abc"), ("ship_from_id", "abc"), ("image_urls", "khong_phai_mang"), ("variants", new[] { Obj(("size", "M"), ("stock", "abc"), ("color", "Đỏ"), ("weight", "abc")) })),
                await LayTokenCuaTaiKhoanAsync(ctx, LaySellerCoDiaChi(ctx))),
            SaiKieu);

        Them(ds, "PRODUCT-ADD-05", "Product", "Thêm sản phẩm giá trị không hợp lệ",
            "title quá 255 ký tự, price/stock âm, id tham chiếu không tồn tại và image_urls trùng.",
            async ctx =>
            {
                var seller = LaySellerCoDiaChi(ctx);
                var duLieu = TaoDuLieuBodySanPhamMoi(ctx, seller);
                duLieu.Body["title"] = new string('A', 256);
                duLieu.Body["price"] = -1;
                duLieu.Body["category_id"] = 999999999;
                duLieu.Body["brand_id"] = 999999999;
                duLieu.Body["ship_from_id"] = 999999999;
                duLieu.Body["image_urls"] = new[] { "https://example.com/trung.jpg", "https://example.com/trung.jpg" };
                duLieu.Body["variants"] = new[] { Obj(("size", "M"), ("stock", -1), ("color", "Đỏ"), ("weight", 1)) };
                return new YeuCauApi(HttpMethod.Post, "/api/add_product", duLieu.Body, await LayTokenCuaTaiKhoanAsync(ctx, seller));
            },
            SaiGiaTri);
    }

    private static void ThemUpdateProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-UPDATE-01", "Product", "Cập nhật sản phẩm hợp lệ",
            "Seller cập nhật title/price/description của sản phẩm mình sở hữu.",
            async ctx =>
            {
                var (sp, seller) = LaySanPhamKemSeller(ctx);
                var tenMoi = $"Cập nhật seed {DateTimeOffset.Now:yyyyMMddHHmmss}";
                var giaMoi = Math.Max(sp.Gia + 1000m, 1000m);
                var req = new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("title", tenMoi), ("price", giaMoi), ("description", "Cập nhật từ testcase tự động.")),
                    await LayTokenCuaTaiKhoanAsync(ctx, seller));
                req.Tam["sanPham"] = sp;
                req.Tam["tenMoi"] = tenMoi;
                req.Tam["giaMoi"] = giaMoi;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var sp = (SanPhamSeed)request.Tam["sanPham"]!;
                await ctx.CapNhatDB.CapNhatSanPhamSauUpdateAsync(
                    sp,
                    (string)request.Tam["tenMoi"]!,
                    (decimal)request.Tam["giaMoi"]!);
            });

        Them(ds, "PRODUCT-UPDATE-02", "Product", "Cập nhật sản phẩm token sai",
            "Authorization sai định dạng.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}", Obj(("title", "Token sai")), ctx.TokenSaiDinhDang);
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-UPDATE-03", "Product", "Cập nhật sản phẩm không tồn tại",
            "id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Patch, "/api/update/999999999", Obj(("title", "Không tồn tại")), await YeuCauTokenHopLeAsync(ctx)),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-UPDATE-04", "Product", "Cập nhật thiếu dữ liệu con",
            "variants rỗng.",
            async ctx =>
            {
                var (sp, seller) = LaySanPhamKemSeller(ctx);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}", Obj(("variants", Array.Empty<object>())), await LayTokenCuaTaiKhoanAsync(ctx, seller));
            },
            ThieuThamSo);

        Them(ds, "PRODUCT-UPDATE-05", "Product", "Cập nhật sản phẩm sai kiểu",
            "price/category_id/ship_from_id sai kiểu.",
            async ctx =>
            {
                var (sp, seller) = LaySanPhamKemSeller(ctx);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("price", "abc"), ("category_id", "abc"), ("ship_from_id", "abc"), ("variants", new[] { Obj(("size", "M"), ("stock", "abc"), ("color", "Đỏ"), ("weight", "abc")) })),
                    await LayTokenCuaTaiKhoanAsync(ctx, seller));
            },
            SaiKieu);

        Them(ds, "PRODUCT-UPDATE-06", "Product", "Cập nhật sản phẩm giá trị không hợp lệ",
            "price < 0, category/ship_from không tồn tại.",
            async ctx =>
            {
                var (sp, seller) = LaySanPhamKemSeller(ctx);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("price", -1), ("category_id", 999999999), ("ship_from_id", 999999999)),
                    await LayTokenCuaTaiKhoanAsync(ctx, seller));
            },
            SaiGiaTri);

        Them(ds, "PRODUCT-UPDATE-07", "Product", "Cập nhật sản phẩm vượt quá số ảnh",
            "image_urls có 5 ảnh.",
            async ctx =>
            {
                var (sp, seller) = LaySanPhamKemSeller(ctx);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("image_urls", new[] { "https://example.com/u1.jpg", "https://example.com/u2.jpg", "https://example.com/u3.jpg", "https://example.com/u4.jpg", "https://example.com/u5.jpg" })),
                    await LayTokenCuaTaiKhoanAsync(ctx, seller));
            },
            Tap("1008"));

        Them(ds, "PRODUCT-UPDATE-08", "Product", "Cập nhật sản phẩm không có quyền",
            "User khác seller gọi PATCH /api/update/:id.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanXemSanPhamKhongBiChan(ctx);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}", Obj(("title", "Không có quyền")), await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            KhongCoQuyen);
    }

    private static void ThemGetUserListings(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LISTING-01", "Product", "Lấy listing của chính seller",
            "Không truyền user_id, server lấy user trong token.",
            async ctx =>
            {
                var (_, seller) = LaySanPhamKemSeller(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10)), await LayTokenCuaTaiKhoanAsync(ctx, seller));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-LISTING-02", "Product", "Lấy listing của user khác",
            "Token user A, user_id là seller B có sản phẩm.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanXemSanPhamKhongBiChan(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("index", 0), ("count", 10), ("user_id", IdBatBuoc(sp.TaiKhoanIdServer, "sanpham_seed.tk_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-LISTING-03", "Product", "Lấy listing không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10), ("user_id", IdBatBuoc(sp.TaiKhoanIdServer, "sanpham_seed.tk_id_server"))));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LISTING-04", "Product", "Lấy listing token sai",
            "Authorization sai định dạng.",
            _ => Req(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10)), "token-sai"),
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LISTING-05", "Product", "Lấy listing user_id không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10), ("user_id", 999999999)), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-LISTING-06", "Product", "Lấy listing thiếu index/count",
            "Body rỗng, thiếu index/count bắt buộc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "PRODUCT-LISTING-07", "Product", "Lấy listing filter sai kiểu",
            "user_id/category_id sai kiểu dữ liệu.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                Obj(("index", 0), ("count", 10), ("user_id", "abc"), ("category_id", "abc")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static void ThemDeleteProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-DELETE-01", "Product", "Xóa sản phẩm hợp lệ",
            "Seller xóa một sản phẩm đang san_sang.",
            async ctx =>
            {
                var (sp, seller) = LaySanPhamCoTheXoaKemSeller(ctx);
                var req = new YeuCauApi(HttpMethod.Delete, $"/api/delete/{sp.SanPhamIdServer}", token: await LayTokenCuaTaiKhoanAsync(ctx, seller));
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var sp = (SanPhamSeed)request.Tam["sanPham"]!;
                await ctx.CapNhatDB.DanhDauSanPhamDaXoaAsync(sp);
            });

        Them(ds, "PRODUCT-DELETE-02", "Product", "Xóa sản phẩm không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Delete, $"/api/delete/{sp.SanPhamIdServer}");
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-DELETE-03", "Product", "Xóa sản phẩm token sai",
            "Authorization sai định dạng.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Delete, $"/api/delete/{sp.SanPhamIdServer}", token: ctx.TokenSaiDinhDang);
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-DELETE-04", "Product", "Xóa sản phẩm không có quyền",
            "User khác seller gọi DELETE.",
            async ctx =>
            {
                var (user, sp) = ChonCapTaiKhoanXemSanPhamKhongBiChan(ctx);
                return new YeuCauApi(HttpMethod.Delete, $"/api/delete/{sp.SanPhamIdServer}", token: await LayTokenCuaTaiKhoanAsync(ctx, user));
            },
            KhongCoQuyen);

        Them(ds, "PRODUCT-DELETE-05", "Product", "Xóa sản phẩm không tồn tại",
            "id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/999999999", token: await YeuCauTokenHopLeAsync(ctx)),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-DELETE-06", "Product", "Xóa sản phẩm id sai kiểu",
            "id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/abc", token: await YeuCauTokenHopLeAsync(ctx)),
            ProductSaiIdDuongDan);

        Them(ds, "PRODUCT-DELETE-07", "Product", "Xóa sản phẩm id không hợp lệ",
            "id = 0.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/0", token: await YeuCauTokenHopLeAsync(ctx)),
            Tap("1004", "9992"));

        Them(ds, "PRODUCT-DELETE-08", "Product", "Xóa sản phẩm đã phát sinh đơn hàng",
            "Cần seed đơn hàng có liên kết tới sản phẩm để kiểm tra nghiệp vụ này.",
            _ => Req(HttpMethod.Delete, "/api/delete/0"),
            Ok,
            lyDoBoQua: "Đã có donhang_seed nhưng server hiện chưa có mã nghiệp vụ ổn định cho xóa sản phẩm đã phát sinh order_items.");
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraSoLuongMangToiDa(int toiDa)
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

            return Task.FromResult(mang.Count <= toiDa
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data có {mang.Count} phần tử, vượt quá count = {toiDa}."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraCanEditNeuCo(bool mongDoi)
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var canEdit = DocBoolTuNode(response.Data?["can_edit"]);
            if (canEdit is null)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            return Task.FromResult(canEdit == mongDoi
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data.can_edit phải bằng {mongDoi}, thực tế là {canEdit}."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraChiTietPublic()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu data object."));
            }

            if (!data.ContainsKey("id"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data thiếu trường `id`."));
            }

            var canEdit = DocBoolTuNode(data["can_edit"]);
            if (canEdit != false)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.can_edit phải bằng False, thực tế là {canEdit?.ToString() ?? "null"}."));
            }

            var isLiked = DocBoolTuNode(data["is_liked"]);
            if (isLiked != false)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.is_liked phải bằng False, thực tế là {isLiked?.ToString() ?? "null"}."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraLike(bool mongDoiDaLike)
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var daLike = DocBoolTuNode(response.Data?["is_liked"]);
            if (daLike is null)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            return Task.FromResult(daLike == mongDoiDaLike
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data.is_liked phải bằng {mongDoiDaLike}, thực tế là {daLike}."));
        };
    }

}








