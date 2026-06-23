using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task TaoSanPhamSeedAsync()
    {
        var danhMuc = _nguCanh.CapNhatDB.DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang");
        if (danhMuc is null)
        {
            return;
        }

        var sellerCoDiaChi = LayTaiKhoanDaDangKySanSang()
            .Select(tk => new
            {
                TaiKhoan = tk,
                DiaChi = _nguCanh.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(dc =>
                    dc.TaiKhoanIdServer == tk.TaiKhoanIdServer &&
                    dc.TrangThai == "san_sang" &&
                    dc.DiaChiIdServer is > 0)
            })
            .Where(x => x.DiaChi is not null)
            .Take(YeuCauDuLieuSeed.SoSellerSanPhamMucTieu)
            .ToList();
        if (sellerCoDiaChi.Count == 0)
        {
            return;
        }

        var thuongHieu = _nguCanh.CapNhatDB.DuLieu.ThuongHieuSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" &&
            x.ThuongHieuIdServer is > 0);
        var coThayDoi = false;
        foreach (var seller in sellerCoDiaChi)
        {
            var soSanPhamCuaSeller = _nguCanh.CapNhatDB.DuLieu.SanPhamSeed.Count(x =>
                x.TaiKhoanIdServer == seller.TaiKhoan.TaiKhoanIdServer &&
                x.TrangThai == "san_sang" &&
                x.SanPhamIdServer is > 0);

            while (soSanPhamCuaSeller < YeuCauDuLieuSeed.SoSanPhamToiThieuMoiSeller)
            {
                var token = await LayTokenSeedAsync(seller.TaiKhoan, $"tạo sản phẩm seed cho tài khoản seller {seller.TaiKhoan.SoThuTu}");

                var shipFromId = IdChoBody(seller.DiaChi!.DiaChiIdServer);
                var categoryId = IdChoBody(danhMuc.DanhMucIdServer);
                if (shipFromId is null || categoryId is null)
                {
                    break;
                }

                var gia = 100000m + seller.TaiKhoan.SoThuTu * 1000m + soSanPhamCuaSeller * 100m;
                var tenSanPham = $"San pham seed {seller.TaiKhoan.SoThuTu}-{soSanPhamCuaSeller + 1}";
                var body = new Dictionary<string, object?>
                {
                    ["title"] = tenSanPham,
                    ["price"] = gia,
                    ["description"] = "San pham moi dung cho kiem thu API.",
                    ["category_id"] = categoryId,
                    ["ship_from_id"] = shipFromId,
                    ["variants"] = new[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["size"] = "M",
                            ["stock"] = 100,
                            ["color"] = "Do",
                            ["weight"] = 500
                        }
                    }
                };

                var brandId = IdChoBody(thuongHieu?.ThuongHieuIdServer);
                if (brandId is not null)
                {
                    body["brand_id"] = brandId;
                }

                var response = await GuiApiSeedAsync(
                    HttpMethod.Post,
                    "/api/add_product",
                    body,
                    token,
                    $"tạo sản phẩm seed cho tài khoản seller {seller.TaiKhoan.SoThuTu}");

                var sanPhamIdServer = DocIdSau(response.Data, "id");
                if (sanPhamIdServer is not > 0)
                {
                    throw new LoiChuanBiKiemThuException(
                        $"API /api/add_product trả thành công nhưng không có id sản phẩm cho tài khoản seed {seller.TaiKhoan.SoThuTu}. Response: {RutGon(response.NoiDungRaw)}");
                }

                _nguCanh.CapNhatDB.DuLieu.SanPhamSeed.Add(new SanPhamSeed
                {
                    ThuTuNoiBo = IdTiepTheo(_nguCanh.CapNhatDB.DuLieu.SanPhamSeed, x => x.ThuTuNoiBo),
                    SanPhamIdServer = sanPhamIdServer,
                    TaiKhoanIdServer = seller.TaiKhoan.TaiKhoanIdServer,
                    DanhMucIdServer = danhMuc.DanhMucIdServer,
                    ThuongHieuIdServer = thuongHieu?.ThuongHieuIdServer,
                    DiaChiGuiHangIdServer = seller.DiaChi.DiaChiIdServer,
                    TenSanPham = response.Data?["name"]?.ToString() ?? tenSanPham,
                    Gia = gia,
                    TrangThai = "san_sang",
                    TaoBoiTest = true,
                    TaoLuc = DateTimeOffset.Now,
                    XacMinhLuc = DateTimeOffset.Now,
                    GhiChu = "Tạo bằng API /api/add_product"
                });
                soSanPhamCuaSeller++;
                coThayDoi = true;
            }
        }

        if (coThayDoi)
        {
            await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.SanPham);
        }
    }

    private async Task TaoLikeSanPhamSeedAsync()
    {
        var sanPham = _nguCanh.CapNhatDB.DuLieu.SanPhamSeed
            .Where(x => x.TrangThai == "san_sang" && x.SanPhamIdServer is > 0)
            .ToList();
        var taiKhoan = LayTaiKhoanDaDangKySanSang();
        if (sanPham.Count == 0 || taiKhoan.Count == 0)
        {
            return;
        }

        var daThich = _nguCanh.CapNhatDB.DuLieu.TaiKhoanThichSanPhamSeed
            .Where(x => x.TaiKhoanIdServer is > 0 &&
                        x.SanPhamIdServer is > 0)
            .Select(x => (TaiKhoanIdServer: x.TaiKhoanIdServer!.Value, SanPhamIdServer: x.SanPhamIdServer!.Value))
            .ToHashSet();

        var coThayDoi = false;
        foreach (var sp in sanPham)
        {
            if (_nguCanh.CapNhatDB.DuLieu.TaiKhoanThichSanPhamSeed.Count(x =>
                    x.TaiKhoanIdServer is > 0 &&
                    x.SanPhamIdServer is > 0) >= YeuCauDuLieuSeed.SoLikeSanPhamMucTieu)
            {
                break;
            }

            var nguoiThich = taiKhoan.FirstOrDefault(x =>
                x.TaiKhoanIdServer != sp.TaiKhoanIdServer &&
                x.TaiKhoanIdServer is > 0 &&
                sp.SanPhamIdServer is > 0 &&
                !daThich.Contains((x.TaiKhoanIdServer, sp.SanPhamIdServer.Value)) &&
                !CoQuanHeChan(x.TaiKhoanIdServer, sp.TaiKhoanIdServer));
            if (nguoiThich is null)
            {
                continue;
            }

            var token = await LayTokenSeedAsync(nguoiThich, $"tạo like sản phẩm seed cho tài khoản {nguoiThich.SoThuTu}");
            var productId = IdChoBody(sp.SanPhamIdServer);
            if (productId is null)
            {
                continue;
            }

            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/api/like_product",
                new Dictionary<string, object?> { ["product_id"] = productId },
                token,
                $"tạo like sản phẩm seed cho tài khoản {nguoiThich.SoThuTu}");

            var daLikeTheoResponse = DocBoolTuNode(response.Data?["is_liked"]);
            if (daLikeTheoResponse == false)
            {
                continue;
            }

            _nguCanh.CapNhatDB.DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
            {
                ThichSanPhamSeedId = IdTiepTheo(_nguCanh.CapNhatDB.DuLieu.TaiKhoanThichSanPhamSeed, x => x.ThichSanPhamSeedId),
                TaiKhoanIdServer = nguoiThich.TaiKhoanIdServer,
                SanPhamIdServer = sp.SanPhamIdServer,
                ThichLuc = DateTimeOffset.Now,
                GhiChu = "Tạo bằng API /api/like_product"
            });
            daThich.Add((nguoiThich.TaiKhoanIdServer, sp.SanPhamIdServer!.Value));
            coThayDoi = true;
        }

        if (coThayDoi)
        {
            await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.ThichSanPham);
        }
    }
}
