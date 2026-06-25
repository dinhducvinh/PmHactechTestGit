using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    internal async Task LuuGioHangSauAddCartAsync(PhanHoiApi response, YeuCauApi request)
    {
        if (!LaMaThanhCong(response))
        {
            return;
        }

        var buyer = (TaiKhoanSignupThanhCongSeed)request.Tam["buyer"]!;
        var sanPham = (SanPhamSeed)request.Tam["sanPham"]!;
        var soLuongThem = request.Tam.TryGetValue("soLuong", out var soLuongTam) && soLuongTam is int soLuong
            ? soLuong
            : 1;
        var gioHangHienCo = request.Tam.TryGetValue("gioHang", out var gioHangTam)
            ? gioHangTam as GioHangSeed
            : null;
        var soLuongTruoc = request.Tam.TryGetValue("soLuongTruoc", out var soLuongTruocTam) &&
                           soLuongTruocTam is int soLuongTruocTamInt
            ? Math.Max(soLuongTruocTamInt, 0)
            : gioHangHienCo?.SoLuong ?? 0;

        var cartItemIdServer =
            DocIdSau(response.Data, "cart_item_id") ??
            DocIdSau(response.Data, "id") ??
            gioHangHienCo?.CartItemIdServer;
        if (cartItemIdServer is not > 0)
        {
            throw new LoiChuanBiKiemThuException(
                "API /order/add_cart đã thành công nhưng response thiếu cart_item_id nên không thể lưu giohang_seed.");
        }

        var sanPhamIdServer =
            DocIntTuNode(response.Data?["product_id"]) ??
            sanPham.SanPhamIdServer ??
            gioHangHienCo?.SanPhamIdServer;
        if (sanPhamIdServer is not > 0)
        {
            throw new LoiChuanBiKiemThuException(
                "API /order/add_cart đã thành công nhưng không xác định được sp_id_server để lưu giohang_seed.");
        }

        var soLuongMoi =
            DocIntTuNode(response.Data?["quantity"]) ??
            soLuongTruoc + soLuongThem;
        var lucCapNhat = DateTimeOffset.Now;

        var gioHang = DuLieu.GioHangSeed.FirstOrDefault(x => x.CartItemIdServer == cartItemIdServer) ??
                      DuLieu.GioHangSeed.FirstOrDefault(x =>
                          x.BuyerTaiKhoanIdServer == buyer.TaiKhoanIdServer &&
                          x.SanPhamIdServer == sanPhamIdServer);

        if (gioHang is null)
        {
            gioHang = new GioHangSeed
            {
                CartItemIdServer = cartItemIdServer,
                BuyerTaiKhoanIdServer = buyer.TaiKhoanIdServer,
                SanPhamIdServer = sanPhamIdServer,
                SoLuong = Math.Max(soLuongMoi, 1),
                TrangThai = "dang_trong_gio",
                TaoLuc = lucCapNhat,
                CapNhatLuc = lucCapNhat,
                GhiChu = "Tạo bởi testcase ORDER-CART-ADD-01"
            };
            DuLieu.GioHangSeed.Add(gioHang);
        }
        else
        {
            gioHang.CartItemIdServer = cartItemIdServer;
            gioHang.BuyerTaiKhoanIdServer = buyer.TaiKhoanIdServer;
            gioHang.SanPhamIdServer = sanPhamIdServer;
            gioHang.SoLuong = Math.Max(soLuongMoi, 1);
            gioHang.TrangThai = "dang_trong_gio";
            gioHang.TaoLuc ??= lucCapNhat;
            gioHang.CapNhatLuc = lucCapNhat;
            gioHang.GhiChu = "Cập nhật bởi testcase ORDER-CART-ADD";
        }

        DuLieu.GioHangSeed.RemoveAll(x =>
            x != gioHang &&
            x.BuyerTaiKhoanIdServer == buyer.TaiKhoanIdServer &&
            x.SanPhamIdServer == sanPhamIdServer);

        await LuuAsync(BangDuLieuSeed.GioHang);
    }

    internal async Task CapNhatSoLuongGioHangAsync(GioHangSeed gioHang, int soLuongMoi)
    {
        gioHang.SoLuong = Math.Max(soLuongMoi, 1);
        gioHang.TrangThai = "dang_trong_gio";
        gioHang.CapNhatLuc = DateTimeOffset.Now;
        gioHang.GhiChu = "Cập nhật bởi testcase ORDER-CART-EDIT-01";
        await LuuAsync(BangDuLieuSeed.GioHang);
    }

    internal async Task XoaGioHangAsync(GioHangSeed gioHang)
    {
        DuLieu.GioHangSeed.RemoveAll(x =>
            x.CartItemIdServer == gioHang.CartItemIdServer ||
            (x.BuyerTaiKhoanIdServer == gioHang.BuyerTaiKhoanIdServer &&
             x.SanPhamIdServer == gioHang.SanPhamIdServer));
        await LuuAsync(BangDuLieuSeed.GioHang);
    }

}
