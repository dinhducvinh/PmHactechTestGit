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
    private static readonly IReadOnlySet<string> OrderDaThucHienTruoc = Tap("1010");
    private static readonly IReadOnlySet<string> OrderLoiNghiepVuHoacGiaTri = Tap("1004", "1010", "1009", "1013");

    private static void ThemKichBanOrder(List<KichBanApi> ds)
    {
        ThemVanChuyen(ds);
        ThemTinhThanhPhuongXa(ds);
        ThemAddOrderAddress(ds);
        ThemAddressModule(ds);
        ThemCart(ds);
        ThemDonHang(ds);
    }

    private static void ThemVanChuyen(List<KichBanApi> ds)
    {
        Them(ds, "ORDER-SHIP-FROM-01", "Order", "Lấy khu vực gửi hàng token sai",
            "Gọi GET /order/get_ship_from bằng token sai, query hợp lệ.",
            ctx =>
            {
                var tinhThanh = LayTinhThanhCoPhuongXa(ctx);
                return Req(HttpMethod.Get, $"/order/get_ship_from?level=1&index=0&count=10&parent_id={tinhThanh.TinhThanhId}", token: ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-SHIP-FROM-02", "Order", "Lấy khu vực gửi hàng theo tỉnh",
            "Token hợp lệ, level = 1, parent_id lấy từ Provinces_seed.",
            async ctx =>
            {
                var tinhThanh = LayTinhThanhCoPhuongXa(ctx);
                return new YeuCauApi(
                    HttpMethod.Get,
                    $"/order/get_ship_from?level=1&index=0&count=10&parent_id={tinhThanh.TinhThanhId}",
                    token: await YeuCauTokenHopLeAsync(ctx));
            },
            Ok,
            DataLaMang());

        Them(ds, "ORDER-SHIP-FROM-03", "Order", "Lấy khu vực gửi hàng theo phường/xã",
            "Token hợp lệ, level = 0, parent_id lấy từ Wards_seed.",
            async ctx =>
            {
                var phuongXa = LayPhuongXaBatKy(ctx);
                return new YeuCauApi(
                    HttpMethod.Get,
                    $"/order/get_ship_from?level=0&index=0&count=10&parent_id={phuongXa.PhuongXaId}",
                    token: await YeuCauTokenHopLeAsync(ctx));
            },
            Ok,
            DataLaMang());

        Them(ds, "ORDER-SHIP-FROM-04", "Order", "Lấy khu vực gửi hàng parent_id không tồn tại",
            "Token hợp lệ, level = 1, parent_id không tồn tại.",
            async ctx => new YeuCauApi(
                HttpMethod.Get,
                $"/order/get_ship_from?level=1&index=0&count=10&parent_id={LayProvinceIdKhongTonTai(ctx)}",
                token: await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-SHIP-FROM-05", "Order", "Lấy khu vực gửi hàng thiếu query",
            "Token hợp lệ nhưng thiếu parent_id.",
            async ctx => new YeuCauApi(
                HttpMethod.Get,
                "/order/get_ship_from?level=1&index=0&count=10",
                token: await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "ORDER-SHIP-FROM-06", "Order", "Lấy khu vực gửi hàng sai kiểu query",
            "Token hợp lệ, index = abc.",
            async ctx =>
            {
                var tinhThanh = LayTinhThanhCoPhuongXa(ctx);
                return new YeuCauApi(
                    HttpMethod.Get,
                    $"/order/get_ship_from?level=1&index=abc&count=10&parent_id={tinhThanh.TinhThanhId}",
                    token: await YeuCauTokenHopLeAsync(ctx));
            },
            SaiKieu);

        Them(ds, "ORDER-SHIP-FROM-07", "Order", "Lấy khu vực gửi hàng sai giá trị query",
            "Token hợp lệ, parent_id = 0 và count = 0.",
            async ctx => new YeuCauApi(
                HttpMethod.Get,
                "/order/get_ship_from?level=1&index=0&count=0&parent_id=0",
                token: await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-SHIP-FEE-01", "Order", "Tính phí vận chuyển token sai",
            "Body có product_id/address_id hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return Req(HttpMethod.Post, "/order/get_ship_fee",
                    Obj(
                        ("product_id", IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")),
                        ("address_id", IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server"))),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-SHIP-FEE-02", "Order", "Tính phí vận chuyển hợp lệ",
            "Token buyer hợp lệ, product_id hợp lệ, address_id thuộc buyer.",
            async ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_ship_fee",
                    Obj(
                        ("product_id", IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")),
                        ("address_id", IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            Ok,
            DataCoTruong("ship_fee", "leatime"));

        Them(ds, "ORDER-SHIP-FEE-03", "Order", "Tính phí vận chuyển product_id không tồn tại",
            "Token buyer hợp lệ, product_id không tồn tại.",
            async ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_ship_fee",
                    Obj(
                        ("product_id", 999999999),
                        ("address_id", IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-SHIP-FEE-04", "Order", "Tính phí vận chuyển address_id không hợp lệ",
            "Token buyer hợp lệ, address_id không tồn tại.",
            async ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_ship_fee",
                    Obj(
                        ("product_id", IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")),
                        ("address_id", 999999999)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);
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

        Them(ds, "ORDER-ADDR-LIST-01", "Order", "Lấy danh sách địa chỉ token sai",
            "Gọi GET /order/get_list_order_address bằng token sai định dạng/hết hạn.",
            ctx => Req(HttpMethod.Get, "/order/get_list_order_address", token: ctx.TokenSaiDinhDang),
            OrderSaiToken);

        Them(ds, "ORDER-ADDR-LIST-02", "Order", "Lấy danh sách địa chỉ của current user",
            "Token hợp lệ của user có địa chỉ trong diachi_tk_seed.",
            async ctx =>
            {
                var cap = LayDiaChiTaiKhoanBatKy(ctx);
                return new YeuCauApi(HttpMethod.Get, "/order/get_list_order_address", token: await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
            },
            Ok,
            DataLaMang());

        Them(ds, "ORDER-ADDR-UPDATE-01", "Order", "Cập nhật địa chỉ token sai",
            "Gọi PATCH /order/update/:id bằng token sai, body hợp lệ.",
            ctx =>
            {
                var cap = LayDiaChiTaiKhoanBatKy(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, cap.TaiKhoan, "ORDER-ADDR-UPDATE-01", cap.DiaChi.LaMacDinh);
                return Req(
                    HttpMethod.Patch,
                    $"/order/update/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    duLieu.Body,
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-ADDR-UPDATE-02", "Order", "Cập nhật địa chỉ nhận hàng thành công",
            "Token hợp lệ, id là địa chỉ thuộc current user, body cập nhật hợp lệ.",
            async ctx =>
            {
                var cap = LayDiaChiTaiKhoanBatKy(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, cap.TaiKhoan, "ORDER-ADDR-UPDATE-02", cap.DiaChi.LaMacDinh);
                var req = new YeuCauApi(
                    HttpMethod.Patch,
                    $"/order/update/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
                req.Tam["diaChi"] = cap.DiaChi;
                req.Tam["duLieuDiaChi"] = duLieu;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) => await CapNhatDiaChiSeedSauUpdateAsync(request, ctx));

        Them(ds, "ORDER-ADDR-UPDATE-03", "Order", "Cập nhật địa chỉ không tồn tại",
            "Token hợp lệ, path id không tồn tại, body hợp lệ.",
            async ctx =>
            {
                var cap = LayDiaChiTaiKhoanBatKy(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, cap.TaiKhoan, "ORDER-ADDR-UPDATE-03", cap.DiaChi.LaMacDinh);
                return new YeuCauApi(
                    HttpMethod.Patch,
                    "/order/update/999999999",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
            },
            SaiGiaTri);

        Them(ds, "ORDER-ADDR-UPDATE-04", "Order", "Cập nhật địa chỉ với dữ liệu trùng",
            "Token hợp lệ, id là địa chỉ hiện có, address_id và address trùng dữ liệu cũ.",
            async ctx =>
            {
                var cap = LayDiaChiTaiKhoanBatKy(ctx);
                return new YeuCauApi(
                    HttpMethod.Patch,
                    $"/order/update/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    Obj(
                        ("address", cap.DiaChi.DiaChi),
                        ("address_id", new[] { cap.DiaChi.PhuongXaId, cap.DiaChi.TinhThanhId })),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
            },
            OrderDaThucHienTruoc);

        Them(ds, "ORDER-ADDR-DELETE-01", "Order", "Xóa địa chỉ nhận hàng với token không hợp lệ",
            "Gọi DELETE /order/delete/:id bằng token sai định dạng/hết hạn, id địa chỉ hợp lệ.",
            ctx =>
            {
                var cap = LayDiaChiTaiKhoanBatKy(ctx);
                return Req(
                    HttpMethod.Delete,
                    $"/order/delete/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    token: ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-ADDR-DELETE-02", "Order", "Xóa địa chỉ nhận hàng không mặc định",
            "Token hợp lệ, id là địa chỉ san_sang thuộc current user và is_default = false.",
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

        Them(ds, "ORDER-ADDR-DELETE-03", "Order", "Không xóa địa chỉ mặc định",
            "Token hợp lệ, id là địa chỉ san_sang thuộc current user nhưng is_default = true.",
            async ctx =>
            {
                var cap = LayDiaChiMacDinh(ctx);
                return new YeuCauApi(
                    HttpMethod.Delete,
                    $"/order/delete/{IdBatBuoc(cap.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")}",
                    token: await LayTokenCuaTaiKhoanAsync(ctx, cap.TaiKhoan));
            },
            SaiGiaTri);

        Them(ds, "ORDER-ADDR-DELETE-04", "Order", "Xóa địa chỉ không tồn tại",
            "Token hợp lệ, path id không tồn tại trên server.",
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
                    $"Không tạo được địa chỉ non-default để chạy ORDER-ADDR-DELETE-02. API trả code {response.MaSoSanh}.");
            }

            await ctx.CapNhatDB.LuuDiaChiVaoSeedSauKhiDatAsync(response, request);
            var diaChiIdServer = DocIdSau(response.Data, "id");
            var diaChi = ctx.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(x =>
                x.DiaChiIdServer == diaChiIdServer &&
                x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer)
                ?? throw new BoQuaKiemThuException("Đã tạo địa chỉ non-default nhưng không tìm thấy trong diachi_tk_seed.");

            return new CapDiaChiTaiKhoan(diaChi, taiKhoan);
        }
    }

    private static void ThemAddressModule(List<KichBanApi> ds)
    {
        Them(ds, "ADDRESS-CREATE-01", "Address", "Tạo address token sai",
            "Gọi POST /addresses/create bằng token sai định dạng/hết hạn, body hợp lệ.",
            ctx => Req(HttpMethod.Post, "/addresses/create", TaoBodyAddressModule(ctx, "ADDRESS-CREATE-01"), ctx.TokenSaiDinhDang),
            OrderSaiToken);

        Them(ds, "ADDRESS-CREATE-02", "Address", "Tạo address thành công",
            "Token hợp lệ, body có receiver_name, phone, full_address, is_default, ward_id, lat, lng.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/addresses/create",
                    TaoBodyAddressModule(ctx, "ADDRESS-CREATE-02"),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Ok,
            DataCoTruong("id"));

        Them(ds, "ADDRESS-CREATE-03", "Address", "Tạo address thiếu field bắt buộc",
            "Token hợp lệ, body thiếu full_address.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var body = TaoBodyAddressModule(ctx, "ADDRESS-CREATE-03");
                body.Remove("full_address");
                return new YeuCauApi(HttpMethod.Post, "/addresses/create", body, await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            ThieuThamSoHoacSaiGiaTri);

        Them(ds, "ADDRESS-CREATE-04", "Address", "Tạo address sai kiểu ward_id",
            "Token hợp lệ, ward_id = abc.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var body = TaoBodyAddressModule(ctx, "ADDRESS-CREATE-04");
                body["ward_id"] = "abc";
                return new YeuCauApi(HttpMethod.Post, "/addresses/create", body, await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiKieuHoacSaiGiaTri);

        Them(ds, "ADDRESS-ME-01", "Address", "Lấy address của current user token sai",
            "Gọi GET /addresses/me bằng token sai định dạng/hết hạn.",
            ctx => Req(HttpMethod.Get, "/addresses/me", token: ctx.TokenSaiDinhDang),
            OrderSaiToken);

        Them(ds, "ADDRESS-ME-02", "Address", "Lấy address của current user",
            "Token hợp lệ của user đang tồn tại.",
            async ctx => new YeuCauApi(HttpMethod.Get, "/addresses/me", token: await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            DataLaMang());
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
                var thanhTien = cap.SanPham.Gia * soLuong;
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
                req.Tam["sanPhamIdServer"] = IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server");
                req.Tam["soLuong"] = soLuong;
                req.Tam["donGia"] = cap.SanPham.Gia;
                req.Tam["thanhTien"] = thanhTien;
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

        Them(ds, "ORDER-CREATE-05", "Order", "Tạo đơn hàng address_id không hợp lệ",
            "Token buyer hợp lệ, sản phẩm hợp lệ nhưng address_id không tồn tại.",
            async ctx =>
            {
                var cap = ChonCapTaoDonHangTrucTiep(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/create_order",
                    Obj(
                        ("items", new[] { Obj(("product_id", IdBatBuoc(cap.SanPham.SanPhamIdServer, "sanpham_seed.sp_id_server")), ("quantity", 1)) }),
                        ("address_id", 999999999),
                        ("order_source", 1)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-CREATE-CART-01", "Order", "Tạo đơn từ giỏ hàng quá số lượng",
            "order_source = 0, quantity lớn hơn số lượng đang có trong giohang_seed.",
            async ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                var diaChi = LayDiaChiCuaTaiKhoan(ctx, cap.Buyer);
                var seller = LayTaiKhoanTheoServerId(ctx, cap.SanPham.TaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu seller của sản phẩm trong giohang_seed.");
                var soLuong = cap.GioHang.SoLuong + 1;
                var req = new YeuCauApi(HttpMethod.Post, "/order/create_order",
                    Obj(
                        ("items", new[] { Obj(("product_id", IdBatBuoc(cap.GioHang.SanPhamIdServer, "giohang_seed.sp_id_server")), ("quantity", soLuong)) }),
                        ("address_id", IdBatBuoc(diaChi.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
                        ("order_source", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["buyer"] = cap.Buyer;
                req.Tam["seller"] = seller;
                req.Tam["diaChi"] = diaChi.DiaChi;
                req.Tam["sanPhamIdServer"] = IdBatBuoc(cap.GioHang.SanPhamIdServer, "giohang_seed.sp_id_server");
                req.Tam["soLuong"] = soLuong;
                req.Tam["donGia"] = cap.SanPham.Gia;
                req.Tam["thanhTien"] = cap.SanPham.Gia * soLuong;
                req.Tam["orderSource"] = 0;
                req.Tam["gioHang"] = cap.GioHang;
                return req;
            },
            SaiGiaTri);

        Them(ds, "ORDER-CREATE-CART-02", "Order", "Tạo đơn từ giỏ hàng thành công",
            "order_source = 0, items lấy từ giohang_seed của buyer.",
            async ctx =>
            {
                var cap = LayGioHangDangTrongGio(ctx);
                var diaChi = LayDiaChiCuaTaiKhoan(ctx, cap.Buyer);
                var seller = LayTaiKhoanTheoServerId(ctx, cap.SanPham.TaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu seller của sản phẩm trong giohang_seed.");
                var soLuong = cap.GioHang.SoLuong;
                var thanhTien = cap.SanPham.Gia * soLuong;
                var req = new YeuCauApi(HttpMethod.Post, "/order/create_order",
                    Obj(
                        ("items", new[] { Obj(("product_id", IdBatBuoc(cap.GioHang.SanPhamIdServer, "giohang_seed.sp_id_server")), ("quantity", soLuong)) }),
                        ("address_id", IdBatBuoc(diaChi.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
                        ("order_source", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["buyer"] = cap.Buyer;
                req.Tam["seller"] = seller;
                req.Tam["diaChi"] = diaChi.DiaChi;
                req.Tam["sanPhamIdServer"] = IdBatBuoc(cap.GioHang.SanPhamIdServer, "giohang_seed.sp_id_server");
                req.Tam["soLuong"] = soLuong;
                req.Tam["donGia"] = cap.SanPham.Gia;
                req.Tam["thanhTien"] = thanhTien;
                req.Tam["orderSource"] = 0;
                req.Tam["gioHang"] = cap.GioHang;
                return req;
            },
            Ok,
            KiemTraResponseCreateOrderCoId(),
            async (response, request, ctx) =>
            {
                await ctx.CapNhatDB.LuuDonHangSauCreateOrderAsync(response, request);
                var gioHang = (GioHangSeed)request.Tam["gioHang"]!;
                await ctx.CapNhatDB.XoaGioHangAsync(gioHang);
            });

        Them(ds, "ORDER-STATUS-01", "Order", "Lấy trạng thái đơn hàng",
            "Buyer lấy trạng thái đơn hàng đang lưu trong donhang_seed.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/order/get_order_status",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            KiemTraTrangThaiDonHang());

        Them(ds, "ORDER-STATUS-02", "Order", "Lấy trạng thái đơn hàng token sai",
            "purchase_id hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return Req(HttpMethod.Post, "/order/get_order_status",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server"))),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-STATUS-03", "Order", "Lấy trạng thái đơn hàng thiếu purchase_id",
            "Token hợp lệ nhưng body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/get_order_status", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-ACCEPT-01", "Order", "Seller chấp nhận đơn pending",
            "Token seller hợp lệ, purchase_id là đơn pending, buyer_id đúng buyer.",
            async ctx =>
            {
                var cap = LayDonHangTheoTrangThai(ctx, "pending");
                var req = new YeuCauApi(HttpMethod.Post, "/order/set_accept_buyer",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("buyer_id", IdBatBuoc(cap.DonHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server").ToString()),
                        ("is_accept", 1)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) => await ctx.CapNhatDB.CapNhatDonHangSauSellerAcceptAsync(request, dongY: true));

        Them(ds, "ORDER-ACCEPT-02", "Order", "Seller từ chối đơn pending",
            "Token seller hợp lệ, purchase_id là đơn pending khác, is_accept = 0.",
            async ctx =>
            {
                var cap = LayDonHangTheoTrangThai(ctx, "pending");
                var req = new YeuCauApi(HttpMethod.Post, "/order/set_accept_buyer",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("buyer_id", IdBatBuoc(cap.DonHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server").ToString()),
                        ("is_accept", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) => await ctx.CapNhatDB.CapNhatDonHangSauSellerAcceptAsync(request, dongY: false));

        Them(ds, "ORDER-ACCEPT-03", "Order", "Seller chấp nhận đơn token sai",
            "Body hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return Req(HttpMethod.Post, "/order/set_accept_buyer",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("buyer_id", IdBatBuoc(cap.DonHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server").ToString()),
                        ("is_accept", 1)),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-ACCEPT-04", "Order", "Seller chấp nhận đơn không còn pending",
            "Token seller hợp lệ nhưng đơn có trạng thái khác pending.",
            async ctx =>
            {
                var cap = LayDonHangKhacTrangThai(ctx, "pending");
                return new YeuCauApi(HttpMethod.Post, "/order/set_accept_buyer",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("buyer_id", IdBatBuoc(cap.DonHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server").ToString()),
                        ("is_accept", 1)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
            },
            OrderDaThucHienTruoc);

        Them(ds, "ORDER-SHIPPED-01", "Order", "Seller đánh dấu shipped token sai",
            "Body hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangTheoTrangThai(ctx, "confirmed");
                return Req(HttpMethod.Post, "/order/seller_mark_as_shipped",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("buyer_id", IdBatBuoc(cap.DonHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server").ToString())),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-SHIPPED-02", "Order", "Seller đánh dấu đơn đã gửi",
            "Token seller hợp lệ, đơn đang confirmed.",
            async ctx =>
            {
                var cap = LayDonHangTheoTrangThai(ctx, "confirmed");
                var req = new YeuCauApi(HttpMethod.Post, "/order/seller_mark_as_shipped",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("buyer_id", IdBatBuoc(cap.DonHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) => await ctx.CapNhatDB.CapNhatDonHangSauSellerShippedAsync(request));

        Them(ds, "ORDER-SHIPPED-03", "Order", "Seller đánh dấu shipped sai trạng thái",
            "Token seller hợp lệ nhưng đơn không ở trạng thái confirmed.",
            async ctx =>
            {
                var cap = LayDonHangKhacTrangThai(ctx, "confirmed");
                return new YeuCauApi(HttpMethod.Post, "/order/seller_mark_as_shipped",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("buyer_id", IdBatBuoc(cap.DonHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
            },
            OrderLoiNghiepVuHoacGiaTri);

        Them(ds, "ORDER-SHIPPED-04", "Order", "Seller đánh dấu shipped thiếu buyer_id",
            "Token seller hợp lệ nhưng body thiếu buyer_id.",
            async ctx =>
            {
                var cap = LayDonHangTheoTrangThai(ctx, "confirmed");
                return new YeuCauApi(HttpMethod.Post, "/order/seller_mark_as_shipped",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
            },
            SaiGiaTri);

        Them(ds, "ORDER-RECEIVED-01", "Order", "Buyer xác nhận nhận hàng token sai",
            "Body hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangTheoTrangThai(ctx, "shipping");
                return Req(HttpMethod.Post, "/order/buyer_confirm_received",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-RECEIVED-02", "Order", "Buyer xác nhận đã nhận hàng",
            "Token buyer hợp lệ, đơn đang shipping.",
            async ctx =>
            {
                var cap = LayDonHangTheoTrangThai(ctx, "shipping");
                var req = new YeuCauApi(HttpMethod.Post, "/order/buyer_confirm_received",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) => await ctx.CapNhatDB.CapNhatDonHangSauBuyerReceivedAsync(request));

        Them(ds, "ORDER-RECEIVED-03", "Order", "Buyer xác nhận nhận hàng sai trạng thái",
            "Token buyer hợp lệ nhưng đơn không ở trạng thái shipping.",
            async ctx =>
            {
                var cap = LayDonHangKhacTrangThai(ctx, "shipping");
                return new YeuCauApi(HttpMethod.Post, "/order/buyer_confirm_received",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            OrderDaThucHienTruoc);

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

        Them(ds, "ORDER-PURCHASE-LIST-03", "Order", "Lấy danh sách đơn mua theo trạng thái",
            "Buyer lọc danh sách đơn mua theo state hiện có trong donhang_seed.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_list_purchases",
                    Obj(("index", 0), ("count", 10), ("state", cap.DonHang.TrangThai)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            Ok,
            DataLaMang());

        Them(ds, "ORDER-PURCHASE-LIST-04", "Order", "Lấy danh sách đơn mua token sai",
            "Body hợp lệ nhưng token sai.",
            ctx => Req(HttpMethod.Post, "/order/get_list_purchases", Obj(("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            OrderSaiToken);

        Them(ds, "ORDER-PURCHASE-LIST-05", "Order", "Lấy danh sách đơn mua thiếu count",
            "Token buyer hợp lệ nhưng thiếu count.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_list_purchases",
                    Obj(("index", 0)),
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

        Them(ds, "ORDER-PURCHASE-SELLER-LIST-02", "Order", "Lấy danh sách đơn bán theo trạng thái",
            "Seller lọc danh sách đơn bán theo state hiện có trong donhang_seed.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_list_purchases_seller",
                    Obj(("index", 0), ("count", 10), ("state", cap.DonHang.TrangThai)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
            },
            Ok,
            DataLaMang());

        Them(ds, "ORDER-PURCHASE-SELLER-LIST-03", "Order", "Lấy danh sách đơn bán token sai",
            "Body hợp lệ nhưng token sai.",
            ctx => Req(HttpMethod.Post, "/order/get_list_purchases_seller", Obj(("index", 0), ("count", 10)), ctx.TokenSaiDinhDang),
            OrderSaiToken);

        Them(ds, "ORDER-PURCHASE-SELLER-LIST-04", "Order", "Lấy danh sách đơn bán thiếu count",
            "Token seller hợp lệ nhưng thiếu count.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_list_purchases_seller",
                    Obj(("index", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Seller));
            },
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-GET-01", "Order", "Lấy chi tiết đơn mua",
            "Buyer lấy chi tiết đơn hàng của chính mình.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/order/get_purchase",
                    Obj(("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            KiemTraChiTietDonMua());

        Them(ds, "ORDER-PURCHASE-GET-02", "Order", "Lấy chi tiết đơn mua với token không hợp lệ",
            "id hợp lệ nhưng dùng token sai định dạng.",
            ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return Req(HttpMethod.Post, "/order/get_purchase",
                    Obj(("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-PURCHASE-GET-03", "Order", "Lấy chi tiết đơn mua không thuộc buyer",
            "Đăng nhập buyer A nhưng truyền id đơn mua thuộc buyer B.",
            async ctx =>
            {
                var (buyer, donHang) = LayBuyerVaDonHangKhongThuocBuyer(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/get_purchase",
                    Obj(("id", IdBatBuoc(donHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-GET-04", "Order", "Lấy chi tiết đơn mua thiếu id",
            "Token buyer hợp lệ nhưng body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/get_purchase", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-EDIT-01", "Order", "Sửa ghi chú đơn mua",
            "Buyer sửa note của đơn hàng pending/confirmed.",
            async ctx =>
            {
                var cap = LayDonHangCoTheSua(ctx);
                var diaChi = LayDiaChiCuaTaiKhoan(ctx, cap.Buyer);
                var note = $"note testcase {DateTimeOffset.Now:HHmmss}";
                var req = new YeuCauApi(HttpMethod.Post, "/order/edit_purchase",
                    Obj(
                        ("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("address_id", IdBatBuoc(diaChi.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server").ToString()),
                        ("note", note)),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                req.Tam["diaChi"] = diaChi.DiaChi;
                req.Tam["note"] = note;
                return req;
            },
            Ok,
            DataCoTruong("id", "state", "note"),
            async (_, request, ctx) =>
            {
                var donHang = (DonHangSeed)request.Tam["donHang"]!;
                var diaChi = (DiaChiTaiKhoanSeed)request.Tam["diaChi"]!;
                var note = request.Tam["note"]?.ToString() ?? "";
                donHang.DiaChiIdServer = diaChi.DiaChiIdServer;
                await ctx.CapNhatDB.CapNhatGhiChuDonHangAsync(donHang, "Sửa note bằng ORDER-PURCHASE-EDIT-01: " + note);
            });

        Them(ds, "ORDER-PURCHASE-EDIT-02", "Order", "Sửa đơn mua id không tồn tại",
            "id = 999999999.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/edit_purchase",
                Obj(("id", "999999999"), ("note", "note invalid")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-EDIT-03", "Order", "Sửa đơn mua token sai",
            "Body hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangCoTheSua(ctx);
                return Req(HttpMethod.Post, "/order/edit_purchase",
                    Obj(("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()), ("note", "note token sai")),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-PURCHASE-EDIT-04", "Order", "Sửa đơn mua thiếu id",
            "Token buyer hợp lệ nhưng body thiếu id.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/edit_purchase", Obj(("note", "missing id")), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "ORDER-PURCHASE-EDIT-05", "Order", "Sửa đơn mua sai trạng thái",
            "Token buyer hợp lệ nhưng đơn không còn pending/confirmed.",
            async ctx =>
            {
                var cap = LayDonHangKhongTheSua(ctx);
                var diaChi = LayDiaChiCuaTaiKhoan(ctx, cap.Buyer);
                return new YeuCauApi(HttpMethod.Post, "/order/edit_purchase",
                    Obj(
                        ("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("address_id", IdBatBuoc(diaChi.DiaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server").ToString()),
                        ("note", "không được sửa trạng thái này")),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-CANCEL-01", "Order", "Buyer hủy đơn pending/confirmed",
            "Token buyer hợp lệ, id là đơn của buyer đang pending hoặc confirmed.",
            async ctx =>
            {
                var cap = LayDonHangCoTheSua(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/order/cancel_order",
                    Obj(
                        ("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("reason", "Buyer cancel test")),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            KiemTraCancelOrder(),
            async (response, request, ctx) => await ctx.CapNhatDB.CapNhatDonHangSauBuyerCancelAsync(request, response));

        Them(ds, "ORDER-CANCEL-02", "Order", "Buyer hủy đơn token sai",
            "Body hủy đơn hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return Req(HttpMethod.Post, "/order/cancel_order",
                    Obj(
                        ("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("reason", "Buyer cancel token sai")),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-CANCEL-03", "Order", "Buyer hủy đơn không được phép",
            "Token buyer hợp lệ nhưng đơn không thuộc trạng thái pending/confirmed.",
            async ctx =>
            {
                var cap = LayDonHangKhongTheSua(ctx);
                return new YeuCauApi(HttpMethod.Post, "/order/cancel_order",
                    Obj(
                        ("id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("reason", "Buyer cancel invalid state")),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            SaiGiaTri);

        Them(ds, "ORDER-REFUND-01", "Order", "Buyer yêu cầu refund đơn delivered",
            "Token buyer hợp lệ, purchase_id là đơn delivered và seller đủ balance trong wallet_seed.",
            async ctx =>
            {
                var cap = LayDonHangDeliveredCoTheRefund(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/order/refund_order",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("reason", "Refund test")),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            null,
            async (_, request, ctx) => await ctx.CapNhatDB.CapNhatDonHangSauRefundAsync(request));

        Them(ds, "ORDER-REFUND-02", "Order", "Buyer refund token sai",
            "Body refund hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return Req(HttpMethod.Post, "/order/refund_order",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("reason", "Refund token sai")),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-REFUND-03", "Order", "Buyer refund đơn không hợp lệ",
            "Token buyer hợp lệ nhưng đơn không ở trạng thái delivered.",
            async ctx =>
            {
                var cap = LayDonHangKhacTrangThai(ctx, "delivered");
                return new YeuCauApi(HttpMethod.Post, "/order/refund_order",
                    Obj(
                        ("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString()),
                        ("reason", "Refund invalid state")),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
            },
            OrderLoiNghiepVuHoacGiaTri);

        Them(ds, "ORDER-TIMELINE-01", "Order", "Lấy timeline đơn hàng",
            "Token buyer hợp lệ, purchase_id là đơn liên quan tới user.",
            async ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                var req = new YeuCauApi(HttpMethod.Post, "/order/get_order_timeline",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    await LayTokenCuaTaiKhoanAsync(ctx, cap.Buyer));
                req.Tam["donHang"] = cap.DonHang;
                return req;
            },
            Ok,
            KiemTraTimelineDonHang());

        Them(ds, "ORDER-TIMELINE-02", "Order", "Lấy timeline token sai",
            "purchase_id hợp lệ nhưng token sai.",
            ctx =>
            {
                var cap = LayDonHangDangLuu(ctx);
                return Req(HttpMethod.Post, "/order/get_order_timeline",
                    Obj(("purchase_id", IdBatBuoc(cap.DonHang.DonHangIdServer, "donhang_seed.donhang_id_server").ToString())),
                    ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-TIMELINE-03", "Order", "Lấy timeline purchase_id không hợp lệ",
            "Token hợp lệ nhưng body thiếu purchase_id.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/order/get_order_timeline", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseCreateOrderCoId()
    {
        return (response, request, _) =>
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

            if (!string.Equals(state, "pending", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"Response create_order phải có status/state = pending, thực tế là `{state}`."));
            }

            var totalPrice = DocDecimalTuNode(response.Data?["total_price"]);
            var shippingFee = DocDecimalTuNode(response.Data?["shipping_fee"]) ?? DocDecimalTuNode(response.Data?["ship_fee"]);
            var finalPrice = DocDecimalTuNode(response.Data?["final_price"]);
            if (totalPrice is null || shippingFee is null || finalPrice is null)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response create_order thiếu total_price, shipping_fee/ship_fee hoặc final_price."));
            }

            if (finalPrice.Value != totalPrice.Value + shippingFee.Value)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"final_price phải bằng total_price + ship_fee, thực tế {finalPrice} != {totalPrice} + {shippingFee}."));
            }

            if (request.Tam.TryGetValue("diaChi", out var diaChiTam) &&
                diaChiTam is DiaChiTaiKhoanSeed diaChi)
            {
                var addressId = DocIntTuNode(response.Data?["address_id"]);
                if (addressId != diaChi.DiaChiIdServer)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data.address_id phải bằng {diaChi.DiaChiIdServer}, thực tế là {addressId?.ToString() ?? "null"}."));
                }
            }

            if (request.Tam.TryGetValue("orderSource", out var orderSourceTam) &&
                orderSourceTam is int orderSource)
            {
                var source = DocIntTuNode(response.Data?["source"]) ?? DocIntTuNode(response.Data?["order_source"]);
                if (source != orderSource)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data.order_source/source phải bằng {orderSource}, thực tế là {source?.ToString() ?? "null"}."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraCancelOrder()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của cancel_order không phải object."));
            }

            if (request.Tam["donHang"] is DonHangSeed donHang)
            {
                var id = DocIdSau(data, "id");
                if (id != donHang.DonHangIdServer)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data.id phải bằng {donHang.DonHangIdServer}, thực tế là {id?.ToString() ?? "null"}."));
                }
            }

            var state = DocChuoiTuNode(data["state"]);
            if (!string.Equals(state, "cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.state phải là cancelled, thực tế là `{state ?? "null"}`."));
            }

            if (!data.ContainsKey("refunded_coins") || !data.ContainsKey("refunded_at"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data thiếu refunded_coins hoặc refunded_at."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraTimelineDonHang()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray timeline)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_order_timeline không phải mảng."));
            }

            if (timeline.Count == 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Timeline đơn hàng đang rỗng."));
            }

            var donHang = request.Tam.TryGetValue("donHang", out var tam) ? tam as DonHangSeed : null;
            foreach (var node in timeline)
            {
                if (node is not JsonObject item)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, "Mỗi item timeline phải là object."));
                }

                foreach (var truong in new[] { "id", "purchase_id", "state", "created_at" })
                {
                    if (!item.ContainsKey(truong))
                    {
                        return Task.FromResult(new KetQuaKiemTraThem(false, $"Timeline item thiếu trường `{truong}`."));
                    }
                }

                if (donHang is not null)
                {
                    var purchaseId = DocIntTuNode(item["purchase_id"]);
                    if (purchaseId != donHang.DonHangIdServer)
                    {
                        return Task.FromResult(new KetQuaKiemTraThem(false, $"timeline.purchase_id phải bằng {donHang.DonHangIdServer}, thực tế là {purchaseId?.ToString() ?? "null"}."));
                    }
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraChiTietDonMua()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_purchase không phải object."));
            }

            if (!request.Tam.TryGetValue("donHang", out var tamDonHang) ||
                tamDonHang is not DonHangSeed donHang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Thiếu donhang_seed mong đợi để kiểm tra get_purchase."));
            }

            var id = DocIdSau(data, "id");
            if (id != donHang.DonHangIdServer)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.id phải bằng {donHang.DonHangIdServer}, thực tế là {id?.ToString() ?? "null"}."));
            }

            var state = DocChuoiTuNode(data["state"]);
            if (!string.Equals(state, donHang.TrangThai, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.state phải là `{donHang.TrangThai}`, thực tế là `{state ?? "null"}`."));
            }

            foreach (var truong in new[] { "total_price", "ship_fee", "final_price", "items", "seller", "buyer" })
            {
                if (!data.ContainsKey(truong))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data thiếu trường `{truong}`."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraTrangThaiDonHang()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_order_status không phải object."));
            }

            if (!request.Tam.TryGetValue("donHang", out var tamDonHang) ||
                tamDonHang is not DonHangSeed donHang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Thiếu donhang_seed mong đợi để kiểm tra get_order_status."));
            }

            var id = DocIdSau(data, "id");
            if (id != donHang.DonHangIdServer)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.id phải bằng {donHang.DonHangIdServer}, thực tế là {id?.ToString() ?? "null"}."));
            }

            foreach (var truong in new[] { "ship_from", "ship_to", "price", "ship_fee", "create", "leatime", "current_status", "status_history", "products" })
            {
                if (!data.ContainsKey(truong))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data thiếu trường `{truong}`."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static (TaiKhoanSignupThanhCongSeed Buyer, DonHangSeed DonHang) LayBuyerVaDonHangKhongThuocBuyer(NguCanhKiemThu ctx)
    {
        var donHang = ctx.CapNhatDB.DuLieu.DonHangSeed
            .Where(DonHangDangLuu)
            .OrderByDescending(x => x.TaoLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.DonHangIdServer)
            .ToList();

        foreach (var donHangMucTieu in donHang)
        {
            var buyerKhac = donHang
                .Where(x => x.BuyerTaiKhoanIdServer != donHangMucTieu.BuyerTaiKhoanIdServer)
                .Select(x => LayTaiKhoanTheoServerId(ctx, x.BuyerTaiKhoanIdServer))
                .FirstOrDefault(x => x is not null);

            if (buyerKhac is not null)
            {
                return (buyerKhac, donHangMucTieu);
            }
        }

        throw new BoQuaKiemThuException("Thiếu ít nhất hai buyer khác nhau trong donhang_seed để kiểm tra get_purchase không thuộc buyer.");
    }

    private static CapDonHang LayDonHangDeliveredCoTheRefund(NguCanhKiemThu ctx)
    {
        foreach (var donHang in ctx.CapNhatDB.DuLieu.DonHangSeed
                     .Where(DonHangDangLuu)
                     .Where(x => string.Equals(x.TrangThai, "delivered", StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(x => x.CapNhatLuc ?? x.TaoLuc ?? DateTimeOffset.MinValue)
                     .ThenByDescending(x => x.DonHangIdServer))
        {
            var buyer = LayTaiKhoanTheoServerId(ctx, donHang.BuyerTaiKhoanIdServer);
            var seller = LayTaiKhoanTheoServerId(ctx, donHang.SellerTaiKhoanIdServer);
            if (buyer is null || seller is null)
            {
                continue;
            }

            var soTien = TinhSoTienDonHang(donHang);
            var sellerWallet = LayWalletCuaTaiKhoan(ctx, seller);
            if (soTien > 0 && sellerWallet?.Balance >= soTien)
            {
                return new CapDonHang(donHang, buyer, seller);
            }
        }

        throw new BoQuaKiemThuException("Thiếu donhang_seed delivered có seller.balance đủ để chạy ORDER-REFUND-01.");
    }

    private static decimal TinhSoTienDonHang(DonHangSeed donHang)
    {
        if (donHang.FinalPrice is > 0)
        {
            return donHang.FinalPrice.Value;
        }

        return (donHang.TotalPrice ?? donHang.ThanhTien ?? 0m) + (donHang.ShippingFee ?? 0m);
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

            if (request.Tam.TryGetValue("sanPham", out var sanPhamTam) &&
                sanPhamTam is SanPhamSeed sanPham)
            {
                var productId = DocIntTuNode(response.Data?["product_id"]);
                if (productId != sanPham.SanPhamIdServer)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data.product_id phải bằng {sanPham.SanPhamIdServer}, thực tế là {productId?.ToString() ?? "null"}."));
                }
            }

            var soLuongThem = request.Tam.TryGetValue("soLuong", out var soLuongTam) && soLuongTam is int sl ? sl : (int?)null;
            var soLuongCu = request.Tam.TryGetValue("gioHang", out var gioHangTam) && gioHangTam is GioHangSeed gioHang ? gioHang.SoLuong : 0;
            if (soLuongThem is > 0)
            {
                var soLuongMongDoi = soLuongCu + soLuongThem.Value;
                var soLuongThucTe = DocIntTuNode(response.Data?["quantity"]);
                if (soLuongThucTe != soLuongMongDoi)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data.quantity phải bằng {soLuongMongDoi}, thực tế là {soLuongThucTe?.ToString() ?? "null"}."));
                }
            }

            if (!response.Data!.AsObject().ContainsKey("subtotal"))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response cart thành công thiếu subtotal."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
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
