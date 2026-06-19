using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task TaoGioHangSeedAsync()
    {
        var sanPhamSanSang = _nguCanh.CapNhatDB.DuLieu.SanPhamSeed
            .Where(x => x.TrangThai == "san_sang" && x.SanPhamIdServer is > 0 && x.TaiKhoanIdServer is > 0)
            .OrderBy(x => x.ThuTuNoiBo)
            .ToList();
        var taiKhoanSanSang = LayTaiKhoanDaDangKySanSang();
        if (sanPhamSanSang.Count == 0 || taiKhoanSanSang.Count == 0)
        {
            return;
        }

        var buyerCoDiaChi = _nguCanh.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed
            .Where(x => x.TrangThai == "san_sang" && x.DiaChiIdServer is > 0 && x.TaiKhoanIdServer is > 0)
            .Select(x => x.TaiKhoanIdServer!.Value)
            .ToHashSet();
        var daCo = _nguCanh.CapNhatDB.DuLieu.GioHangSeed
            .Where(GioHangDangTrongGio)
            .Select(x => (BuyerTaiKhoanIdServer: x.BuyerTaiKhoanIdServer!.Value, SanPhamIdServer: x.SanPhamIdServer!.Value))
            .ToHashSet();
        var sanPhamDaCoTrongGio = _nguCanh.CapNhatDB.DuLieu.GioHangSeed
            .Where(GioHangDangTrongGio)
            .Select(x => x.SanPhamIdServer!.Value)
            .ToHashSet();

        var coThayDoi = false;
        foreach (var sanPham in sanPhamSanSang)
        {
            if (_nguCanh.CapNhatDB.DuLieu.GioHangSeed.Count(GioHangDangTrongGio) >= YeuCauDuLieuSeed.SoGioHangMucTieu)
            {
                break;
            }

            var sanPhamIdServer = IdChoBody(sanPham.SanPhamIdServer);
            if (sanPhamIdServer is null || sanPhamDaCoTrongGio.Contains(sanPhamIdServer.Value))
            {
                continue;
            }

            var buyer = taiKhoanSanSang
                .OrderByDescending(x => buyerCoDiaChi.Contains(x.TaiKhoanIdServer))
                .ThenBy(x => x.SoThuTu)
                .FirstOrDefault(x =>
                    x.TaiKhoanIdServer is > 0 &&
                    x.TaiKhoanIdServer != sanPham.TaiKhoanIdServer &&
                    !daCo.Contains((x.TaiKhoanIdServer, sanPhamIdServer.Value)) &&
                    !CoQuanHeChan(x.TaiKhoanIdServer, sanPham.TaiKhoanIdServer));
            if (buyer is null)
            {
                continue;
            }

            var token = await LayTokenSeedAsync(buyer, $"tạo giỏ hàng seed cho tài khoản buyer {buyer.SoThuTu}");
            const int soLuong = 1;
            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/order/add_cart",
                new Dictionary<string, object?>
                {
                    ["product_id"] = sanPhamIdServer,
                    ["quantity"] = soLuong
                },
                token,
                $"tạo giỏ hàng seed cho tài khoản buyer {buyer.SoThuTu}");

            var cartItemIdServer = DocIdSau(response.Data, "cart_item_id") ?? DocIdSau(response.Data, "id");
            if (cartItemIdServer is not > 0)
            {
                throw new LoiChuanBiKiemThuException(
                    $"API /order/add_cart trả thành công nhưng không có cart_item_id cho tài khoản seed {buyer.SoThuTu}. Response: {RutGon(response.NoiDungRaw)}");
            }

            var soLuongThucTe = DocIntTuNode(response.Data?["quantity"]) ?? soLuong;
            var lucTao = DateTimeOffset.Now;
            _nguCanh.CapNhatDB.DuLieu.GioHangSeed.Add(new GioHangSeed
            {
                CartItemIdServer = cartItemIdServer,
                BuyerTaiKhoanIdServer = buyer.TaiKhoanIdServer,
                SanPhamIdServer = sanPham.SanPhamIdServer,
                SoLuong = Math.Max(soLuongThucTe, 1),
                TrangThai = "dang_trong_gio",
                TaoLuc = lucTao,
                CapNhatLuc = lucTao,
                GhiChu = "Tạo bằng API /order/add_cart"
            });
            daCo.Add((buyer.TaiKhoanIdServer, sanPhamIdServer.Value));
            sanPhamDaCoTrongGio.Add(sanPhamIdServer.Value);
            coThayDoi = true;
        }

        if (coThayDoi)
        {
            await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.GioHang);
        }
    }

}
