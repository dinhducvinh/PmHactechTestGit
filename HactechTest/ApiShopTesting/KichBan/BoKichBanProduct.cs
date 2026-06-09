using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> ProductOkHoacHetDuLieu = Tap("1000", "9994");
    private static readonly IReadOnlySet<string> ProductOkHoacKhongTonTai = Tap("1000", "9992");
    private static readonly IReadOnlySet<string> ProductKhongTonTai = Tap("9992");
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
                return Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", SoIdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server"))));
            },
            ProductOkHoacHetDuLieu,
            DataLaMang());

        Them(ds, "PRODUCT-CAT-03", "Product", "Lấy danh mục parent_id không có con",
            "parent_id lấy từ danh mục tồn tại nhưng không có category con.",
            ctx =>
            {
                var dm = LayDanhMucKhongCoCon(ctx);
                return Req(HttpMethod.Post, "/api/get_categories", Obj(("parent_id", SoIdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server"))));
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
                return Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", SoIdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server")), ("index", 0), ("count", 10)));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-BRAND-04", "Product", "Lấy thương hiệu category không có brand",
            "category_id tồn tại nhưng không có thương hiệu liên kết.",
            ctx =>
            {
                var dm = LayDanhMucKhongCoThuongHieu(ctx);
                return Req(HttpMethod.Post, "/api/get_list_brands", Obj(("category_id", SoIdBatBuoc(dm.DanhMucIdServer, "danhmuc_seed.dm_id_server")), ("index", 0), ("count", 10)));
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
            "Token user không có block với seller, id lấy từ sanpham_seed.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var user = LayTaiKhoanKhacSellerKhongBiChan(ctx, sp);
                var req = new YeuCauApi(HttpMethod.Post, "/api/get_products",
                    Obj(("id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            DataCoTruong("id"));

        Them(ds, "PRODUCT-DETAIL-02", "Product", "Seller xem sản phẩm của mình",
            "Dùng token của seller sở hữu product, kỳ vọng can_edit = true nếu server trả trường này.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products",
                    Obj(("id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            ProductOkHoacKhongTonTai,
            KiemTraCanEditNeuCo(true));

        Them(ds, "PRODUCT-DETAIL-03", "Product", "User khác xem sản phẩm",
            "Dùng token của user khác seller, kỳ vọng can_edit = false nếu server trả trường này.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var user = LayTaiKhoanKhacSellerKhongBiChan(ctx, sp);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products",
                    Obj(("id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            Ok,
            KiemTraCanEditNeuCo(false));

        Them(ds, "PRODUCT-DETAIL-04", "Product", "Lấy sản phẩm không tồn tại",
            "id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(("id", 999999999)), await ctx.YeuCauTokenHopLeAsync()),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-DETAIL-05", "Product", "Lấy sản phẩm thiếu id",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_products", Obj(), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-DETAIL-06", "Product", "Lấy sản phẩm không gửi token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/get_products", Obj(("id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-DETAIL-07", "Product", "Lấy sản phẩm khi có quan hệ block",
            "Current user và seller có quan hệ block theo tk_chan_seed. Server hiện vẫn có thể trả 1000.",
            async ctx =>
            {
                var (user, sp) = LayCapCoQuanHeChanVoiSanPham(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/get_products",
                    Obj(("id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            ProductOkHoacKhongTonTai);
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
                    ("category_id", SoIdBatBuoc(sp.DanhMucIdServer, "sanpham_seed.dm_id_server")),
                    ("keyword", sp.TenSanPham),
                    ("price_min", 0),
                    ("price_max", Math.Max(sp.Gia + 10000m, 1m)),
                    ("order", "created_desc"),
                    ("index", 0),
                    ("count", 10));
                if (!string.IsNullOrWhiteSpace(sp.ThuongHieuIdServer))
                {
                    body["brand_id"] = SoIdBatBuoc(sp.ThuongHieuIdServer, "sanpham_seed.thuonghieu_id_server");
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
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("index", 0), ("count", 10)));
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
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("index", 999999), ("count", 10)));
            },
            Tap("9994"));
    }

    private static void ThemSetCommentsProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-COMMENT-SET-01", "Product", "Tạo bình luận sản phẩm",
            "Token hợp lệ, product tồn tại, user không block seller.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var user = LayTaiKhoanKhacSellerKhongBiChan(ctx, sp);
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("content", $"Binh luan test {DateTimeOffset.Now:HHmmss}"), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            Ok,
            DataLaMang());

        Them(ds, "PRODUCT-COMMENT-SET-02", "Product", "Tạo bình luận không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("content", "No token"), ("index", 0), ("count", 10)));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-COMMENT-SET-03", "Product", "Tạo bình luận sản phẩm không tồn tại",
            "product_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("product_id", 999999999), ("content", "Product khong ton tai"), ("index", 0), ("count", 10)),
                await ctx.YeuCauTokenHopLeAsync()),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-COMMENT-SET-04", "Product", "Tạo bình luận thiếu tham số",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product", Obj(), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-05", "Product", "Tạo bình luận sai kiểu",
            "product_id = abc, index/count sai kiểu.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                Obj(("product_id", "abc"), ("content", "Sai kieu"), ("index", "abc"), ("count", "xyz")),
                await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-COMMENT-SET-06", "Product", "Tạo bình luận khi có quan hệ block",
            "Current user và seller có quan hệ block theo tk_chan_seed. Server hiện chưa chặn API này.",
            async ctx =>
            {
                var (user, sp) = LayCapCoQuanHeChanVoiSanPham(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/set_comments_product",
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("content", $"Block comment {DateTimeOffset.Now:HHmmss}"), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            Ok,
            DataLaMang());
    }

    private static void ThemLikeProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LIKE-01", "Product", "Like sản phẩm mới",
            "Chọn cặp user/product chưa có trong tk_thich_sanpham_seed.",
            async ctx =>
            {
                var (user, sp) = await LayCapChuaLikeSanPhamKhopServerAsync(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/api/like_product",
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
                req.Tam["taiKhoan"] = user;
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            KiemTraLike(true),
            async (response, request, ctx) =>
            {
                var user = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoan"]!;
                var sp = (SanPhamSeed)request.Tam["sanPham"]!;
                if (!ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Any(x =>
                    x.TaiKhoanIdServer == user.TaiKhoanIdServer &&
                    x.SanPhamIdServer == sp.SanPhamIdServer &&
                    x.TrangThai == "san_sang"))
                {
                    ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
                    {
                        ThichSanPhamSeedId = TaoIdLikeSanPhamMoi(ctx),
                        TaiKhoanIdServer = user.TaiKhoanIdServer,
                        SanPhamIdServer = sp.SanPhamIdServer,
                        ThichLuc = DateTimeOffset.Now,
                        TrangThai = "san_sang",
                        GhiChu = "Tao boi testcase PRODUCT-LIKE-01"
                    });
                    await ctx.KhoSeed.LuuAsync();
                }
            });

        Them(ds, "PRODUCT-LIKE-02", "Product", "Unlike sản phẩm đã like",
            "Dùng một dòng tk_thich_sanpham_seed đang san_sang.",
            async ctx =>
            {
                var (like, user) = await LayHoacTaoLikeSanPhamKhopServerAsync(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/api/like_product",
                    Obj(("product_id", SoIdBatBuoc(like.SanPhamIdServer, "tk_thich_sanpham_seed.sp_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
                req.Tam["like"] = like;
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
                ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Remove(like);
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "PRODUCT-LIKE-03", "Product", "Like sản phẩm không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/like_product", Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LIKE-04", "Product", "Like sản phẩm token sai",
            "Authorization sai định dạng.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/like_product", Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))), ctx.TokenSaiDinhDang);
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LIKE-05", "Product", "Like sản phẩm thiếu product_id",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIKE-06", "Product", "Like sản phẩm product_id sai kiểu",
            "product_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(("product_id", "abc")), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-LIKE-07", "Product", "Like sản phẩm không tồn tại",
            "product_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/like_product", Obj(("product_id", 999999999)), await ctx.YeuCauTokenHopLeAsync()),
            ProductKhongTonTai);
    }

    private static void ThemReportProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-REPORT-01", "Product", "Báo cáo sản phẩm hợp lệ",
            "Token hợp lệ, product tồn tại, user không block seller.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var user = LayTaiKhoanKhacSellerKhongBiChan(ctx, sp);
                return new YeuCauApi(HttpMethod.Post, "/api/report_product",
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("subject", "Noi dung can kiem tra"), ("details", "Bao cao tu testcase tu dong.")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            Ok);

        Them(ds, "PRODUCT-REPORT-02", "Product", "Báo cáo sản phẩm không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/report_product",
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("subject", "No token"), ("details", "No token")));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-REPORT-03", "Product", "Báo cáo sản phẩm không tồn tại",
            "product_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/report_product",
                Obj(("product_id", 999999999), ("subject", "Khong ton tai"), ("details", "Khong ton tai")),
                await ctx.YeuCauTokenHopLeAsync()),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-REPORT-04", "Product", "Báo cáo sản phẩm thiếu tham số",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/report_product", Obj(), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-REPORT-05", "Product", "Báo cáo sản phẩm sai kiểu",
            "product_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/report_product",
                Obj(("product_id", "abc"), ("subject", "Sai kieu"), ("details", "Sai kieu")),
                await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-REPORT-06", "Product", "Báo cáo sản phẩm khi có quan hệ block",
            "Current user và seller có quan hệ block theo tk_chan_seed.",
            async ctx =>
            {
                var (user, sp) = LayCapCoQuanHeChanVoiSanPham(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/report_product",
                    Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("subject", "Blocked report"), ("details", "Bao cao khi co quan he block.")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            KhongCoQuyen);
    }

    private static void ThemAddProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-ADD-01", "Product", "Thêm sản phẩm hợp lệ",
            "Dùng seller có diachi_tk_seed, category/brand từ seed.",
            async ctx =>
            {
                var seller = LaySellerCoDiaChi(ctx);
                var duLieu = TaoDuLieuBodySanPhamMoi(ctx, seller);
                var req = new YeuCauApi(HttpMethod.Post, "/api/add_product", duLieu.Body, await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
                req.Tam["seller"] = seller;
                req.Tam["duLieu"] = duLieu;
                return req;
            },
            Ok,
            DataCoTruong("id"),
            async (response, request, ctx) =>
            {
                var seller = (TaiKhoanSignupThanhCongSeed)request.Tam["seller"]!;
                var duLieu = (DuLieuBodySanPham)request.Tam["duLieu"]!;
                var sanPhamIdServer = DocIdSanPham(response.Data);
                if (string.IsNullOrWhiteSpace(sanPhamIdServer))
                {
                    return;
                }

                ctx.KhoSeed.DuLieu.SanPhamSeed.Add(new SanPhamSeed
                {
                    ThuTuNoiBo = TaoIdSanPhamMoi(ctx),
                    SanPhamIdServer = sanPhamIdServer,
                    TaiKhoanIdServer = seller.TaiKhoanIdServer,
                    DanhMucIdServer = duLieu.DanhMuc.DanhMucIdServer,
                    ThuongHieuIdServer = duLieu.ThuongHieu?.ThuongHieuIdServer,
                    DiaChiGuiHangIdServer = duLieu.DiaChi.DiaChiIdServer,
                    TenSanPham = TienIchJson.DocChuoi(response.Data, "title", "name") ?? duLieu.Ten,
                    Gia = duLieu.Gia,
                    TrangThai = "san_sang",
                    TaoBoiTest = true,
                    TaoLuc = DateTimeOffset.Now,
                    XacMinhLuc = DateTimeOffset.Now,
                    GhiChu = "Tao boi testcase PRODUCT-ADD-01"
                });
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "PRODUCT-ADD-02", "Product", "Thêm sản phẩm token sai",
            "Authorization sai định dạng.",
            ctx =>
            {
                var seller = LaySellerCoDiaChi(ctx);
                var duLieu = TaoDuLieuBodySanPhamMoi(ctx, seller);
                return Req(HttpMethod.Post, "/api/add_product", duLieu.Body, ctx.TokenSaiDinhDang);
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-ADD-03", "Product", "Thêm sản phẩm thiếu field",
            "Thiếu title/description/ship_from_id/variants.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/add_product", Obj(("price", 100000), ("category_id", SoIdBatBuoc(LayDanhMucSanSang(ctx).DanhMucIdServer, "danhmuc_seed.dm_id_server"))), await ctx.YeuCauTokenCuaTaiKhoanAsync(LaySellerCoDiaChi(ctx))),
            ThieuThamSo);

        Them(ds, "PRODUCT-ADD-04", "Product", "Thêm sản phẩm sai kiểu",
            "price/category_id/ship_from_id sai kiểu.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/add_product",
                Obj(("title", "Sai kieu"), ("price", "abc"), ("description", "Sai kieu"), ("category_id", "abc"), ("ship_from_id", "abc"), ("variants", new[] { Obj(("size", "M"), ("stock", "abc"), ("color", "Do"), ("weight", "abc")) })),
                await ctx.YeuCauTokenCuaTaiKhoanAsync(LaySellerCoDiaChi(ctx))),
            SaiKieu);

        Them(ds, "PRODUCT-ADD-05", "Product", "Thêm sản phẩm giá trị không hợp lệ",
            "price < 0, ship_from_id/category_id không tồn tại.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/add_product",
                Obj(("title", "Gia tri sai"), ("price", -1), ("description", "Gia tri sai"), ("category_id", 999999999), ("ship_from_id", 999999999), ("variants", new[] { Obj(("size", "M"), ("stock", -1), ("color", "Do"), ("weight", -1)) })),
                await ctx.YeuCauTokenCuaTaiKhoanAsync(LaySellerCoDiaChi(ctx))),
            SaiGiaTri);

        Them(ds, "PRODUCT-ADD-06", "Product", "Thêm sản phẩm vượt quá số ảnh",
            "image_urls có 5 ảnh.",
            async ctx =>
            {
                var seller = LaySellerCoDiaChi(ctx);
                var duLieu = TaoDuLieuBodySanPhamMoi(ctx, seller);
                duLieu.Body["image_urls"] = new[] { "https://example.com/1.jpg", "https://example.com/2.jpg", "https://example.com/3.jpg", "https://example.com/4.jpg", "https://example.com/5.jpg" };
                return new YeuCauApi(HttpMethod.Post, "/api/add_product", duLieu.Body, await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            Tap("1008"));
    }

    private static void ThemUpdateProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-UPDATE-01", "Product", "Cập nhật sản phẩm hợp lệ",
            "Seller cập nhật title/price/description của sản phẩm mình sở hữu.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                var tenMoi = $"Cap nhat seed {DateTimeOffset.Now:yyyyMMddHHmmss}";
                var giaMoi = Math.Max(sp.Gia + 1000m, 1000m);
                var req = new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("title", tenMoi), ("price", giaMoi), ("description", "Cap nhat tu testcase tu dong.")),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
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
                sp.TenSanPham = (string)request.Tam["tenMoi"]!;
                sp.Gia = (decimal)request.Tam["giaMoi"]!;
                sp.XacMinhLuc = DateTimeOffset.Now;
                sp.GhiChu = "Cap nhat boi testcase PRODUCT-UPDATE-01";
                await ctx.KhoSeed.LuuAsync();
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
            async ctx => new YeuCauApi(HttpMethod.Patch, "/api/update/999999999", Obj(("title", "Khong ton tai")), await ctx.YeuCauTokenHopLeAsync()),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-UPDATE-04", "Product", "Cập nhật thiếu dữ liệu con",
            "variants rỗng.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}", Obj(("variants", Array.Empty<object>())), await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            ThieuThamSo);

        Them(ds, "PRODUCT-UPDATE-05", "Product", "Cập nhật sản phẩm sai kiểu",
            "price/category_id/ship_from_id sai kiểu.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("price", "abc"), ("category_id", "abc"), ("ship_from_id", "abc"), ("variants", new[] { Obj(("size", "M"), ("stock", "abc"), ("color", "Do"), ("weight", "abc")) })),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            SaiKieu);

        Them(ds, "PRODUCT-UPDATE-06", "Product", "Cập nhật sản phẩm giá trị không hợp lệ",
            "price < 0, category/ship_from không tồn tại.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("price", -1), ("category_id", 999999999), ("ship_from_id", 999999999)),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            SaiGiaTri);

        Them(ds, "PRODUCT-UPDATE-07", "Product", "Cập nhật sản phẩm vượt quá số ảnh",
            "image_urls có 5 ảnh.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}",
                    Obj(("image_urls", new[] { "https://example.com/u1.jpg", "https://example.com/u2.jpg", "https://example.com/u3.jpg", "https://example.com/u4.jpg", "https://example.com/u5.jpg" })),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            Tap("1008"));

        Them(ds, "PRODUCT-UPDATE-08", "Product", "Cập nhật sản phẩm không có quyền",
            "User khác seller gọi PATCH /api/update/:id.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var user = LayTaiKhoanKhacSellerKhongBiChan(ctx, sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SanPhamIdServer}", Obj(("title", "Khong co quyen")), await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            KhongCoQuyen);
    }

    private static void ThemGetUserListings(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LISTING-01", "Product", "Lấy listing của chính seller",
            "Không truyền user_id, server lấy user trong token.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10)), await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-LISTING-02", "Product", "Lấy listing của user khác",
            "Token user A, user_id là seller B có sản phẩm.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                var user = LayTaiKhoanKhacSellerKhongBiChan(ctx, sp);
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("index", 0), ("count", 10), ("user_id", SoIdBatBuoc(sp.TaiKhoanIdServer, "sanpham_seed.tk_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            ProductOkHoacHetDuLieu,
            KiemTraSoLuongMangToiDa(10));

        Them(ds, "PRODUCT-LISTING-03", "Product", "Lấy listing không token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10), ("user_id", SoIdBatBuoc(sp.TaiKhoanIdServer, "sanpham_seed.tk_id_server"))));
            },
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LISTING-04", "Product", "Lấy listing token sai",
            "Authorization sai định dạng.",
            _ => Req(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10)), "token-sai"),
            ProductSaiTokenHoacNguoiDung);

        Them(ds, "PRODUCT-LISTING-05", "Product", "Lấy listing user_id không tồn tại",
            "user_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10), ("user_id", 999999999)), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);
    }

    private static void ThemDeleteProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-DELETE-01", "Product", "Xóa sản phẩm hợp lệ",
            "Seller xóa một sản phẩm đang san_sang.",
            async ctx =>
            {
                var sp = LaySanPhamCoTheXoa(ctx);
                var seller = LaySellerCuaSanPham(ctx, sp);
                var req = new YeuCauApi(HttpMethod.Delete, $"/api/delete/{sp.SanPhamIdServer}", token: await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var sp = (SanPhamSeed)request.Tam["sanPham"]!;
                sp.TrangThai = "da_xoa";
                sp.XacMinhLuc = DateTimeOffset.Now;
                sp.GhiChu = "Xoa boi testcase PRODUCT-DELETE-01";
                await ctx.KhoSeed.LuuAsync();
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
                var sp = LaySanPhamSanSang(ctx);
                var user = LayTaiKhoanKhacSellerKhongBiChan(ctx, sp);
                return new YeuCauApi(HttpMethod.Delete, $"/api/delete/{sp.SanPhamIdServer}", token: await ctx.YeuCauTokenCuaTaiKhoanAsync(user));
            },
            KhongCoQuyen);

        Them(ds, "PRODUCT-DELETE-05", "Product", "Xóa sản phẩm không tồn tại",
            "id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/999999999", token: await ctx.YeuCauTokenHopLeAsync()),
            ProductKhongTonTai);

        Them(ds, "PRODUCT-DELETE-06", "Product", "Xóa sản phẩm id sai kiểu",
            "id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/abc", token: await ctx.YeuCauTokenHopLeAsync()),
            ProductSaiIdDuongDan);

        Them(ds, "PRODUCT-DELETE-07", "Product", "Xóa sản phẩm id không hợp lệ",
            "id = 0.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/0", token: await ctx.YeuCauTokenHopLeAsync()),
            Tap("1004", "9992"));

        Them(ds, "PRODUCT-DELETE-08", "Product", "Xóa sản phẩm đã phát sinh đơn hàng",
            "Cần seed đơn hàng có liên kết tới sản phẩm để kiểm tra nghiệp vụ này.",
            _ => Req(HttpMethod.Delete, "/api/delete/0"),
            Ok,
            lyDoBoQua: "Chưa có donhang_seed/donhang_sanpham_seed để tự động chọn sản phẩm đã phát sinh đơn hàng.");
    }

    private static DanhMucSeed LayDanhMucSanSang(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.DanhMucIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu danhmuc_seed trạng thái san_sang.");
    }

    private static DanhMucSeed LayDanhMucCoCon(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                !string.IsNullOrWhiteSpace(x.DanhMucIdServer) &&
                (x.CoDanhMucCon == true ||
                 ctx.KhoSeed.DuLieu.DanhMucSeed.Any(con =>
                     con.TrangThai == "san_sang" &&
                     con.DanhMucChaIdServer == x.DanhMucIdServer)))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed có danh mục con.");
    }

    private static DanhMucSeed LayDanhMucKhongCoCon(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                !string.IsNullOrWhiteSpace(x.DanhMucIdServer) &&
                x.CoDanhMucCon != true &&
                !ctx.KhoSeed.DuLieu.DanhMucSeed.Any(con =>
                    con.TrangThai == "san_sang" &&
                    con.DanhMucChaIdServer == x.DanhMucIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed tồn tại nhưng không có danh mục con.");
    }

    private static DanhMucSeed LayDanhMucCoThuongHieu(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                !string.IsNullOrWhiteSpace(x.DanhMucIdServer) &&
                (x.CoThuongHieu == true ||
                 ctx.KhoSeed.DuLieu.ThuongHieuSeed.Any(th =>
                     th.TrangThai == "san_sang" &&
                     th.DanhMucIdServer == x.DanhMucIdServer)))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed có thương hiệu.");
    }

    private static DanhMucSeed LayDanhMucKhongCoThuongHieu(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                !string.IsNullOrWhiteSpace(x.DanhMucIdServer) &&
                x.CoThuongHieu != true &&
                !ctx.KhoSeed.DuLieu.ThuongHieuSeed.Any(th =>
                    th.TrangThai == "san_sang" &&
                    th.DanhMucIdServer == x.DanhMucIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed tồn tại nhưng không có thương hiệu.");
    }

    private static ThuongHieuSeed? LayThuongHieuSanSang(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.ThuongHieuSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.ThuongHieuIdServer));
    }

    private static SanPhamSeed LaySanPhamSanSang(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.SanPhamSeed
            .Where(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.SanPhamIdServer))
            .OrderBy(x => x.ThuTuNoiBo)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu sanpham_seed trạng thái san_sang.");
    }

    private static SanPhamSeed LaySanPhamCoTheXoa(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.SanPhamSeed
            .Where(x =>
                x.TrangThai == "san_sang" &&
                x.TaoBoiTest &&
                !string.IsNullOrWhiteSpace(x.SanPhamIdServer) &&
                !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer))
            .OrderByDescending(x => x.TaoLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.ThuTuNoiBo)
            .FirstOrDefault()
            ?? ctx.KhoSeed.DuLieu.SanPhamSeed
                .Where(x =>
                    x.TrangThai == "san_sang" &&
                    !string.IsNullOrWhiteSpace(x.SanPhamIdServer) &&
                    !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer))
                .OrderByDescending(x => x.TaoLuc ?? DateTimeOffset.MinValue)
                .ThenByDescending(x => x.ThuTuNoiBo)
                .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu sanpham_seed có thể xóa.");
    }

    private static TaiKhoanSignupThanhCongSeed LaySellerCuaSanPham(NguCanhKiemThu ctx, SanPhamSeed sanPham)
    {
        return LayTaiKhoanTheoServerId(ctx, sanPham.TaiKhoanIdServer)
            ?? throw new BoQuaKiemThuException($"Thiếu tài khoản seller của sản phẩm seed {sanPham.ThuTuNoiBo}.");
    }

    private static TaiKhoanSignupThanhCongSeed LaySellerCoDiaChi(NguCanhKiemThu ctx)
    {
        var seller = ctx.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(TaiKhoanSanSang)
            .FirstOrDefault(tk => ctx.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.Any(dc =>
                dc.TaiKhoanIdServer == tk.TaiKhoanIdServer &&
                dc.TrangThai == "san_sang" &&
                !string.IsNullOrWhiteSpace(dc.DiaChiIdServer)));

        return seller ?? throw new BoQuaKiemThuException("Thiếu seller đã đăng ký có diachi_tk_seed san_sang.");
    }

    private static TaiKhoanSignupThanhCongSeed LayTaiKhoanKhacSellerKhongBiChan(NguCanhKiemThu ctx, SanPhamSeed sanPham)
    {
        return ctx.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(TaiKhoanSanSang)
            .FirstOrDefault(tk => tk.TaiKhoanIdServer != sanPham.TaiKhoanIdServer && !CoQuanHeChan(ctx, tk.TaiKhoanIdServer, sanPham.TaiKhoanIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu tài khoản khác seller và không có quan hệ block với seller.");
    }

    private static (TaiKhoanSignupThanhCongSeed user, SanPhamSeed sanPham) LayCapCoQuanHeChanVoiSanPham(NguCanhKiemThu ctx)
    {
        var sanPhams = ctx.KhoSeed.DuLieu.SanPhamSeed
            .Where(x =>
                x.TrangThai == "san_sang" &&
                !string.IsNullOrWhiteSpace(x.SanPhamIdServer) &&
                !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer))
            .ToList();

        foreach (var chan in LayDanhSachQuanHeChanDangHoatDong(ctx))
        {
            var sanPhamBiChan = sanPhams.FirstOrDefault(x => x.TaiKhoanIdServer == chan.BlockedTaiKhoanIdServer);
            var blocker = LayTaiKhoanTheoServerId(ctx, chan.BlockerTaiKhoanIdServer);
            if (sanPhamBiChan is not null && blocker is not null)
            {
                return (blocker, sanPhamBiChan);
            }

            var sanPhamBlocker = sanPhams.FirstOrDefault(x => x.TaiKhoanIdServer == chan.BlockerTaiKhoanIdServer);
            var blocked = LayTaiKhoanTheoServerId(ctx, chan.BlockedTaiKhoanIdServer);
            if (sanPhamBlocker is not null && blocked is not null)
            {
                return (blocked, sanPhamBlocker);
            }
        }

        throw new BoQuaKiemThuException("Thiếu cặp user/sản phẩm có quan hệ block trong tk_chan_seed.");
    }

    private static (TaiKhoanSignupThanhCongSeed user, SanPhamSeed sanPham) LayCapLikeMoi(NguCanhKiemThu ctx)
    {
        foreach (var sp in ctx.KhoSeed.DuLieu.SanPhamSeed.Where(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.SanPhamIdServer)))
        {
            var user = ctx.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed
                .Where(TaiKhoanSanSang)
                .FirstOrDefault(tk =>
                    tk.TaiKhoanIdServer != sp.TaiKhoanIdServer &&
                    !CoQuanHeChan(ctx, tk.TaiKhoanIdServer, sp.TaiKhoanIdServer) &&
                    !ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Any(like =>
                        like.TrangThai == "san_sang" &&
                        like.TaiKhoanIdServer == tk.TaiKhoanIdServer &&
                        like.SanPhamIdServer == sp.SanPhamIdServer));

            if (user is not null)
            {
                return (user, sp);
            }
        }

        throw new BoQuaKiemThuException("Thiếu cặp tài khoản/sản phẩm chưa like để chạy PRODUCT-LIKE-01.");
    }

    private static async Task<(TaiKhoanSignupThanhCongSeed user, SanPhamSeed sanPham)> LayCapChuaLikeSanPhamKhopServerAsync(NguCanhKiemThu ctx)
    {
        var daBoSungSeedBiLech = false;
        foreach (var sp in ctx.KhoSeed.DuLieu.SanPhamSeed.Where(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.SanPhamIdServer)))
        {
            foreach (var user in ctx.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed.Where(TaiKhoanSanSang))
            {
                if (user.TaiKhoanIdServer == sp.TaiKhoanIdServer ||
                    CoQuanHeChan(ctx, user.TaiKhoanIdServer, sp.TaiKhoanIdServer) ||
                    ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Any(like =>
                        like.TrangThai == "san_sang" &&
                        like.TaiKhoanIdServer == user.TaiKhoanIdServer &&
                        like.SanPhamIdServer == sp.SanPhamIdServer))
                {
                    continue;
                }

                var response = await ctx.Api.GuiAsync(new YeuCauApi(
                    HttpMethod.Post,
                    "/api/get_products",
                    Obj(("id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(user)));

                if (!LaMaThanhCong(response))
                {
                    continue;
                }

                var daLike = TienIchJson.DocBool(response.Data, "is_liked", "liked");
                if (daLike == true)
                {
                    ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
                    {
                        ThichSanPhamSeedId = TaoIdLikeSanPhamMoi(ctx),
                        TaiKhoanIdServer = user.TaiKhoanIdServer,
                        SanPhamIdServer = sp.SanPhamIdServer,
                        ThichLuc = DateTimeOffset.Now,
                        TrangThai = "san_sang",
                        GhiChu = "Dong bo lai seed bi thieu truoc PRODUCT-LIKE-01"
                    });
                    daBoSungSeedBiLech = true;
                    continue;
                }

                if (daBoSungSeedBiLech)
                {
                    await ctx.KhoSeed.LuuAsync();
                }

                return (user, sp);
            }
        }

        if (daBoSungSeedBiLech)
        {
            await ctx.KhoSeed.LuuAsync();
        }

        throw new BoQuaKiemThuException("Thiếu cặp tài khoản/sản phẩm chưa like khớp trạng thái server để chạy PRODUCT-LIKE-01.");
    }

    private static async Task<(TaiKhoanThichSanPhamSeed Like, TaiKhoanSignupThanhCongSeed User)> LayHoacTaoLikeSanPhamKhopServerAsync(NguCanhKiemThu ctx)
    {
        var daXoaSeedLech = false;
        foreach (var like in ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed
                     .Where(x => x.TrangThai == "san_sang" &&
                                 !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer) &&
                                 !string.IsNullOrWhiteSpace(x.SanPhamIdServer))
                     .ToList())
        {
            var user = LayTaiKhoanTheoServerId(ctx, like.TaiKhoanIdServer);
            if (user is null)
            {
                ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Remove(like);
                daXoaSeedLech = true;
                continue;
            }

            var productId = SoIdBatBuoc(like.SanPhamIdServer, "tk_thich_sanpham_seed.sp_id_server");
            var token = await ctx.YeuCauTokenCuaTaiKhoanAsync(user);
            var response = await ctx.Api.GuiAsync(new YeuCauApi(
                HttpMethod.Post,
                "/api/get_products",
                Obj(("id", productId)),
                token));

            if (response.MaSoSanh == "9992")
            {
                ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Remove(like);
                daXoaSeedLech = true;
                continue;
            }

            if (LaMaThanhCong(response))
            {
                var daLike = TienIchJson.DocBool(response.Data, "is_liked", "liked");
                if (daLike != true)
                {
                    ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Remove(like);
                    daXoaSeedLech = true;
                    continue;
                }
            }

            if (daXoaSeedLech)
            {
                await ctx.KhoSeed.LuuAsync();
            }

            return (like, user);
        }

        if (daXoaSeedLech)
        {
            await ctx.KhoSeed.LuuAsync();
        }

        return await TaoLikeSanPhamChoUnlikeAsync(ctx);
    }

    private static async Task<(TaiKhoanThichSanPhamSeed Like, TaiKhoanSignupThanhCongSeed User)> TaoLikeSanPhamChoUnlikeAsync(NguCanhKiemThu ctx)
    {
        var (user, sp) = LayCapLikeMoi(ctx);
        var response = await ctx.Api.GuiAsync(new YeuCauApi(
            HttpMethod.Post,
            "/api/like_product",
            Obj(("product_id", SoIdBatBuoc(sp.SanPhamIdServer, "sanpham_seed.sp_id_server"))),
            await ctx.YeuCauTokenCuaTaiKhoanAsync(user)));

        if (!LaMaThanhCong(response) || TienIchJson.DocBool(response.Data, "is_liked", "liked") != true)
        {
            throw new BoQuaKiemThuException("Không tạo được dữ liệu like sản phẩm để chạy PRODUCT-LIKE-02.");
        }

        var like = new TaiKhoanThichSanPhamSeed
        {
            ThichSanPhamSeedId = TaoIdLikeSanPhamMoi(ctx),
            TaiKhoanIdServer = user.TaiKhoanIdServer,
            SanPhamIdServer = sp.SanPhamIdServer,
            ThichLuc = DateTimeOffset.Now,
            TrangThai = "san_sang",
            GhiChu = "Tao lai boi testcase PRODUCT-LIKE-02"
        };

        ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Add(like);
        await ctx.KhoSeed.LuuAsync();
        return (like, user);
    }

    private static DuLieuBodySanPham TaoDuLieuBodySanPhamMoi(NguCanhKiemThu ctx, TaiKhoanSignupThanhCongSeed seller)
    {
        var danhMuc = LayDanhMucSanSang(ctx);
        var thuongHieu = LayThuongHieuSanSang(ctx);
        var diaChi = ctx.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(x =>
            x.TaiKhoanIdServer == seller.TaiKhoanIdServer &&
            x.TrangThai == "san_sang" &&
            !string.IsNullOrWhiteSpace(x.DiaChiIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu địa chỉ seed của seller để thêm sản phẩm.");

        var soThuTu = ctx.KhoSeed.DuLieu.SanPhamSeed.Count + 1;
        var ten = $"San pham testcase {soThuTu} {DateTimeOffset.Now:HHmmss}";
        var gia = 150000m + soThuTu * 1000m;
        var body = Obj(
            ("title", ten),
            ("price", gia),
            ("description", "San pham tao tu testcase Product."),
            ("category_id", SoIdBatBuoc(danhMuc.DanhMucIdServer, "danhmuc_seed.dm_id_server")),
            ("ship_from_id", SoIdBatBuoc(diaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
            ("variants", new[] { Obj(("size", "M"), ("stock", 20), ("color", "Do"), ("weight", 500)) }));

        if (!string.IsNullOrWhiteSpace(thuongHieu?.ThuongHieuIdServer))
        {
            body["brand_id"] = SoIdBatBuoc(thuongHieu.ThuongHieuIdServer, "thuonghieu_seed.thuonghieu_id_server");
        }

        return new DuLieuBodySanPham(body, danhMuc, thuongHieu, diaChi, ten, gia);
    }

    private static string? DocIdSanPham(JsonNode? data)
    {
        return TienIchJson.DocChuoi(data, "id", "product_id", "sp_id") ??
               TienIchJson.DocChuoi(data?["product"], "id", "product_id", "sp_id");
    }

    private static int TaoIdSanPhamMoi(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.SanPhamSeed.Count == 0
            ? 1
            : ctx.KhoSeed.DuLieu.SanPhamSeed.Max(x => x.ThuTuNoiBo) + 1;
    }

    private static int TaoIdLikeSanPhamMoi(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Count == 0
            ? 1
            : ctx.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Max(x => x.ThichSanPhamSeedId) + 1;
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

            var canEdit = TienIchJson.DocBool(response.Data, "can_edit");
            if (canEdit is null)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            return Task.FromResult(canEdit == mongDoi
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data.can_edit phải bằng {mongDoi}, thực tế là {canEdit}."));
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

            var daLike = TienIchJson.DocBool(response.Data, "is_liked", "liked");
            if (daLike is null)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            return Task.FromResult(daLike == mongDoiDaLike
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data.is_liked phải bằng {mongDoiDaLike}, thực tế là {daLike}."));
        };
    }

    private sealed record DuLieuBodySanPham(
        Dictionary<string, object?> Body,
        DanhMucSeed DanhMuc,
        ThuongHieuSeed? ThuongHieu,
        DiaChiTaiKhoanSeed DiaChi,
        string Ten,
        decimal Gia);
}


