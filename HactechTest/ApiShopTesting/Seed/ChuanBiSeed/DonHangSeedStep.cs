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
            .Select(x => new CapBuyerDiaChiSeed(x.TaiKhoan, x.DiaChi!))
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
        var capDonHangDaCo = _nguCanh.CapNhatDB.DuLieu.DonHangSeed
            .Where(DonHangDangLuu)
            .Where(x => x.BuyerTaiKhoanIdServer is > 0 && x.SanPhamIdServer is > 0)
            .Select(x => (BuyerId: x.BuyerTaiKhoanIdServer!.Value, SanPhamId: x.SanPhamIdServer!.Value))
            .ToHashSet();

        var chiSoSanPham = 0;
        var chiSoBuyer = 0;
        var soLanThu = 0;
        var soLanThuToiDa = Math.Max(YeuCauDuLieuSeed.SoDonHangMucTieu * 4, sanPhamSanSang.Count * 2);
        LoiChuanBiKiemThuException? loiGanNhat = null;

        while (!DaDuDonHangSeed() && soLanThu < soLanThuToiDa)
        {
            soLanThu++;
            var trangThaiMucTieu = LayTrangThaiDonHangConThieu();
            if (trangThaiMucTieu is null)
            {
                break;
            }

            var sanPham = ChonSanPhamTaoDonHang(sanPhamSanSang, sanPhamDaCoDon, ref chiSoSanPham);
            if (sanPham is null)
            {
                break;
            }

            var sanPhamIdServer = IdChoBody(sanPham.SanPhamIdServer);
            if (sanPhamIdServer is null)
            {
                continue;
            }

            var seller = LayTaiKhoanSeedTheoId(sanPham.TaiKhoanIdServer);
            if (seller is null)
            {
                sanPhamDaCoDon.Add(sanPhamIdServer.Value);
                continue;
            }

            var buyerVaDiaChi = ChonBuyerTaoDonHang(buyerCoDiaChi, sanPham, capDonHangDaCo, ref chiSoBuyer);
            if (buyerVaDiaChi is null)
            {
                sanPhamDaCoDon.Add(sanPhamIdServer.Value);
                continue;
            }

            try
            {
                var donHang = await TaoDonHangPendingSeedAsync(sanPham, seller, buyerVaDiaChi);
                sanPhamDaCoDon.Add(sanPhamIdServer.Value);
                capDonHangDaCo.Add((buyerVaDiaChi.TaiKhoan.TaiKhoanIdServer, sanPhamIdServer.Value));
                await DuaDonHangSeedDenTrangThaiAsync(donHang, trangThaiMucTieu);
            }
            catch (LoiChuanBiKiemThuException ex)
            {
                loiGanNhat = ex;
                sanPhamDaCoDon.Add(sanPhamIdServer.Value);
            }
        }

        if (!DaDuDonHangSeed() && loiGanNhat is not null)
        {
            throw new LoiChuanBiKiemThuException(
                $"Không tạo đủ donhang_seed theo trạng thái. Hiện tại: {TaoTomTatDonHangSeed()}. Lỗi gần nhất: {loiGanNhat.Message}");
        }
    }

    private bool DaDuDonHangSeed()
    {
        return YeuCauDuLieuSeed.TrangThaiDonHangMucTieu.All(x =>
            DemDonHangTheoTrangThai(x) >= YeuCauDuLieuSeed.SoDonHangMoiTrangThaiMucTieu);
    }

    private string? LayTrangThaiDonHangConThieu()
    {
        return YeuCauDuLieuSeed.TrangThaiDonHangMucTieu.FirstOrDefault(x =>
            DemDonHangTheoTrangThai(x) < YeuCauDuLieuSeed.SoDonHangMoiTrangThaiMucTieu);
    }

    private int DemDonHangTheoTrangThai(string trangThai)
    {
        return _nguCanh.CapNhatDB.DuLieu.DonHangSeed.Count(x =>
            DonHangDangLuu(x) &&
            string.Equals(x.TrangThai, trangThai, StringComparison.OrdinalIgnoreCase));
    }

    private string TaoTomTatDonHangSeed()
    {
        return string.Join(", ", YeuCauDuLieuSeed.TrangThaiDonHangMucTieu.Select(x =>
            $"{x} {DemDonHangTheoTrangThai(x)}/{YeuCauDuLieuSeed.SoDonHangMoiTrangThaiMucTieu}"));
    }

    private async Task<DonHangSeed> TaoDonHangPendingSeedAsync(
        SanPhamSeed sanPham,
        TaiKhoanSignupThanhCongSeed seller,
        CapBuyerDiaChiSeed buyerVaDiaChi)
    {
        const int soLuong = 1;
        const int orderSource = 1;
        var sanPhamIdServer = IdChoBody(sanPham.SanPhamIdServer);
        var diaChiIdServer = IdChoBody(buyerVaDiaChi.DiaChi.DiaChiIdServer);
        if (sanPhamIdServer is null || diaChiIdServer is null)
        {
            throw new LoiChuanBiKiemThuException("Thiếu product_id hoặc address_id hợp lệ để tạo donhang_seed.");
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
        return await _nguCanh.CapNhatDB.LuuDonHangSauCreateOrderAsync(response, requestLuu);
    }

    private async Task DuaDonHangSeedDenTrangThaiAsync(DonHangSeed donHang, string trangThaiMucTieu)
    {
        switch (trangThaiMucTieu.ToLowerInvariant())
        {
            case "pending":
                return;
            case "confirmed":
                await DamBaoDonHangConfirmedAsync(donHang);
                return;
            case "shipping":
                await DamBaoDonHangShippingAsync(donHang);
                return;
            case "delivered":
                await DamBaoDonHangDeliveredAsync(donHang);
                return;
            case "cancelled":
                await TuChoiDonHangSeedAsync(donHang);
                return;
            case "refunded":
                await DamBaoDonHangDeliveredAsync(donHang);
                await RefundDonHangSeedAsync(donHang);
                return;
            default:
                throw new LoiChuanBiKiemThuException($"Trạng thái donhang_seed mục tiêu không được hỗ trợ: {trangThaiMucTieu}.");
        }
    }

    private async Task DamBaoDonHangConfirmedAsync(DonHangSeed donHang)
    {
        if (string.Equals(donHang.TrangThai, "confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(donHang.TrangThai, "pending", StringComparison.OrdinalIgnoreCase))
        {
            throw new LoiChuanBiKiemThuException(
                $"Không thể chuyển donhang_seed {donHang.DonHangIdServer} từ {donHang.TrangThai} sang confirmed.");
        }

        await ChapNhanDonHangSeedAsync(donHang);
    }

    private async Task DamBaoDonHangShippingAsync(DonHangSeed donHang)
    {
        if (string.Equals(donHang.TrangThai, "shipping", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await DamBaoDonHangConfirmedAsync(donHang);
        await DanhDauDonHangDaGuiSeedAsync(donHang);
    }

    private async Task DamBaoDonHangDeliveredAsync(DonHangSeed donHang)
    {
        if (string.Equals(donHang.TrangThai, "delivered", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await DamBaoDonHangShippingAsync(donHang);
        await XacNhanDaNhanHangSeedAsync(donHang);
    }

    private async Task ChapNhanDonHangSeedAsync(DonHangSeed donHang)
    {
        var seller = LayTaiKhoanSeedBatBuoc(donHang.SellerTaiKhoanIdServer, "seller");
        var token = await LayTokenSeedAsync(seller, $"seller chấp nhận đơn hàng seed {donHang.DonHangIdServer}");
        var body = BodySellerCapNhatDonHang(donHang, isAccept: 1);
        await GuiApiSeedAsync(
            HttpMethod.Post,
            "/order/set_accept_buyer",
            body,
            token,
            $"seller chấp nhận đơn hàng seed {donHang.DonHangIdServer}");

        var request = new YeuCauApi(HttpMethod.Post, "/order/set_accept_buyer", body, token);
        request.Tam["donHang"] = donHang;
        await _nguCanh.CapNhatDB.CapNhatDonHangSauSellerAcceptAsync(request, dongY: true);
    }

    private async Task TuChoiDonHangSeedAsync(DonHangSeed donHang)
    {
        var seller = LayTaiKhoanSeedBatBuoc(donHang.SellerTaiKhoanIdServer, "seller");
        var token = await LayTokenSeedAsync(seller, $"seller từ chối đơn hàng seed {donHang.DonHangIdServer}");
        var body = BodySellerCapNhatDonHang(donHang, isAccept: 0);
        await GuiApiSeedAsync(
            HttpMethod.Post,
            "/order/set_accept_buyer",
            body,
            token,
            $"seller từ chối đơn hàng seed {donHang.DonHangIdServer}");

        var request = new YeuCauApi(HttpMethod.Post, "/order/set_accept_buyer", body, token);
        request.Tam["donHang"] = donHang;
        await _nguCanh.CapNhatDB.CapNhatDonHangSauSellerAcceptAsync(request, dongY: false);
    }

    private async Task DanhDauDonHangDaGuiSeedAsync(DonHangSeed donHang)
    {
        var seller = LayTaiKhoanSeedBatBuoc(donHang.SellerTaiKhoanIdServer, "seller");
        var token = await LayTokenSeedAsync(seller, $"seller đánh dấu shipped đơn hàng seed {donHang.DonHangIdServer}");
        var body = BodySellerCapNhatDonHang(donHang);
        await GuiApiSeedAsync(
            HttpMethod.Post,
            "/order/seller_mark_as_shipped",
            body,
            token,
            $"seller đánh dấu shipped đơn hàng seed {donHang.DonHangIdServer}");

        var request = new YeuCauApi(HttpMethod.Post, "/order/seller_mark_as_shipped", body, token);
        request.Tam["donHang"] = donHang;
        await _nguCanh.CapNhatDB.CapNhatDonHangSauSellerShippedAsync(request);
    }

    private async Task XacNhanDaNhanHangSeedAsync(DonHangSeed donHang)
    {
        var buyer = LayTaiKhoanSeedBatBuoc(donHang.BuyerTaiKhoanIdServer, "buyer");
        var token = await LayTokenSeedAsync(buyer, $"buyer xác nhận nhận hàng seed {donHang.DonHangIdServer}");
        var body = new Dictionary<string, object?>
        {
            ["purchase_id"] = IdServerDangChuoi(donHang.DonHangIdServer, "donhang_seed.donhang_id_server")
        };
        await GuiApiSeedAsync(
            HttpMethod.Post,
            "/order/buyer_confirm_received",
            body,
            token,
            $"buyer xác nhận nhận hàng seed {donHang.DonHangIdServer}");

        var request = new YeuCauApi(HttpMethod.Post, "/order/buyer_confirm_received", body, token);
        request.Tam["donHang"] = donHang;
        await _nguCanh.CapNhatDB.CapNhatDonHangSauBuyerReceivedAsync(request);
    }

    private async Task RefundDonHangSeedAsync(DonHangSeed donHang)
    {
        var buyer = LayTaiKhoanSeedBatBuoc(donHang.BuyerTaiKhoanIdServer, "buyer");
        var token = await LayTokenSeedAsync(buyer, $"buyer refund đơn hàng seed {donHang.DonHangIdServer}");
        var body = new Dictionary<string, object?>
        {
            ["purchase_id"] = IdServerDangChuoi(donHang.DonHangIdServer, "donhang_seed.donhang_id_server"),
            ["reason"] = "Refund seed"
        };
        await GuiApiSeedAsync(
            HttpMethod.Post,
            "/order/refund_order",
            body,
            token,
            $"buyer refund đơn hàng seed {donHang.DonHangIdServer}");

        var request = new YeuCauApi(HttpMethod.Post, "/order/refund_order", body, token);
        request.Tam["donHang"] = donHang;
        await _nguCanh.CapNhatDB.CapNhatDonHangSauRefundAsync(request);
    }

    private static Dictionary<string, object?> BodySellerCapNhatDonHang(DonHangSeed donHang, int? isAccept = null)
    {
        var body = new Dictionary<string, object?>
        {
            ["purchase_id"] = IdServerDangChuoi(donHang.DonHangIdServer, "donhang_seed.donhang_id_server"),
            ["buyer_id"] = IdServerDangChuoi(donHang.BuyerTaiKhoanIdServer, "donhang_seed.buyer_tk_id_server")
        };
        if (isAccept is not null)
        {
            body["is_accept"] = isAccept.Value;
        }

        return body;
    }

    private static SanPhamSeed? ChonSanPhamTaoDonHang(
        IReadOnlyList<SanPhamSeed> sanPhamSanSang,
        ISet<int> sanPhamDaCoDon,
        ref int chiSo)
    {
        for (var vong = 0; vong < 2; vong++)
        {
            for (var i = 0; i < sanPhamSanSang.Count; i++)
            {
                var viTri = (chiSo + i) % sanPhamSanSang.Count;
                var sanPham = sanPhamSanSang[viTri];
                var sanPhamIdServer = IdChoBody(sanPham.SanPhamIdServer);
                if (sanPhamIdServer is null)
                {
                    continue;
                }

                if (vong == 0 && sanPhamDaCoDon.Contains(sanPhamIdServer.Value))
                {
                    continue;
                }

                chiSo = (viTri + 1) % sanPhamSanSang.Count;
                return sanPham;
            }
        }

        return null;
    }

    private CapBuyerDiaChiSeed? ChonBuyerTaoDonHang(
        IReadOnlyList<CapBuyerDiaChiSeed> buyerCoDiaChi,
        SanPhamSeed sanPham,
        ISet<(int BuyerId, int SanPhamId)> capDonHangDaCo,
        ref int chiSo)
    {
        var sanPhamIdServer = IdChoBody(sanPham.SanPhamIdServer);
        if (sanPhamIdServer is null)
        {
            return null;
        }

        for (var vong = 0; vong < 2; vong++)
        {
            for (var i = 0; i < buyerCoDiaChi.Count; i++)
            {
                var viTri = (chiSo + i) % buyerCoDiaChi.Count;
                var buyerVaDiaChi = buyerCoDiaChi[viTri];
                var buyerId = buyerVaDiaChi.TaiKhoan.TaiKhoanIdServer;
                if (buyerId <= 0 ||
                    buyerId == sanPham.TaiKhoanIdServer ||
                    CoQuanHeChan(buyerId, sanPham.TaiKhoanIdServer))
                {
                    continue;
                }

                if (vong == 0 && capDonHangDaCo.Contains((buyerId, sanPhamIdServer.Value)))
                {
                    continue;
                }

                chiSo = (viTri + 1) % buyerCoDiaChi.Count;
                return buyerVaDiaChi;
            }
        }

        return null;
    }

    private TaiKhoanSignupThanhCongSeed? LayTaiKhoanSeedTheoId(int? taiKhoanIdServer)
    {
        return taiKhoanIdServer is > 0
            ? _nguCanh.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed.FirstOrDefault(x =>
                x.TaiKhoanIdServer == taiKhoanIdServer.Value)
            : null;
    }

    private TaiKhoanSignupThanhCongSeed LayTaiKhoanSeedBatBuoc(int? taiKhoanIdServer, string vaiTro)
    {
        return LayTaiKhoanSeedTheoId(taiKhoanIdServer)
            ?? throw new LoiChuanBiKiemThuException($"Thiếu tài khoản {vaiTro} hợp lệ cho donhang_seed.");
    }

    private static string IdServerDangChuoi(int? id, string tenTruong)
    {
        return id is > 0
            ? id.Value.ToString()
            : throw new LoiChuanBiKiemThuException($"Thiếu {tenTruong} hợp lệ để tạo request seed.");
    }

    private sealed record CapBuyerDiaChiSeed(TaiKhoanSignupThanhCongSeed TaiKhoan, DiaChiTaiKhoanSeed DiaChi);
}
