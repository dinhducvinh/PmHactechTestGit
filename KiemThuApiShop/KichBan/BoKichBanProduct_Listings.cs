using KiemThuApiShop.Core;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemGetUserListings(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-LISTING-01", "Product", "Lấy listing của current user",
            "Token seller có sản phẩm, không truyền user_id.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var seller = ctx.YeuCauSellerCuaSanPham(sp);
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10)), await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LISTING-02", "Product", "Lấy listing theo user_id seller có sản phẩm",
            "Truyền seller_tk_id từ sanpham_seed.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(sp.SellerTkId, "seller_tk_id")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LISTING-03", "Product", "Lấy listing user chưa đăng sản phẩm",
            "Dùng tài khoản không có sản phẩm trong sanpham_seed nếu có.",
            async ctx =>
            {
                var tk = ctx.KhoSeed.DuLieu.TaiKhoanSeed
                    .FirstOrDefault(x => x.TrangThaiDangKy == "da_dang_ky" && ctx.KhoSeed.DuLieu.SanPhamSeed.All(sp => sp.SellerTkSeedId != x.TkSeedId))
                    ?? throw new BoQuaKiemThuException("Thiếu user đã đăng ký nhưng chưa có sản phẩm.");
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(tk.TkId, "tk_id")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            KhongCoDuLieu);

        Them(ds, "PRODUCT-LISTING-04", "Product", "Lấy listing không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10))),
            SaiToken);

        Them(ds, "PRODUCT-LISTING-05", "Product", "Lấy listing với token không hợp lệ",
            "Token sai định dạng.",
            ctx => Req(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            SaiToken);

        Them(ds, "PRODUCT-LISTING-06", "Product", "Lấy listing thiếu index",
            "Chỉ truyền count.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);

        Them(ds, "PRODUCT-LISTING-07", "Product", "Lấy listing thiếu count",
            "Chỉ truyền index.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0)), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSo);

        Them(ds, "PRODUCT-LISTING-08", "Product", "Lấy listing index/count sai kiểu",
            "index = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", "abc"), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        Them(ds, "PRODUCT-LISTING-09", "Product", "Lấy listing index/count không hợp lệ",
            "count = 0.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("index", 0), ("count", 0)), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-LISTING-10", "Product", "Lấy listing user_id sai kiểu",
            "user_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("user_id", "abc"), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        Them(ds, "PRODUCT-LISTING-11", "Product", "Lấy listing user_id không tồn tại",
            "user_id = 999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_user_listings", Obj(("user_id", 999999), ("index", 0), ("count", 10)), await ctx.YeuCauTokenHopLeAsync()),
            Tap("1013", "1000", "9994"));

        Them(ds, "PRODUCT-LISTING-12", "Product", "Lọc listing theo keyword có kết quả",
            "Keyword là một phần tên sản phẩm seed.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var keyword = sp.TenSp.Length > 10 ? sp.TenSp[..10] : sp.TenSp;
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(sp.SellerTkId, "seller_tk_id")), ("keyword", keyword), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LISTING-13", "Product", "Lọc listing theo keyword không có kết quả",
            "Keyword không khớp sản phẩm nào.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(sp.SellerTkId, "seller_tk_id")), ("keyword", "khong_co_san_pham_987654"), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            KhongCoDuLieu);

        Them(ds, "PRODUCT-LISTING-14", "Product", "Lọc listing theo category_id hợp lệ",
            "Dùng category_id của sản phẩm seed.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(sp.SellerTkId, "seller_tk_id")), ("category_id", SoId(sp.DmId, "dm_id")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "PRODUCT-LISTING-15", "Product", "Lọc listing theo category_id không tồn tại",
            "category_id = 999999.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(sp.SellerTkId, "seller_tk_id")), ("category_id", 999999), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            KhongCoDuLieu);

        Them(ds, "PRODUCT-LISTING-16", "Product", "Lấy listing với index vượt tổng số sản phẩm",
            "index rất lớn.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(sp.SellerTkId, "seller_tk_id")), ("index", 9999), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            KhongCoDuLieu);

        Them(ds, "PRODUCT-LISTING-17", "Product", "Kiểm tra trường is_stock trong listing",
            "Seller có sản phẩm còn hàng/hết hàng.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return new YeuCauApi(HttpMethod.Post, "/api/get_user_listings",
                    Obj(("user_id", SoIdBatBuoc(sp.SellerTkId, "seller_tk_id")), ("index", 0), ("count", 10)),
                    await ctx.YeuCauTokenHopLeAsync());
            },
            Ok,
            DataLaMang("is_stock"));

        Them(ds, "PRODUCT-LISTING-18", "Product", "User bị seller chặn không xem được listing seller",
            "Cần tk_chan_seed giữa user A và seller B.",
            _ => Req(HttpMethod.Post, "/api/get_user_listings", Obj()),
            KhongCoQuyen,
            lyDoBoQua: "Cần dữ liệu tk_chan_seed có seller chặn user xem listing; chưa đủ điều kiện tự động hóa.");
    }
}
