using KiemThuApiShop.Core;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemAddProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-ADD-01", "Product", "Tạo sản phẩm hợp lệ có image_urls",
            "Token seller hợp lệ, category/brand/address lấy từ seed.",
            async ctx => await TaoYeuCauThemSanPhamHopLeAsync(ctx, "add_image", dungVideo: false, boBrand: false, nhieuVariant: false),
            Ok,
            DataCoTruong("id"),
            async (response, request, ctx) => await LuuSanPhamSeedAsync(response, request, ctx, "add_image"));

        Them(ds, "PRODUCT-ADD-02", "Product", "Tạo sản phẩm hợp lệ có videos",
            "Có videos và không có image_urls.",
            async ctx => await TaoYeuCauThemSanPhamHopLeAsync(ctx, "add_video", dungVideo: true, boBrand: false, nhieuVariant: false),
            Ok,
            DataCoTruong("id"),
            async (response, request, ctx) => await LuuSanPhamSeedAsync(response, request, ctx, "add_video"));

        Them(ds, "PRODUCT-ADD-03", "Product", "Tạo sản phẩm không gửi brand_id",
            "Gửi đủ trường bắt buộc, bỏ qua brand_id.",
            async ctx => await TaoYeuCauThemSanPhamHopLeAsync(ctx, "add_no_brand", dungVideo: false, boBrand: true, nhieuVariant: false),
            Tap("1000", "1004"),
            sauKhiDat: async (response, request, ctx) =>
            {
                if (response.MaSoSanh == "1000")
                {
                    await LuuSanPhamSeedAsync(response, request, ctx, "add_no_brand");
                }
            });

        Them(ds, "PRODUCT-ADD-04", "Product", "Tạo sản phẩm có nhiều variants",
            "Gửi nhiều biến thể size/color/stock/weight.",
            async ctx => await TaoYeuCauThemSanPhamHopLeAsync(ctx, "add_variants", dungVideo: false, boBrand: false, nhieuVariant: true),
            Ok,
            DataCoTruong("id"),
            async (response, request, ctx) => await LuuSanPhamSeedAsync(response, request, ctx, "add_variants"));

        Them(ds, "PRODUCT-ADD-05", "Product", "Tạo sản phẩm không gửi token",
            "Không kèm Authorization.",
            async ctx =>
            {
                var req = await TaoYeuCauThemSanPhamHopLeAsync(ctx, "add_no_token", false, false, false);
                return new YeuCauApi(HttpMethod.Post, "/api/add_product", req.Body);
            },
            SaiToken);

        Them(ds, "PRODUCT-ADD-06", "Product", "Tạo sản phẩm với token không hợp lệ",
            "Token sai định dạng/hết hạn.",
            async ctx =>
            {
                var req = await TaoYeuCauThemSanPhamHopLeAsync(ctx, "add_bad_token", false, false, false);
                return new YeuCauApi(HttpMethod.Post, "/api/add_product", req.Body, ctx.TokenSaiDinhDang);
            },
            SaiToken);

        ThemAddProductValidation(ds, "PRODUCT-ADD-07", "Thiếu title", body => body.Remove("title"), ThieuThamSo);
        ThemAddProductValidation(ds, "PRODUCT-ADD-08", "title rỗng", body => body["title"] = "   ", SaiGiaTri);
        ThemAddProductValidation(ds, "PRODUCT-ADD-09", "title quá 255 ký tự", body => body["title"] = new string('a', 256), SaiGiaTri);
        ThemAddProductValidation(ds, "PRODUCT-ADD-10", "Thiếu price", body => body.Remove("price"), ThieuThamSo);
        ThemAddProductValidation(ds, "PRODUCT-ADD-11", "price âm", body => body["price"] = -1, SaiGiaTri);
        ThemAddProductValidation(ds, "PRODUCT-ADD-12", "price sai kiểu", body => body["price"] = "abc", SaiKieu);
        ThemAddProductValidation(ds, "PRODUCT-ADD-13", "price = 0", body => body["price"] = 0, Tap("1000", "1004"));
        ThemAddProductValidation(ds, "PRODUCT-ADD-14", "Thiếu description", body => body.Remove("description"), ThieuThamSo);
        ThemAddProductValidation(ds, "PRODUCT-ADD-15", "description rỗng", body => body["description"] = "   ", SaiGiaTri);
        ThemAddProductValidation(ds, "PRODUCT-ADD-16", "Thiếu category_id", body => body.Remove("category_id"), ThieuThamSo);
        ThemAddProductValidation(ds, "PRODUCT-ADD-17", "category_id không tồn tại", body => body["category_id"] = 999999, Tap("1004", "9994"));
        ThemAddProductValidation(ds, "PRODUCT-ADD-18", "brand_id không tồn tại", body => body["brand_id"] = 999999, Tap("1004", "9994"));
        ThemAddProductValidation(ds, "PRODUCT-ADD-19", "Thiếu ship_from_id", body => body.Remove("ship_from_id"), ThieuThamSo);
        ThemAddProductValidation(ds, "PRODUCT-ADD-20", "ship_from_id không tồn tại", body => body["ship_from_id"] = 999999, Tap("1004", "9994"));

        Them(ds, "PRODUCT-ADD-21", "Product", "ship_from_id thuộc tài khoản khác",
            "Token user A nhưng dùng địa chỉ của user B.",
            async ctx =>
            {
                var seller = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var diaChiKhac = ctx.KhoSeed.LayDiaChiKhacTaiKhoan(seller.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Cần địa chỉ của tài khoản khác để kiểm thử ship_from_id không thuộc seller.");
                var dm = ctx.YeuCauDanhMucBatKy();
                var body = NguCanhKiemThu.TaoBodySanPhamHopLe("Sản phẩm sai địa chỉ", dm, ctx.LayThuongHieuNeuCo(), diaChiKhac);
                return new YeuCauApi(HttpMethod.Post, "/api/add_product", body, await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            KhongCoQuyen);

        ThemAddProductValidation(ds, "PRODUCT-ADD-22", "Gửi đồng thời image_urls và videos", body =>
        {
            body["videos"] = new[] { Obj(("url", "https://example.com/video.mp4")) };
        }, Tap("1004", "1008"));

        ThemAddProductValidation(ds, "PRODUCT-ADD-23", "Gửi quá 4 ảnh", body =>
        {
            body["image_urls"] = new[]
            {
                "https://example.com/1.jpg",
                "https://example.com/2.jpg",
                "https://example.com/3.jpg",
                "https://example.com/4.jpg",
                "https://example.com/5.jpg"
            };
        }, Tap("1004", "1008"));

        ThemAddProductValidation(ds, "PRODUCT-ADD-24", "Gửi ảnh trùng nhau", body =>
        {
            body["image_urls"] = new[]
            {
                "https://example.com/trung.jpg",
                "https://example.com/trung.jpg"
            };
        }, SaiGiaTri);

        ThemAddProductValidation(ds, "PRODUCT-ADD-25", "Variant stock âm", body =>
        {
            body["variants"] = new[] { Obj(("size", "M"), ("stock", -1), ("color", "Đen"), ("weight", 1)) };
        }, SaiGiaTri);

        ThemAddProductValidation(ds, "PRODUCT-ADD-26", "Variant weight âm", body =>
        {
            body["variants"] = new[] { Obj(("size", "M"), ("stock", 1), ("color", "Đen"), ("weight", -1)) };
        }, SaiGiaTri);

        ThemAddProductValidation(ds, "PRODUCT-ADD-27", "Variant sai kiểu dữ liệu", body =>
        {
            body["variants"] = new[] { Obj(("size", "M"), ("stock", "abc"), ("color", "Đen"), ("weight", "abc")) };
        }, SaiKieu);
    }

    private static void ThemUpdateProduct(List<KichBanApi> ds)
    {
        ThemUpdateOk(ds, "PRODUCT-UPDATE-01", "Cập nhật title sản phẩm", Obj(("title", $"Tên update {DateTimeOffset.Now:HHmmss}")));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-02", "Cập nhật price và description", Obj(("price", 180000), ("description", "Mô tả update từ test")));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-03", "Cập nhật price_discount hợp lệ", Obj(("price_discount", 10000)));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-04", "Thêm image_urls mới", Obj(("image_urls", new[] { "https://example.com/update-new.jpg" })));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-05", "Xóa image_urls_del", Obj(("image_urls_del", new[] { "https://example.com/image-test-1.jpg" })), Tap("1000", "1004"));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-06", "Cập nhật videos", Obj(("videos", new[] { Obj(("url", "https://example.com/video-update.mp4")) })));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-07", "Cập nhật variant cũ", Obj(("variants", new[] { Obj(("id", 1), ("size", "M"), ("stock", 2), ("color", "Xanh"), ("weight", 1)) })), Tap("1000", "1004"));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-08", "Thêm variant mới", Obj(("variants", new[] { Obj(("size", "XL"), ("stock", 2), ("color", "Đỏ"), ("weight", 1)) })));
        ThemUpdateOk(ds, "PRODUCT-UPDATE-09", "Cập nhật stock = 0", Obj(("variants", new[] { Obj(("size", "M"), ("stock", 0), ("color", "Đen"), ("weight", 1)) })));

        Them(ds, "PRODUCT-UPDATE-10", "Product", "Update không gửi token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamTheoLoai("cho_update");
                return Req(HttpMethod.Patch, $"/api/update/{sp.SpId}", Obj(("title", "Không token")));
            },
            SaiToken);

        Them(ds, "PRODUCT-UPDATE-11", "Product", "Update với token không hợp lệ",
            "Token sai định dạng.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamTheoLoai("cho_update");
                return Req(HttpMethod.Patch, $"/api/update/{sp.SpId}", Obj(("title", "Token sai")), ctx.TokenSaiDinhDang);
            },
            SaiToken);

        Them(ds, "PRODUCT-UPDATE-12", "Product", "User không phải seller update sản phẩm",
            "Dùng token user khác seller.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamTheoLoai("cho_update");
                var userKhac = ctx.YeuCauTaiKhoanKhacSeller(sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SpId}", Obj(("title", "Không phải seller")), await ctx.YeuCauTokenCuaTaiKhoanAsync(userKhac));
            },
            KhongCoQuyen);

        Them(ds, "PRODUCT-UPDATE-13", "Product", "Update product id không tồn tại",
            "Path /api/update/999999.",
            async ctx => new YeuCauApi(HttpMethod.Patch, "/api/update/999999", Obj(("title", "Không tồn tại")), await ctx.YeuCauTokenHopLeAsync()),
            KhongCoSanPham);

        Them(ds, "PRODUCT-UPDATE-14", "Product", "Update product id sai kiểu",
            "Path /api/update/abc.",
            async ctx => new YeuCauApi(HttpMethod.Patch, "/api/update/abc", Obj(("title", "Sai kiểu")), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        ThemUpdateValidation(ds, "PRODUCT-UPDATE-15", "Body rỗng", new Dictionary<string, object?>(), Tap("1000", "1002", "1004"));
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-16", "title rỗng", Obj(("title", "   ")), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-17", "title quá 255 ký tự", Obj(("title", new string('a', 256))), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-18", "price âm", Obj(("price", -1)), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-19", "price sai kiểu", Obj(("price", "abc")), SaiKieu);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-20", "price_discount âm", Obj(("price_discount", -1)), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-21", "category_id không tồn tại", Obj(("category_id", 999999)), Tap("1004", "9994"));
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-22", "brand_id không tồn tại", Obj(("brand_id", 999999)), Tap("1004", "9994"));
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-23", "ship_from_id không tồn tại", Obj(("ship_from_id", 999999)), Tap("1004", "9994"));
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-24", "ship_from_id thuộc tài khoản khác", Obj(("ship_from_id", 999999)), KhongCoQuyen);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-25", "Gửi đồng thời image_urls và videos", Obj(("image_urls", new[] { "https://example.com/a.jpg" }), ("videos", new[] { Obj(("url", "https://example.com/a.mp4")) })), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-26", "Gửi quá 4 ảnh", Obj(("image_urls", new[] { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg" })), Tap("1004", "1008"));
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-27", "Gửi ảnh trùng", Obj(("image_urls", new[] { "same.jpg", "same.jpg" })), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-28", "Variant stock âm", Obj(("variants", new[] { Obj(("size", "M"), ("stock", -1), ("color", "Đen"), ("weight", 1)) })), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-29", "Variant weight âm", Obj(("variants", new[] { Obj(("size", "M"), ("stock", 1), ("color", "Đen"), ("weight", -1)) })), SaiGiaTri);
        ThemUpdateValidation(ds, "PRODUCT-UPDATE-30", "Variant id không thuộc sản phẩm", Obj(("variants", new[] { Obj(("id", 999999), ("size", "M"), ("stock", 1), ("color", "Đen"), ("weight", 1)) })), SaiGiaTri);
    }

    private static void ThemDeleteProduct(List<KichBanApi> ds)
    {
        Them(ds, "PRODUCT-DELETE-01", "Product", "Seller xóa sản phẩm tồn tại",
            "Dùng product loại cho_delete.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamTheoLoai("cho_delete");
                var seller = ctx.YeuCauSellerCuaSanPham(sp);
                var req = new YeuCauApi(HttpMethod.Delete, $"/api/delete/{sp.SpId}", token: await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
                req.Tam["sanPham"] = sp;
                return req;
            },
            Ok,
            sauKhiDat: async (_, request, ctx) =>
            {
                var sp = (SanPhamSeed)request.Tam["sanPham"]!;
                sp.TrangThai = "da_xoa";
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "PRODUCT-DELETE-02", "Product", "Delete không gửi token",
            "Không kèm Authorization.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Delete, $"/api/delete/{sp.SpId}");
            },
            SaiToken);

        Them(ds, "PRODUCT-DELETE-03", "Product", "Delete với token không hợp lệ",
            "Token sai định dạng.",
            ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                return Req(HttpMethod.Delete, $"/api/delete/{sp.SpId}", token: ctx.TokenSaiDinhDang);
            },
            SaiToken);

        Them(ds, "PRODUCT-DELETE-04", "Product", "User không phải seller xóa sản phẩm",
            "Dùng token user khác seller.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamBatKy();
                var userKhac = ctx.YeuCauTaiKhoanKhacSeller(sp);
                return new YeuCauApi(HttpMethod.Delete, $"/api/delete/{sp.SpId}", token: await ctx.YeuCauTokenCuaTaiKhoanAsync(userKhac));
            },
            KhongCoQuyen);

        Them(ds, "PRODUCT-DELETE-05", "Product", "Delete product id không tồn tại",
            "Path /api/delete/999999.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/999999", token: await ctx.YeuCauTokenHopLeAsync()),
            KhongCoSanPham);

        Them(ds, "PRODUCT-DELETE-06", "Product", "Delete product id sai kiểu",
            "Path /api/delete/abc.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/abc", token: await ctx.YeuCauTokenHopLeAsync()),
            SaiKieu);

        Them(ds, "PRODUCT-DELETE-07", "Product", "Delete product id không hợp lệ",
            "Path /api/delete/0.",
            async ctx => new YeuCauApi(HttpMethod.Delete, "/api/delete/0", token: await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);

        Them(ds, "PRODUCT-DELETE-08", "Product", "Delete sản phẩm đã phát sinh đơn hàng",
            "Cần dữ liệu donhang_sanpham_seed liên kết product.",
            _ => Req(HttpMethod.Delete, "/api/delete/0"),
            Tap("1000", "1004", "1009", "1011"),
            lyDoBoQua: "Cần seed đơn hàng đã phát sinh cho product; chưa đủ dữ liệu trong phạm vi Auth/User/Product.");
    }

    private static async Task<YeuCauApi> TaoYeuCauThemSanPhamHopLeAsync(NguCanhKiemThu ctx, string loai, bool dungVideo, bool boBrand, bool nhieuVariant)
    {
        var seller = await ctx.YeuCauTaiKhoanDaDangKyAsync();
        var diaChi = ctx.YeuCauDiaChiCuaTaiKhoan(seller);
        var danhMuc = ctx.YeuCauDanhMucBatKy();
        var thuongHieu = boBrand ? null : ctx.LayThuongHieuNeuCo();
        var body = NguCanhKiemThu.TaoBodySanPhamHopLe($"Sản phẩm {loai} {DateTimeOffset.Now:HHmmss}", danhMuc, thuongHieu, diaChi, dungVideo, nhieuVariant);
        var req = new YeuCauApi(HttpMethod.Post, "/api/add_product", body, await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
        req.Tam["seller"] = seller;
        req.Tam["danhMuc"] = danhMuc;
        req.Tam["thuongHieu"] = thuongHieu;
        req.Tam["diaChi"] = diaChi;
        req.Tam["tenSp"] = body["title"];
        return req;
    }

    private static void ThemAddProductValidation(List<KichBanApi> ds, string ma, string ten, Action<Dictionary<string, object?>> suaBody, IReadOnlySet<string> expected)
    {
        Them(ds, ma, "Product", ten,
            $"Tạo sản phẩm invalid: {ten}.",
            async ctx =>
            {
                var req = await TaoYeuCauThemSanPhamHopLeAsync(ctx, ma, false, false, false);
                var body = (Dictionary<string, object?>)req.Body!;
                suaBody(body);
                return req;
            },
            expected);
    }

    private static void ThemUpdateOk(List<KichBanApi> ds, string ma, string ten, Dictionary<string, object?> body, IReadOnlySet<string>? expected = null)
    {
        Them(ds, ma, "Product", ten,
            $"Update sản phẩm hợp lệ: {ten}.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamTheoLoai("cho_update");
                var seller = ctx.YeuCauSellerCuaSanPham(sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SpId}", body, await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            expected ?? Ok);
    }

    private static void ThemUpdateValidation(List<KichBanApi> ds, string ma, string ten, Dictionary<string, object?> body, IReadOnlySet<string> expected)
    {
        Them(ds, ma, "Product", ten,
            $"Update sản phẩm invalid: {ten}.",
            async ctx =>
            {
                var sp = ctx.YeuCauSanPhamTheoLoai("cho_update");
                var seller = ctx.YeuCauSellerCuaSanPham(sp);
                return new YeuCauApi(HttpMethod.Patch, $"/api/update/{sp.SpId}", body, await ctx.YeuCauTokenCuaTaiKhoanAsync(seller));
            },
            expected);
    }

    private static async Task LuuSanPhamSeedAsync(PhanHoiApi response, YeuCauApi request, NguCanhKiemThu ctx, string loaiSeed)
    {
        var id = TienIchJson.DocChuoi(response.Data, "id", "product_id", "sp_id");
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var seller = (TaiKhoanSeed)request.Tam["seller"]!;
        var danhMuc = (DanhMucSeed)request.Tam["danhMuc"]!;
        var thuongHieu = (ThuongHieuSeed?)request.Tam["thuongHieu"];
        var diaChi = (DiaChiTaiKhoanSeed)request.Tam["diaChi"]!;
        var tenSp = request.Tam["tenSp"]?.ToString() ?? $"Sản phẩm {loaiSeed}";

        ctx.KhoSeed.DuLieu.SanPhamSeed.Add(new SanPhamSeed
        {
            SpSeedId = ctx.KhoSeed.DuLieu.SanPhamSeed.Count + 1,
            SpId = id,
            SellerTkSeedId = seller.TkSeedId,
            SellerTkId = seller.TkId,
            DmId = danhMuc.DmId,
            ThuongHieuId = thuongHieu?.ThuongHieuId,
            DiaChiId = diaChi.DiaChiId,
            TenSp = tenSp,
            Gia = 150000,
            TrangThai = "san_sang",
            LoaiSeed = loaiSeed,
            TaoBoiTest = true,
            XacMinhLuc = DateTimeOffset.Now
        });

        await ctx.KhoSeed.LuuAsync();
    }
}
