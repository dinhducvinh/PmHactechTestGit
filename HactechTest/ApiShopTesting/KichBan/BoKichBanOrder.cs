using System.Globalization;
using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> OrderSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> OrderSanPhamKhongTonTai = Tap("9992");

    private static void ThemKichBanOrder(List<KichBanApi> ds)
    {
        ThemTinhThanhPhuongXa(ds);
        ThemAddOrderAddress(ds);
        ThemCart(ds);
        ThemDonHang(ds);
    }

    private static void ThemTinhThanhPhuongXa(List<KichBanApi> ds)
    {
        Them(ds, "ORDER-PROVINCES-GET-01", "Order", "Lấy danh sách tỉnh/thành phố",
            "Token hợp lệ, không truyền query/body.",
            async ctx => new YeuCauApi(HttpMethod.Get, "/order/provinces", token: await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            KiemTraDanhSachTinhThanh());

        Them(ds, "ORDER-PROVINCES-GET-02", "Order", "Lấy tỉnh/thành phố không gửi token",
            "Không gửi Authorization hoặc token không hợp lệ.",
            ctx => Req(HttpMethod.Get, "/order/provinces"),
            OrderSaiToken);

        Them(ds, "ORDER-WARDS-GET-01", "Order", "Lấy danh sách phường/xã theo tỉnh",
            "Token hợp lệ, province_id lấy từ Provinces_seed có phường/xã tương ứng trong Wards_seed.",
            async ctx =>
            {
                var tinhThanh = LayTinhThanhCoPhuongXa(ctx);
                var req = new YeuCauApi(
                    HttpMethod.Get,
                    $"/order/wards?province_id={tinhThanh.TinhThanhId}",
                    token: await YeuCauTokenHopLeAsync(ctx));
                req.Tam["provinceId"] = tinhThanh.TinhThanhId;
                return req;
            },
            Ok,
            KiemTraDanhSachPhuongXaTheoTinh());

        Them(ds, "ORDER-WARDS-GET-02", "Order", "Lấy phường/xã thiếu province_id",
            "Token hợp lệ nhưng không truyền query province_id.",
            async ctx => new YeuCauApi(HttpMethod.Get, "/order/wards", token: await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-WARDS-GET-03", "Order", "Lấy phường/xã với province_id không tồn tại",
            "Token hợp lệ, province_id không có trong Provinces_seed/server.",
            async ctx => new YeuCauApi(
                HttpMethod.Get,
                $"/order/wards?province_id={LayProvinceIdKhongTonTai(ctx)}",
                token: await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-WARDS-GET-04", "Order", "Lấy phường/xã không gửi token",
            "province_id hợp lệ nhưng không gửi Authorization hoặc token không hợp lệ.",
            ctx =>
            {
                var tinhThanh = LayTinhThanhCoPhuongXa(ctx);
                return Req(HttpMethod.Get, $"/order/wards?province_id={tinhThanh.TinhThanhId}");
            },
            OrderSaiToken);
    }

    private static void ThemAddOrderAddress(List<KichBanApi> ds)
    {
        Them(ds, "ORDER-ADDR-ADD-01", "Order", "Thêm địa chỉ nhận hàng với token không hợp lệ",
            "Gọi POST /order/add_order_address bằng token sai định dạng/hết hạn, body hợp lệ.",
            ctx =>
            {
                var duLieu = TaoDuLieuThemDiaChi(ctx, null, "ORDER-ADDR-ADD-01");
                return Req(HttpMethod.Post, "/order/add_order_address", duLieu.Body, ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-ADDR-ADD-02", "Order", "Thêm địa chỉ nhận hàng thành công",
            "Token hợp lệ, address và address_id lấy từ Provinces_seed/Wards_seed.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-02");
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
                req.Tam["taiKhoan"] = taiKhoan;
                req.Tam["duLieuDiaChi"] = duLieu;
                return req;
            },
            Ok,
            KiemTraResponseDiaChiCoId(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuDiaChiVaoSeedSauKhiDatAsync(response, request));

        Them(ds, "ORDER-ADDR-ADD-03", "Order", "Thêm địa chỉ thiếu address_id",
            "Token hợp lệ, body có các trường địa chỉ khác nhưng bỏ address_id.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-03");
                duLieu.Body.Remove("address_id");
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            ThieuThamSo);

        Them(ds, "ORDER-ADDR-ADD-04", "Order", "Thêm địa chỉ với address_id sai kiểu",
            "Token hợp lệ, address_id không phải array [ward_id, province_id].",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-04");
                duLieu.Body["address_id"] = "1,2";
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiKieu);

        Them(ds, "ORDER-ADDR-ADD-05", "Order", "Thêm địa chỉ với ward/province không tồn tại",
            "Token hợp lệ, address_id là array nhưng ward_id/province_id không tồn tại hoặc không khớp quan hệ.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-05");
                duLieu.Body["address_id"] = new[] { 999999, 999999 };
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiGiaTri);

        Them(ds, "ORDER-ADDR-DELETE-01", "Order", "Xoa dia chi nhan hang voi token khong hop le",
            "Goi DELETE /order/delete/:id bang token sai dinh dang/het han, id dia chi hop le.",
            ctx =>
            {
                var cap = LayDiaChiTaiKhoanBatKy(ctx);
                return Req(
                    HttpMethod.Delete,
                    $"/order/delete/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    token: ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-ADDR-DELETE-02", "Order", "Xoa dia chi nhan hang khong mac dinh",
            "Token hop le, id la dia chi san_sang thuoc current user va is_default = false.",
            async ctx =>
            {
                var cap = await LayHoacTaoDiaChiKhongMacDinhDeXoaAsync(ctx);
                var req = new YeuCauApi(
                    HttpMethod.Delete,
                    $"/order/delete/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    token: await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
                req.Tam["diaChi"] = cap.DiaChi;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var diaChi = (DiaChiTaiKhoanSeed)request.Tam["diaChi"]!;
                await ctx.CapNhatDB.XoaDiaChiTaiKhoanAsync(diaChi);
            });

        Them(ds, "ORDER-ADDR-DELETE-03", "Order", "Khong xoa dia chi mac dinh",
            "Token hop le, id la dia chi san_sang thuoc current user nhung is_default = true.",
            async ctx =>
            {
                var cap = LayDiaChiMacDinh(ctx);
                return new YeuCauApi(
                    HttpMethod.Delete,
                    $"/order/delete/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    token: await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
            },
            SaiGiaTri);

        Them(ds, "ORDER-ADDR-DELETE-04", "Order", "Xoa dia chi khong ton tai",
            "Token hop le, path id khong ton tai tren server.",
            async ctx => new YeuCauApi(
                HttpMethod.Delete,
                "/order/delete/999999999",
                token: await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static async Task<CapDiaChiTaiKhoan> LayHoacTaoDiaChiKhongMacDinhDeXoaAsync(NguCanhKiemThu ctx)
    {
        try
        {
            return LayDiaChiKhongMacDinhCoTheXoa(ctx);
        }
        catch (BoQuaKiemThuException)
        {
            var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
            var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-DELETE-02", laMacDinh: false);
            var token = await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan);
            var request = new YeuCauApi(
                HttpMethod.Post,
                "/order/add_order_address",
                duLieu.Body,
                token);
            request.Tam["taiKhoan"] = taiKhoan;
            request.Tam["duLieuDiaChi"] = duLieu;

            var response = await ctx.Api.GuiAsync(request);
            if (!LaMaThanhCong(response))
            {
                throw new BoQuaKiemThuException(
                    $"Khong tao duoc dia chi non-default de chay ORDER-ADDR-DELETE-02. API tra code {response.MaSoSanh}.");
            }

            await ctx.CapNhatDB.LuuDiaChiVaoSeedSauKhiDatAsync(response, request);
            var diaChiIdServer = DocIdSau(response.Data, "id");
            var diaChi = ctx.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(x =>
                x.DiaChiIdServer == diaChiIdServer &&
                x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer)
                ?? throw new BoQuaKiemThuException("Da tao dia chi non-default nhung khong tim thay trong diachi_tk_seed.");

            return new CapDiaChiTaiKhoan(diaChi, taiKhoan);
        }
    }

    private static void ThemCart(List<KichBanApi> ds)
    {
        Them(ds, "ORDER-CART-GET-01", "Order", "Lấy giỏ hàng có dữ liệu",
            "Token buyer hợp lệ, buyer có cart item trong giohang_seed.",
            async ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                return new YeuCauApi(HttpMethod.Get, "/order/get_cart", token: await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            Ok,
            DataLaMang());

        Them(ds, "ORDER-CART-GET-02", "Order", "Lấy giỏ hàng rỗng",
            "Token buyer hợp lệ, buyer không có dòng nào trong giohang_seed.",
            async ctx =>
            {
                var buyer = ChonTaiKhoanKhongCoGioHang(ctx);
                return new YeuCauApi(HttpMethod.Get, "/order/get_cart", token: await LayTokenCuaTaiKhoanAsync(ctx, buyer));
            },
            Ok,
            DataLaMang());

        Them(ds, "ORDER-CART-GET-03", "Order", "Lấy giỏ hàng token sai",
            "Không gửi token hợp lệ.",
            ctx => Req(HttpMethod.Get, "/order/get_cart", token: ctx.TokenSaiDinhDang),
            OrderSaiToken);

        Them(ds, "ORDER-CART-ADD-01", "Order", "Thêm sản phẩm mới vào giỏ",
            "Buyer/seller không bị block, sản phẩm chưa có trong giohang_seed của buyer.",
            async ctx =>
            {
                var (buyer, sanPham) = ChonCapTaiKhoanSanPhamChuaCoGioHang(ctx);
                const int soLuong = 1;
                var req = new YeuCauApi(HttpMethod.Post, "/order/add_cart",
                    Obj(("product_id", IdBatBuoc(sanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("quantity", soLuong)),
                    await LayTokenCuaTaiKhoanAsync(ctx, buyer));
                req.Tam["buyer"] = buyer;
                req.Tam["sanPham"] = sanPham;
                req.Tam["soLuong"] = soLuong;
                return req;
            },
            Ok,
            KiemTraResponseCartCoId(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuGioHangSauAddCartAsync(response, request));

        Them(ds, "ORDER-CART-ADD-02", "Order", "Thêm lại sản phẩm đã có trong giỏ",
            "Dùng một dòng giohang_seed đang dang_trong_gio để kiểm tra cộng dồn số lượng.",
            async ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                const int soLuongThem = 2;
                var req = new YeuCauApi(HttpMethod.Post, "/order/add_cart",
                    Obj(("product_id", IdBatBuoc(cap.GioHang.SanPhamIdServer, "giohang_seed.sp_id_server")), ("quantity", soLuongThem)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["buyer"] = cap.Buyer;
                req.Tam["sanPham"] = cap.SanPham;
                req.Tam["gioHang"] = cap.GioHang;
                req.Tam["soLuong"] = soLuongThem;
                return req;
            },
            Ok,
            KiemTraResponseCartCoId(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuGioHangSauAddCartAsync(response, request));

        Them(ds, "ORDER-CART-ADD-03", "Order", "Thêm giỏ hàng với product_id không tồn tại",
            "Token buyer hợp lệ, product_id chắc chắn không tồn tại.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/add_cart",
                Obj(("product_id", 999999999), ("quantity", 1)),
                await YeuCauTokenHopLeAsync(ctx)),
            OrderSanPhamKhongTonTai);

        Them(ds, "ORDER-CART-ADD-04", "Order", "Thêm giỏ hàng sai quantity",
            "quantity = 0.",
            async ctx =>
            {
                var sanPham = LaySanPhamSanSang(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/add_cart",
                    Obj(("product_id", IdBatBuoc(sanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("quantity", 0)),
                    await YeuCauTokenHopLeAsync(ctx));
            },
            SaiGiaTri);

        Them(ds, "ORDER-CART-ADD-05", "Order", "Thêm giỏ hàng token sai",
            "Token sai định dạng, body hợp lệ.",
            ctx =>
            {
                var sanPham = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/order/add_cart",
                    Obj(("product_id", IdBatBuoc(sanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("quantity", 1)),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-CART-EDIT-01", "Order", "Cập nhật số lượng cart item",
            "Dùng cart_item_id_server thuộc đúng buyer trong giohang_seed.",
            async ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                const int soLuongMoi = 3;
                var req = new YeuCauApi(HttpMethod.Post, "/order/edit_cart",
                    Obj(("cart_item_id", IdBatBuoc(cap.GioHang.CartItemIdServer, "giohang_seed.cart_item_id_server")), ("quantity", soLuongMoi)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["gioHang"] = cap.GioHang;
                req.Tam["soLuongMoi"] = soLuongMoi;
                return req;
            },
            Ok,
            KiemTraResponseCartCoIdVaSoLuong("soLuongMoi"),
            async (_, request, ctx) =>
            {
                var gioHang = (GioHangSeed)request.Tam["gioHang"]!;
                var soLuongMoi = (int)request.Tam["soLuongMoi"]!;
                await ctx.CapNhatDB.CapNhatSoLuongGioHangAsync(gioHang, soLuongMoi);
            });

        Them(ds, "ORDER-CART-EDIT-02", "Order", "Cập nhật cart item sai quantity",
            "quantity = 0.",
            async ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/edit_cart",
                    Obj(("cart_item_id", IdBatBuoc(cap.GioHang.CartItemIdServer, "giohang_seed.cart_item_id_server")), ("quantity", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-CART-EDIT-03", "Order", "Cập nhật cart item không tồn tại",
            "cart_item_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/edit_cart",
                Obj(("cart_item_id", 999999999), ("quantity", 2)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-CART-EDIT-04", "Order", "Cập nhật cart item token sai",
            "Token sai định dạng, body dùng cart_item_id hợp lệ.",
            ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                return Req(HttpMethod.Post, "/order/edit_cart",
                    Obj(("cart_item_id", IdBatBuoc(cap.GioHang.CartItemIdServer, "giohang_seed.cart_item_id_server")), ("quantity", 2)),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-CART-DELETE-01", "Order", "Xóa cart item hợp lệ",
            "Dùng cart_item_id_server thuộc đúng buyer trong giohang_seed.",
            async ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/order/delete_cart",
                    Obj(("cart_item_id", IdBatBuoc(cap.GioHang.CartItemIdServer, "giohang_seed.cart_item_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["gioHang"] = cap.GioHang;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) =>
            {
                var gioHang = (GioHangSeed)request.Tam["gioHang"]!;
                await ctx.CapNhatDB.XoaGioHangAsync(gioHang);
            });

        Them(ds, "ORDER-CART-DELETE-02", "Order", "Xóa cart item không tồn tại",
            "cart_item_id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/delete_cart",
                Obj(("cart_item_id", 999999999)),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-CART-DELETE-03", "Order", "Xóa cart item token sai",
            "Token sai định dạng, body dùng cart_item_id hợp lệ.",
            ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                return Req(HttpMethod.Post, "/order/delete_cart",
                    Obj(("cart_item_id", IdBatBuoc(cap.GioHang.CartItemIdServer, "giohang_seed.cart_item_id_server"))),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);
    }

    private static void ThemDonHang(List<KichBanApi> ds)
    {
        Them(ds, "ORDER-CREATE-01", "Order", "Tạo đơn hàng trực tiếp thành công",
            "Buyer có địa chỉ, sản phẩm san_sang của seller khác và hai user không bị block.",
            async ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                const int soLuong = 1;
                const int orderSource = 1;
                var itemsSeed = new[]
                {
                    new DonHangSanPhamSeed
                    {
                        SanPhamIdServer = cap.SanPham.SanPhamIdServer,
                        SoLuong = soLuong,
                        DonGia = cap.SanPham.Gia,
                        ThanhTien = cap.SanPham.Gia * soLuong
                    }
                };
                var body = Obj(
                    ("items", new[]
                    {
                        Obj(
                            ("product_id", IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")),
                            ("quantity", soLuong))
                    }),
                    ("address_id", IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
                    ("order_source", orderSource));
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/order/create_order",
                    body,
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["buyer"] = cap.Buyer;
                req.Tam["seller"] = cap.Seller;
                req.Tam["diaChi"] = cap.DiaChi;
                req.Tam["itemsSeed"] = itemsSeed;
                req.Tam["orderSource"] = orderSource;
                return req;
            },
            Ok,
            KiemTraResponseCreateOrderCoId(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuDonHangSauCreateOrderAsync(response, request));

        Them(ds, "ORDER-CREATE-02", "Order", "Tạo đơn hàng token sai",
            "Body hợp lệ nhưng Authorization sai.",
            ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return Req(HttpMethod.Post, "/order/create_order",
                    Obj(
                        ("items", new[]
                        {
                            Obj(
                                ("product_id", IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")),
                                ("quantity", 1))
                        }),
                        ("address_id", IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
                        ("order_source", 1)),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-CREATE-03", "Order", "Tạo đơn hàng với product_id không tồn tại",
            "Token buyer hợp lệ, address_id hợp lệ, product_id = 999999999.",
            async ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/create_order",
                    Obj(
                        ("items", new[] { Obj(("product_id", 999999999), ("quantity", 1)) }),
                        ("address_id", IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
                        ("order_source", 1)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            OrderSanPhamKhongTonTai);

        Them(ds, "ORDER-CREATE-04", "Order", "Tạo đơn hàng sai quantity",
            "quantity = 0.",
            async ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/create_order",
                    Obj(
                        ("items", new[]
                        {
                            Obj(
                                ("product_id", IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")),
                                ("quantity", 0))
                        }),
                        ("address_id", IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
                        ("order_source", 1)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-LIST-01", "Order", "Lấy danh sách đơn mua của buyer",
            "Buyer có đơn hàng trong donhang_seed.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_list_purchases",
                    Obj(("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "ORDER-PURCHASE-LIST-02", "Order", "Lấy danh sách đơn mua count không hợp lệ",
            "count = 0.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_list_purchases",
                    Obj(("index", 0), ("count", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-SELLER-LIST-01", "Order", "Lấy danh sách đơn bán của seller",
            "Seller có đơn hàng trong donhang_seed.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_list_purchases_seller",
                    Obj(("index", 0), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
            },
            Ok,
            DataLaMang("id"));

        Them(ds, "ORDER-PURCHASE-GET-01", "Order", "Lấy chi tiết đơn mua",
            "Buyer lấy chi tiết đơn hàng của chính mình.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_purchase",
                    Obj(("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            Ok,
            DataCoTruong("id", "state", "items"));

        Them(ds, "ORDER-PURCHASE-GET-02", "Order", "Seller không được gọi get_purchase của buyer",
            "Dùng token seller để lấy đơn mua của buyer.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_purchase",
                    Obj(("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
            },
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-EDIT-01", "Order", "Sửa ghi chú đơn mua",
            "Buyer sửa note của đơn hàng pending/confirmed.",
            async ctx =>
            {
                var cap = LayDonHangCoTheSua(ctx);
                var note = $"note testcase {DateTimeOffset.Now:HHmmss}";
                var req = new YeuCauApi(HttpMethod.Post, "/order/edit_purchase",
                    Obj(
                        ("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("note", note)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                req.Tam["note"] = note;
                return req;
            },
            Ok,
            DataCoTruong("id", "state", "note"),
            async (_, request, ctx) =>
            {
                var donHang = (DonHangSeed)request.Tam["donHang"]!;
                var note = request.Tam["note"]?.ToString() ?? "";
                await ctx.CapNhatDB.CapNhatGhiChuDonHangAsync(donHang, "Sửa note bằng ORDER-PURCHASE-EDIT-01: " + note);
            });

        Them(ds, "ORDER-PURCHASE-EDIT-02", "Order", "Sửa đơn mua id không tồn tại",
            "id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/edit_purchase",
                Obj(("id", "999999999"), ("note", "note invalid")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseCreateOrderCoId()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var id = DocIdSau(response.Data, "order_id") ?? DocIdSau(response.Data, "id");
            if (id is not > 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response create_order thành công nhưng thiếu order_id/id."));
            }

            var state = response.Data?["status"]?.ToString() ?? response.Data?["state"]?.ToString();
            if (string.IsNullOrWhiteSpace(state))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response create_order thành công nhưng thiếu status/state."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachTinhThanh()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của /order/provinces không phải mảng."));
            }

            if (mang.Count == 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của /order/provinces đang rỗng, không đủ dữ liệu tỉnh/thành phố để chạy test địa chỉ."));
            }

            string? tenTruoc = null;
            foreach (var node in mang)
            {
                if (node is not JsonObject item)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, "Mỗi item tỉnh/thành phố phải là object."));
                }

                var id = DocIdSau(item, "id");
                var ten = DocChuoiTuNode(item["name"]);
                if (id is not > 0 || string.IsNullOrWhiteSpace(ten))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, "Mỗi item tỉnh/thành phố phải có id > 0 và name."));
                }

                if (tenTruoc is not null && SoSanhTenDiaChi(tenTruoc, ten) > 0)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"Danh sách tỉnh/thành phố chưa sort theo name tăng dần: `{tenTruoc}` đứng trước `{ten}`."));
                }

                tenTruoc = ten;
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachPhuongXaTheoTinh()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của /order/wards không phải mảng."));
            }

            if (!request.Tam.TryGetValue("provinceId", out var tamProvinceId) ||
                tamProvinceId is not int provinceId)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Thiếu province_id mong đợi để kiểm tra response /order/wards."));
            }

            if (mang.Count == 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data của /order/wards đang rỗng với province_id = {provinceId}."));
            }

            string? tenTruoc = null;
            foreach (var node in mang)
            {
                if (node is not JsonObject item)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, "Mỗi item phường/xã phải là object."));
                }

                var id = DocIdSau(item, "id");
                var ten = DocChuoiTuNode(item["name"]);
                var provinceIdThucTe = DocIdSau(item, "province_id") ?? DocIdSau(item, "provinces_id");
                if (id is not > 0 || string.IsNullOrWhiteSpace(ten) || provinceIdThucTe is not > 0)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, "Mỗi item phường/xã phải có id > 0, name và province_id."));
                }

                if (provinceIdThucTe.Value != provinceId)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data[].province_id phải bằng {provinceId}, thực tế là {provinceIdThucTe.Value}."));
                }

                if (tenTruoc is not null && SoSanhTenDiaChi(tenTruoc, ten) > 0)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"Danh sách phường/xã chưa sort theo name tăng dần: `{tenTruoc}` đứng trước `{ten}`."));
                }

                tenTruoc = ten;
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static TinhThanhSeed LayTinhThanhCoPhuongXa(NguCanhKiemThu ctx)
    {
        var tinhThanhCoPhuongXa = ctx.CapNhatDB.DuLieu.PhuongXaSeed
            .Where(x => x.TinhThanhId > 0)
            .Select(x => x.TinhThanhId)
            .ToHashSet();

        return ctx.CapNhatDB.DuLieu.TinhThanhSeed
            .Where(x => x.TinhThanhId > 0 && tinhThanhCoPhuongXa.Contains(x.TinhThanhId))
            .OrderBy(x => x.TinhThanhId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu cặp Provinces_seed/Wards_seed hợp lệ để gọi GET /order/wards. Hãy bấm Kiểm tra seed để đồng bộ lại dữ liệu địa chỉ.");
    }

    private static int LayProvinceIdKhongTonTai(NguCanhKiemThu ctx)
    {
        var idDaCo = ctx.CapNhatDB.DuLieu.TinhThanhSeed
            .Select(x => x.TinhThanhId)
            .ToHashSet();
        var id = 999999999;
        while (idDaCo.Contains(id))
        {
            id--;
        }

        return id;
    }

    private static int SoSanhTenDiaChi(string tenThuNhat, string tenThuHai)
    {
        return CultureInfo.GetCultureInfo("vi-VN")
            .CompareInfo
            .Compare(tenThuNhat, tenThuHai, CompareOptions.IgnoreCase);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseDiaChiCoId()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var id = DocIdSau(response.Data, "id");
            return Task.FromResult(id is not > 0
                ? new KetQuaKiemTraThem(false, "Response thêm địa chỉ thành công nhưng thiếu id/address_id.")
                : KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseCartCoId()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var id = DocIdSau(response.Data, "cart_item_id") ?? DocIdSau(response.Data, "id");
            return Task.FromResult(id is not > 0
                ? new KetQuaKiemTraThem(false, "Response cart thành công nhưng thiếu cart_item_id.")
                : KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseCartCoIdVaSoLuong(string tenTamSoLuong)
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var id = DocIdSau(response.Data, "cart_item_id") ?? DocIdSau(response.Data, "id");
            if (id is not > 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response cart thành công nhưng thiếu cart_item_id."));
            }

            var soLuongMongDoi = request.Tam.TryGetValue(tenTamSoLuong, out var tam) && tam is int soLuong
                ? soLuong
                : (int?)null;
            if (soLuongMongDoi is null)
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var soLuongThucTe = DocIntTuNode(response.Data?["quantity"]);
            return Task.FromResult(soLuongThucTe == soLuongMongDoi.Value
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data.quantity phải bằng {soLuongMongDoi.Value}, thực tế là {soLuongThucTe?.ToString() ?? "null"}."));
        };
    }
}
