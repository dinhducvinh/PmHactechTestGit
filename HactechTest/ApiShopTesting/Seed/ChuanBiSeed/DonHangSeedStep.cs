using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task TaoDonHangSeedAsync()
    {
        var sanPhamSanSang = _nguCanh.CapNhatDB.DuLieu.SanPhamSeed
            .Where(x => x.TrangThai == "san_sang" && x.SanPhamIdServer is > 0 && x.TaiKhoanIdServer is > 0)
            .OrderBy(x => x.ThuTuNoiBo)
            .ToList();
        var buyerCoDiaChi = LayTaiKhoanDaDangKySanSang()
            .Select(tk => new
            {
                TaiKhoan = tk,
                DiaChi = _nguCanh.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(dc =>
                    dc.TaiKhoanIdServer == tk.TaiKhoanIdServer &&
                    dc.TrangThai == "san_sang" &&
                    dc.DiaChiIdServer is > 0)
            })
            .Where(x => x.DiaChi is not null)
            .OrderBy(x => x.TaiKhoan.SoThuTu)
            .ToList();
        if (sanPhamSanSang.Count == 0 || buyerCoDiaChi.Count == 0)
        {
            return;
        }

        var sanPhamDaCoDon = _nguCanh.CapNhatDB.DuLieu.DonHangSeed
            .Where(x => x.DonHangIdServer is > 0 && x.SanPhamIdServer is > 0)
            .Select(x => x.SanPhamIdServer!.Value)
            .ToHashSet();
        var danhSachSanPhamUuTien = sanPhamSanSang
            .OrderBy(x => sanPhamDaCoDon.Contains(x.SanPhamIdServer!.Value) ? 1 : 0)
            .ThenBy(x => x.ThuTuNoiBo)
            .ToList();

        foreach (var sanPham in danhSachSanPhamUuTien)
        {
            if (DaDuDonHangSeed())
            {
                break;
            }

            var seller = _nguCanh.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed.FirstOrDefault(x =>
                x.TaiKhoanIdServer == sanPham.TaiKhoanIdServer);
            if (seller is null)
            {
                continue;
            }

            var buyerVaDiaChi = buyerCoDiaChi.FirstOrDefault(x =>
                x.TaiKhoan.TaiKhoanIdServer != sanPham.TaiKhoanIdServer &&
                !CoQuanHeChan(x.TaiKhoan.TaiKhoanIdServer, sanPham.TaiKhoanIdServer));
            if (buyerVaDiaChi is null)
            {
                continue;
            }

            const int soLuong = 1;
            const int orderSource = 1;
            var sanPhamIdServer = IdChoBody(sanPham.SanPhamIdServer);
            var diaChiIdServer = IdChoBody(buyerVaDiaChi.DiaChi!.DiaChiIdServer);
            if (sanPhamIdServer is null || diaChiIdServer is null)
            {
                continue;
            }

            var itemBody = new Dictionary<string, object?>
            {
                ["product_id"] = sanPhamIdServer,
                ["quantity"] = soLuong
            };
            var body = new Dictionary<string, object?>
            {
                ["items"] = new[] { itemBody },
                ["address_id"] = diaChiIdServer,
                ["order_source"] = orderSource
            };
            var thanhTien = sanPham.Gia * soLuong;

            var token = await LayTokenSeedAsync(
                buyerVaDiaChi.TaiKhoan,
                $"tạo đơn hàng seed cho buyer {buyerVaDiaChi.TaiKhoan.SoThuTu}");
            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/order/create_order",
                body,
                token,
                $"tạo đơn hàng seed cho buyer {buyerVaDiaChi.TaiKhoan.SoThuTu}");

            var requestLuu = new YeuCauApi(HttpMethod.Post, "/order/create_order", body, token);
            requestLuu.Tam["buyer"] = buyerVaDiaChi.TaiKhoan;
            requestLuu.Tam["seller"] = seller;
            requestLuu.Tam["diaChi"] = buyerVaDiaChi.DiaChi;
            requestLuu.Tam["sanPhamIdServer"] = sanPhamIdServer.Value;
            requestLuu.Tam["soLuong"] = soLuong;
            requestLuu.Tam["donGia"] = sanPham.Gia;
            requestLuu.Tam["thanhTien"] = thanhTien;
            requestLuu.Tam["orderSource"] = orderSource;
            requestLuu.Tam["loaiSeed"] = "order_seed";
            await _nguCanh.CapNhatDB.LuuDonHangSauCreateOrderAsync(response, requestLuu);
            sanPhamDaCoDon.Add(sanPhamIdServer.Value);
        }
    }

    private bool DaDuDonHangSeed()
    {
        var donHangSeed = _nguCanh.CapNhatDB.DuLieu.DonHangSeed;
        return donHangSeed.Count(DonHangDangLuu) >= YeuCauDuLieuSeed.SoDonHangMucTieu &&
               donHangSeed.Count(DonHangCoTheSua) >= YeuCauDuLieuSeed.SoDonHangCoTheSuaMucTieu;
    }

    private static bool DonHangCoTheSua(DonHangSeed donHang)
    {
        return DonHangDangLuu(donHang) &&
               (string.Equals(donHang.TrangThai, "pending", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(donHang.TrangThai, "confirmed", StringComparison.OrdinalIgnoreCase));
    }

}
