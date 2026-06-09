using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;

namespace HactechTest.ApiShopTesting.Seed;

public sealed class ChuanBiDuLieuMoi
{
    private const int SoQuanHeChinhToiThieu = 2;
    private const int SoQuanHeChinhMucTieu = 3;
    private const int SoDiaChiTaiKhoanMucTieu = 50;
    private const int SoSellerSanPhamMucTieu = 20;
    private const int SoSanPhamToiThieuMoiSeller = 3;
    private const int SoLikeSanPhamMucTieu = 5;
    private const int SoTinNhanMucTieu = 5;

    private readonly NguCanhKiemThu _nguCanh;
    private readonly int _soTaiKhoanDangKyToiThieu;

    public ChuanBiDuLieuMoi(NguCanhKiemThu nguCanh, int soTaiKhoanDangKyToiThieu)
    {
        _nguCanh = nguCanh;
        _soTaiKhoanDangKyToiThieu = soTaiKhoanDangKyToiThieu;
    }

    public async Task ChuanBiAsync()
    {
        if (!ConThieuDuLieu())
        {
            await DongBoThongBaoSeedAsync();
            return;
        }

        try
        {
            if (ThieuTaiKhoanDaDangKy())
            {
                await DangKyTaiKhoanChuaDangKyAsync();
            }

            if (ThieuDanhMuc() || ThieuThuongHieu())
            {
                await DongBoDanhMucVaThuongHieuAsync();
            }

            if (ThieuDiaChiTaiKhoan())
            {
                await TaoDiaChiTaiKhoanSeedAsync();
            }

            if (ThieuChan())
            {
                await TaoChanSeedAsync();
            }

            if (ThieuTheoDoi())
            {
                await TaoTheoDoiSeedAsync();
            }

            if (ThieuTimKiem())
            {
                await TaoTimKiemSeedAsync();
            }

            if (ThieuSanPham())
            {
                await TaoSanPhamSeedAsync();
            }

            if (ThieuLikeSanPham())
            {
                await TaoLikeSanPhamSeedAsync();
            }

            if (ThieuTinNhan())
            {
                await TaoTinNhanSeedAsync();
            }

            await DongBoThongBaoSeedAsync();
        }
        catch (LoiChuanBiKiemThuException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"Không kết nối được API seed tại Base URL {_nguCanh.CauHinh.BaseUrl}. Chi tiết: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"API seed bị timeout sau {_nguCanh.CauHinh.TimeoutGiay} giây. Chi tiết: {ex.Message}");
        }
    }

    private bool ConThieuDuLieu() =>
        ThieuTaiKhoanDaDangKy() ||
        ThieuDanhMuc() ||
        ThieuDiaChiTaiKhoan() ||
        ThieuTheoDoi() ||
        ThieuTimKiem() ||
        ThieuChan() ||
        ThieuSanPham() ||
        ThieuLikeSanPham() ||
        ThieuTinNhan();

    private bool ThieuTaiKhoanDaDangKy()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed.Count(x =>
            !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer)) < _soTaiKhoanDangKyToiThieu;
    }

    private bool ThieuDiaChiTaiKhoan()
    {
        return _nguCanh.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.Count(x =>
            x.TrangThai == "san_sang" &&
            !string.IsNullOrWhiteSpace(x.DiaChiIdServer)) < SoDiaChiTaiKhoanMucTieu;
    }

    private bool ThieuDanhMuc()
    {
        return _nguCanh.KhoSeed.DuLieu.DanhMucSeed.Count(x => x.TrangThai == "san_sang") == 0;
    }

    private bool ThieuThuongHieu()
    {
        return _nguCanh.KhoSeed.DuLieu.ThuongHieuSeed.Count(x => x.TrangThai == "san_sang") == 0;
    }

    private bool ThieuSanPham()
    {
        var soSellerDuSanPham = _nguCanh.KhoSeed.DuLieu.SanPhamSeed
            .Where(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.SanPhamIdServer))
            .GroupBy(x => x.TaiKhoanIdServer)
            .Count(x => x.Count() >= SoSanPhamToiThieuMoiSeller);

        return soSellerDuSanPham < SoSellerSanPhamMucTieu;
    }

    private bool ThieuLikeSanPham()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Count(x =>
            x.TrangThai == "san_sang" &&
            !string.IsNullOrWhiteSpace(x.SanPhamIdServer)) < SoLikeSanPhamMucTieu;
    }

    private bool ThieuTinNhan()
    {
        return _nguCanh.KhoSeed.DuLieu.TinNhanSeed.Count(x => x.TrangThai == "da_gui") < SoTinNhanMucTieu;
    }

    private bool ThieuTheoDoi()
    {
        return DemQuanHeTheoDoiCuaFollowerChinh() < SoQuanHeChinhToiThieu;
    }

    private bool ThieuTimKiem()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Count(x => x.TrangThai == "dang_luu") < 2;
    }

    private bool ThieuChan()
    {
        return DemQuanHeChanCuaBlockerChinh() < SoQuanHeChinhToiThieu;
    }

    private int DemQuanHeTheoDoiCuaFollowerChinh()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .GroupBy(x => x.FollowerTaiKhoanIdServer)
            .Select(x => x.Count())
            .DefaultIfEmpty(0)
            .Max();
    }

    private int DemQuanHeChanCuaBlockerChinh()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .GroupBy(x => x.BlockerTaiKhoanIdServer)
            .Select(x => x.Count())
            .DefaultIfEmpty(0)
            .Max();
    }

    private async Task DangKyTaiKhoanChuaDangKyAsync()
    {
        var daDangKy = _nguCanh.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed.Count(x =>
            !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer));
        var canDangKy = Math.Max(0, _soTaiKhoanDangKyToiThieu - daDangKy);

        for (var i = 0; i < canDangKy; i++)
        {
            var taiKhoan = _nguCanh.KhoSeed.LayTaiKhoanChuaDangKy();
            if (taiKhoan is null)
            {
                throw new LoiChuanBiKiemThuException(
                    $"Không còn tài khoản chưa đăng ký trong taikhoan_seed để chuẩn bị đủ {_soTaiKhoanDangKyToiThieu} tài khoản mồi.");
            }

            var matKhauDungDeDangKy = taiKhoan.MatKhauHienTai;
            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/auth/signup",
                NguCanhKiemThu.TaoBodyDangKy(taiKhoan),
                mucDich: $"đăng ký tài khoản seed {taiKhoan.SoThuTu} ({taiKhoan.SoDienThoai})");

            var taiKhoanDaDangKy = NguCanhKiemThu.TaoTaiKhoanSauDangKy(taiKhoan, response.Data, matKhauDungDeDangKy);
            if (string.IsNullOrWhiteSpace(taiKhoanDaDangKy.TaiKhoanIdServer))
            {
                throw new LoiChuanBiKiemThuException(
                    $"API /auth/signup trả thành công nhưng không có tk_id_server cho tài khoản seed {taiKhoan.SoThuTu}. Response: {TienIchJson.RutGon(response.NoiDungRaw)}");
            }

            _nguCanh.KhoSeed.DuLieu.TaiKhoanChuaDangKySeed.Remove(taiKhoan);
            _nguCanh.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed.Add(taiKhoanDaDangKy);
            await _nguCanh.KhoSeed.LuuAsync();
        }
    }

    private async Task<string> LayTokenSeedAsync(TaiKhoanSignupThanhCongSeed taiKhoan, string mucDich)
    {
        var body = new Dictionary<string, object?>
        {
            ["phone_number"] = taiKhoan.SoDienThoai,
            ["password"] = taiKhoan.MatKhauHienTai,
            ["devtoken"] = taiKhoan.UuidThietBi
        };

        var response = await GuiApiSeedAsync(HttpMethod.Post, "/auth/login", body, mucDich: mucDich);
        var token = TienIchJson.DocChuoi(response.Data, "token", "access_token", "jwt_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new LoiChuanBiKiemThuException(
                $"API /auth/login trả thành công nhưng không có token khi {mucDich}. Response: {TienIchJson.RutGon(response.NoiDungRaw)}");
        }

        return token;
    }

    private async Task<PhanHoiApi> GuiApiSeedAsync(
        HttpMethod phuongThuc,
        string duongDan,
        object? body = null,
        string? token = null,
        string mucDich = "chuẩn bị dữ liệu seed")
    {
        try
        {
            var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(phuongThuc, duongDan, body, token));
            if (response.MaSoSanh != "1000" &&
                !(duongDan == "/api/get_list_brands" && response.MaSoSanh == "9994"))
            {
                throw TaoLoiApiSeed(duongDan, response, mucDich);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"Không kết nối được API {duongDan} khi {mucDich}. Base URL: {_nguCanh.CauHinh.BaseUrl}. Chi tiết: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"API {duongDan} bị timeout sau {_nguCanh.CauHinh.TimeoutGiay} giây khi {mucDich}. Chi tiết: {ex.Message}");
        }
    }

    private static LoiChuanBiKiemThuException TaoLoiApiSeed(string duongDan, PhanHoiApi response, string mucDich)
    {
        var message = string.IsNullOrWhiteSpace(response.Message) ? "(không có message)" : response.Message;
        var raw = TienIchJson.RutGon(response.NoiDungRaw);
        return new LoiChuanBiKiemThuException(
            $"API {duongDan} không thành công khi {mucDich}. HTTP {(int)response.HttpStatusCode}, code {response.MaSoSanh}, message: {message}. Response: {raw}");
    }

    private async Task DongBoDanhMucVaThuongHieuAsync()
    {
        await DongBoDanhMucAsync();
        await DongBoThuongHieuAsync();
        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task DongBoDanhMucAsync()
    {
        var response = await GuiApiSeedAsync(
            HttpMethod.Post,
            "/api/get_categories",
            new Dictionary<string, object?>(),
            mucDich: "đồng bộ danh mục seed");

        foreach (var item in LayObjectDeQuy(response.Data))
        {
            var id = TienIchJson.DocChuoi(item, "id", "category_id", "dm_id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var ten = TienIchJson.DocChuoi(item, "name", "category_name", "title") ?? $"Danh muc {id}";
            var hienCo = _nguCanh.KhoSeed.DuLieu.DanhMucSeed.FirstOrDefault(x => x.DanhMucIdServer == id);
            if (hienCo is null)
            {
                _nguCanh.KhoSeed.DuLieu.DanhMucSeed.Add(new DanhMucSeed
                {
                    DanhMucIdServer = id,
                    TenDanhMuc = ten,
                    DanhMucChaIdServer = TienIchJson.DocChuoi(item, "parent_id", "parentId"),
                    CoDanhMucCon = TienIchJson.DocBool(item, "has_child", "hasChild"),
                    CoThuongHieu = TienIchJson.DocBool(item, "has_brand", "hasBrand"),
                    CoKichCo = TienIchJson.DocBool(item, "has_size", "hasSize"),
                    YeuCauCanNang = TienIchJson.DocBool(item, "require_weight", "requireWeight"),
                    TrangThai = "san_sang",
                    DongBoLuc = DateTimeOffset.Now
                });
            }
            else
            {
                hienCo.TenDanhMuc = ten;
                hienCo.DanhMucChaIdServer = TienIchJson.DocChuoi(item, "parent_id", "parentId");
                hienCo.CoDanhMucCon = TienIchJson.DocBool(item, "has_child", "hasChild");
                hienCo.CoThuongHieu = TienIchJson.DocBool(item, "has_brand", "hasBrand");
                hienCo.CoKichCo = TienIchJson.DocBool(item, "has_size", "hasSize");
                hienCo.YeuCauCanNang = TienIchJson.DocBool(item, "require_weight", "requireWeight");
                hienCo.TrangThai = "san_sang";
                hienCo.DongBoLuc = DateTimeOffset.Now;
            }
        }
    }

    private async Task DongBoThuongHieuAsync()
    {
        var body = new Dictionary<string, object?> { ["category_id"] = 0, ["index"] = 0, ["count"] = 1000 };
        var response = await GuiApiSeedAsync(
            HttpMethod.Post,
            "/api/get_list_brands",
            body,
            mucDich: "đồng bộ thương hiệu seed");

        foreach (var item in LayObjectTuNode(response.Data))
        {
            var id = TienIchJson.DocChuoi(item, "id", "brand_id", "thuonghieu_id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var ten = TienIchJson.DocChuoi(item, "brand_name", "name", "title") ?? $"Thuong hieu {id}";
            var hienCo = _nguCanh.KhoSeed.DuLieu.ThuongHieuSeed.FirstOrDefault(x => x.ThuongHieuIdServer == id);
            if (hienCo is null)
            {
                _nguCanh.KhoSeed.DuLieu.ThuongHieuSeed.Add(new ThuongHieuSeed
                {
                    ThuongHieuIdServer = id,
                    TenThuongHieu = ten,
                    DanhMucIdServer = TienIchJson.DocChuoi(item, "category_id", "dm_id_server"),
                    TrangThai = "san_sang",
                    DongBoLuc = DateTimeOffset.Now
                });
            }
            else
            {
                hienCo.TenThuongHieu = ten;
                hienCo.DanhMucIdServer = TienIchJson.DocChuoi(item, "category_id", "dm_id_server");
                hienCo.TrangThai = "san_sang";
                hienCo.DongBoLuc = DateTimeOffset.Now;
            }
        }
    }

    private async Task TaoDiaChiTaiKhoanSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        var diaChiSanSang = _nguCanh.KhoSeed.DuLieu.DiaChiTaiKhoanSeed
            .Where(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.DiaChiIdServer))
            .Select(x => x.TaiKhoanIdServer)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet();
        var wards = _nguCanh.KhoSeed.DuLieu.PhuongXaSeed.OrderBy(x => x.PhuongXaId).ToList();

        if (taiKhoanDaDangKy.Count == 0 || wards.Count == 0)
        {
            return;
        }

        for (var i = 0; i < taiKhoanDaDangKy.Count && diaChiSanSang.Count < SoDiaChiTaiKhoanMucTieu; i++)
        {
            var taiKhoan = taiKhoanDaDangKy[i];
            if (string.IsNullOrWhiteSpace(taiKhoan.TaiKhoanIdServer) || diaChiSanSang.Contains(taiKhoan.TaiKhoanIdServer))
            {
                continue;
            }

            var ward = wards[i % wards.Count];
            var province = _nguCanh.KhoSeed.DuLieu.TinhThanhSeed.FirstOrDefault(x => x.TinhThanhId == ward.TinhThanhId);
            if (province is null)
            {
                continue;
            }

            var token = await LayTokenSeedAsync(taiKhoan, $"tạo địa chỉ seed cho tài khoản {taiKhoan.SoThuTu}");

            var diaChiChiTiet = $"So {taiKhoan.SoThuTu}, duong Seed";
            var diaChi = $"{diaChiChiTiet}, {ward.TenPhuongXa}, {province.TenTinhThanh}";
            var body = new Dictionary<string, object?>
            {
                ["address"] = diaChi,
                ["is_default"] = true,
                ["address_id"] = new[] { ward.PhuongXaId, province.TinhThanhId },
                ["lat"] = 21.0m + taiKhoan.SoThuTu / 10000m,
                ["lng"] = 105.8m + taiKhoan.SoThuTu / 10000m,
                ["receiver_name"] = $"Seed User {taiKhoan.SoThuTu}",
                ["phone"] = taiKhoan.SoDienThoai,
                ["full_address"] = diaChi,
                ["address_detail"] = diaChiChiTiet
            };

            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/order/add_order_address",
                body,
                token,
                $"tạo địa chỉ seed cho tài khoản {taiKhoan.SoThuTu}");

            var diaChiIdServer = DocChuoiSau(response.Data, "id", "address_id", "diachi_id");
            if (string.IsNullOrWhiteSpace(diaChiIdServer))
            {
                throw new LoiChuanBiKiemThuException(
                    $"API /order/add_order_address trả thành công nhưng không có id địa chỉ cho tài khoản seed {taiKhoan.SoThuTu}. Response: {TienIchJson.RutGon(response.NoiDungRaw)}");
            }

            _nguCanh.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
            {
                DiaChiSeedId = IdTiepTheo(_nguCanh.KhoSeed.DuLieu.DiaChiTaiKhoanSeed, x => x.DiaChiSeedId),
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                DiaChiIdServer = diaChiIdServer,
                PhuongXaId = ward.PhuongXaId,
                TinhThanhId = province.TinhThanhId,
                DiaChi = diaChi,
                DiaChiDayDu = diaChi,
                DiaChiChiTiet = diaChiChiTiet,
                ViDo = (decimal)body["lat"]!,
                KinhDo = (decimal)body["lng"]!,
                TenNguoiNhan = $"Seed User {taiKhoan.SoThuTu}",
                SoDienThoaiNguoiNhan = taiKhoan.SoDienThoai,
                LaMacDinh = true,
                MucDichSeed = "ca_hai",
                TrangThai = "san_sang",
                TaoLuc = DateTimeOffset.Now,
                XacMinhLuc = DateTimeOffset.Now,
                GhiChu = "Tao bang API /order/add_order_address"
            });
            diaChiSanSang.Add(taiKhoan.TaiKhoanIdServer);
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoSanPhamSeedAsync()
    {
        if (ThieuDanhMuc())
        {
            await DongBoDanhMucVaThuongHieuAsync();
        }

        if (ThieuDiaChiTaiKhoan())
        {
            await TaoDiaChiTaiKhoanSeedAsync();
        }

        var danhMuc = _nguCanh.KhoSeed.DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang");
        if (danhMuc is null)
        {
            return;
        }

        var sellerCoDiaChi = LayTaiKhoanDaDangKySanSang()
            .Select(tk => new
            {
                TaiKhoan = tk,
                DiaChi = _nguCanh.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(dc =>
                    dc.TaiKhoanIdServer == tk.TaiKhoanIdServer &&
                    dc.TrangThai == "san_sang" &&
                    !string.IsNullOrWhiteSpace(dc.DiaChiIdServer))
            })
            .Where(x => x.DiaChi is not null)
            .Take(SoSellerSanPhamMucTieu)
            .ToList();
        if (sellerCoDiaChi.Count == 0)
        {
            return;
        }

        var thuongHieu = _nguCanh.KhoSeed.DuLieu.ThuongHieuSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" &&
            !string.IsNullOrWhiteSpace(x.ThuongHieuIdServer));
        foreach (var seller in sellerCoDiaChi)
        {
            var soSanPhamCuaSeller = _nguCanh.KhoSeed.DuLieu.SanPhamSeed.Count(x =>
                x.TaiKhoanIdServer == seller.TaiKhoan.TaiKhoanIdServer &&
                x.TrangThai == "san_sang" &&
                !string.IsNullOrWhiteSpace(x.SanPhamIdServer));

            while (soSanPhamCuaSeller < SoSanPhamToiThieuMoiSeller)
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

                var sanPhamIdServer = DocChuoiSau(response.Data, "id", "product_id", "sp_id");
                if (string.IsNullOrWhiteSpace(sanPhamIdServer))
                {
                    throw new LoiChuanBiKiemThuException(
                        $"API /api/add_product trả thành công nhưng không có id sản phẩm cho tài khoản seed {seller.TaiKhoan.SoThuTu}. Response: {TienIchJson.RutGon(response.NoiDungRaw)}");
                }

                _nguCanh.KhoSeed.DuLieu.SanPhamSeed.Add(new SanPhamSeed
                {
                    ThuTuNoiBo = IdTiepTheo(_nguCanh.KhoSeed.DuLieu.SanPhamSeed, x => x.ThuTuNoiBo),
                    SanPhamIdServer = sanPhamIdServer,
                    TaiKhoanIdServer = seller.TaiKhoan.TaiKhoanIdServer,
                    DanhMucIdServer = danhMuc.DanhMucIdServer,
                    ThuongHieuIdServer = thuongHieu?.ThuongHieuIdServer,
                    DiaChiGuiHangIdServer = seller.DiaChi.DiaChiIdServer,
                    TenSanPham = TienIchJson.DocChuoi(response.Data, "title", "name") ?? tenSanPham,
                    Gia = gia,
                    TrangThai = "san_sang",
                    TaoBoiTest = true,
                    TaoLuc = DateTimeOffset.Now,
                    XacMinhLuc = DateTimeOffset.Now,
                    GhiChu = "Tao bang API /api/add_product"
                });
                soSanPhamCuaSeller++;
            }
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoLikeSanPhamSeedAsync()
    {
        if (ThieuSanPham())
        {
            await TaoSanPhamSeedAsync();
        }

        var sanPham = _nguCanh.KhoSeed.DuLieu.SanPhamSeed
            .Where(x => x.TrangThai == "san_sang" && !string.IsNullOrWhiteSpace(x.SanPhamIdServer))
            .ToList();
        var taiKhoan = LayTaiKhoanDaDangKySanSang();
        if (sanPham.Count == 0 || taiKhoan.Count == 0)
        {
            return;
        }

        var daThich = _nguCanh.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed
            .Where(x => x.TrangThai == "san_sang" &&
                        !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer) &&
                        !string.IsNullOrWhiteSpace(x.SanPhamIdServer))
            .Select(x => (TaiKhoanIdServer: x.TaiKhoanIdServer!, SanPhamIdServer: x.SanPhamIdServer!))
            .ToHashSet();

        foreach (var sp in sanPham)
        {
            if (_nguCanh.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Count(x => x.TrangThai == "san_sang") >= SoLikeSanPhamMucTieu)
            {
                break;
            }

            var nguoiThich = taiKhoan.FirstOrDefault(x =>
                x.TaiKhoanIdServer != sp.TaiKhoanIdServer &&
                !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer) &&
                !string.IsNullOrWhiteSpace(sp.SanPhamIdServer) &&
                !daThich.Contains((x.TaiKhoanIdServer, sp.SanPhamIdServer)) &&
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

            var daLikeTheoResponse = TienIchJson.DocBool(response.Data, "is_liked", "liked");
            if (daLikeTheoResponse == false)
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
            {
                ThichSanPhamSeedId = IdTiepTheo(_nguCanh.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed, x => x.ThichSanPhamSeedId),
                TaiKhoanIdServer = nguoiThich.TaiKhoanIdServer,
                SanPhamIdServer = sp.SanPhamIdServer,
                ThichLuc = DateTimeOffset.Now,
                TrangThai = "san_sang",
                GhiChu = "Tao bang API /api/like_product"
            });
            daThich.Add((nguoiThich.TaiKhoanIdServer!, sp.SanPhamIdServer!));
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoTinNhanSeedAsync()
    {
        var taiKhoan = LayTaiKhoanDaDangKySanSang();
        if (taiKhoan.Count < 2)
        {
            return;
        }

        var tokenCache = new Dictionary<int, string>();
        for (var i = 0; i < taiKhoan.Count && _nguCanh.KhoSeed.DuLieu.TinNhanSeed.Count(x => x.TrangThai == "da_gui") < SoTinNhanMucTieu; i++)
        {
            var sender = taiKhoan[i];
            var receiver = taiKhoan.FirstOrDefault(x =>
                x.SoThuTu != sender.SoThuTu &&
                !CoQuanHeChan(sender.TaiKhoanIdServer, x.TaiKhoanIdServer));
            if (receiver is null || string.IsNullOrWhiteSpace(receiver.TaiKhoanIdServer))
            {
                continue;
            }

            if (!tokenCache.TryGetValue(sender.SoThuTu, out var token))
            {
                token = await LayTokenSeedAsync(sender, $"tạo tin nhắn seed từ tài khoản {sender.SoThuTu}");

                tokenCache[sender.SoThuTu] = token;
            }

            var noiDung = $"Tin nhan seed {DateTimeOffset.Now:yyyyMMddHHmmssfff}";
            var body = new Dictionary<string, object?>
            {
                ["to_id"] = IdChoBody(receiver.TaiKhoanIdServer),
                ["message"] = noiDung,
                ["type_message"] = "text"
            };

            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/conversation/send_message",
                body,
                token,
                $"tạo tin nhắn seed từ tài khoản {sender.SoThuTu} tới {receiver.SoThuTu}");

            var conversationId = DocChuoiSau(response.Data, "conversation_id", "conversationId");
            var messageId = DocChuoiSau(response.Data, "message_id", "messageId", "id");
            if (string.IsNullOrWhiteSpace(conversationId) && string.IsNullOrWhiteSpace(messageId))
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.TinNhanSeed.Add(new TinNhanSeed
            {
                TinNhanSeedId = IdTiepTheo(_nguCanh.KhoSeed.DuLieu.TinNhanSeed, x => x.TinNhanSeedId),
                ConversationIdServer = conversationId,
                MessageIdServer = messageId,
                SenderTaiKhoanIdServer = sender.TaiKhoanIdServer,
                ReceiverTaiKhoanIdServer = receiver.TaiKhoanIdServer,
                TypeMessage = "text",
                NoiDung = noiDung,
                TrangThai = "da_gui",
                TaoBoiTest = true,
                GuiLuc = DateTimeOffset.Now,
                GhiChu = "Tao bang API /conversation/send_message"
            });
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task DongBoThongBaoSeedAsync()
    {
        try
        {
            var taiKhoan = LayTaiKhoanDaDangKySanSang().Take(10).ToList();
            foreach (var tk in taiKhoan)
            {
                var token = await _nguCanh.DangNhapLayTokenAsync(tk);
                if (token is null)
                {
                    continue;
                }

                var body = new Dictionary<string, object?> { ["index"] = 1, ["count"] = 20, ["group"] = 0 };
                var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/notification/get_notification", body, token));
                if (response.MaSoSanh != "1000")
                {
                    continue;
                }

                foreach (var item in LayObjectTuNode(response.Data))
                {
                    var notificationId = TienIchJson.DocChuoi(item, "id", "notification_id", "notificationId");
                    if (string.IsNullOrWhiteSpace(notificationId))
                    {
                        continue;
                    }

                    var daDoc = TienIchJson.DocBool(item, "read", "is_read", "seen", "da_doc");
                    var hienCo = _nguCanh.KhoSeed.DuLieu.ThongBaoSeed.FirstOrDefault(x => x.NotificationIdServer == notificationId);
                    if (hienCo is null)
                    {
                        _nguCanh.KhoSeed.DuLieu.ThongBaoSeed.Add(new ThongBaoSeed
                        {
                            NotificationIdServer = notificationId,
                            TaiKhoanIdServer = tk.TaiKhoanIdServer,
                            Title = TienIchJson.DocChuoi(item, "title"),
                            Content = TienIchJson.DocChuoi(item, "content", "message"),
                            ObjectIdServer = TienIchJson.DocChuoi(item, "object_id", "objectId", "target_id"),
                            NotificationType = TienIchJson.DocChuoi(item, "type", "notification_type"),
                            DaDoc = daDoc,
                            TrangThai = daDoc == true ? "da_doc" : "dang_luu",
                            LayLuc = DateTimeOffset.Now
                        });
                    }
                    else
                    {
                        hienCo.TaiKhoanIdServer = tk.TaiKhoanIdServer;
                        hienCo.Title = TienIchJson.DocChuoi(item, "title");
                        hienCo.Content = TienIchJson.DocChuoi(item, "content", "message");
                        hienCo.ObjectIdServer = TienIchJson.DocChuoi(item, "object_id", "objectId", "target_id");
                        hienCo.NotificationType = TienIchJson.DocChuoi(item, "type", "notification_type");
                        hienCo.DaDoc = daDoc;
                        hienCo.TrangThai = daDoc == true ? "da_doc" : "dang_luu";
                        hienCo.LayLuc = DateTimeOffset.Now;
                    }
                }
            }

            await _nguCanh.KhoSeed.LuuAsync();
        }
        catch (HttpRequestException)
        {
            return;
        }
        catch (TaskCanceledException)
        {
            return;
        }
    }

    private async Task TaoTimKiemSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        if (taiKhoanDaDangKy.Count == 0)
        {
            return;
        }

        var mucTieu = 2;
        var daCo = _nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Count(x => x.TrangThai == "dang_luu");
        for (var i = 0; i < taiKhoanDaDangKy.Count && daCo < mucTieu; i++)
        {
            var taiKhoan = taiKhoanDaDangKy[i];
            var token = await LayTokenSeedAsync(taiKhoan, $"tạo saved search seed cho tài khoản {taiKhoan.SoThuTu}");

            var keyword = $"search_seed_{taiKhoan.SoThuTu}_{DateTimeOffset.Now:yyyyMMddHHmmss}";
            var body = new Dictionary<string, object?> { ["keyword"] = keyword };
            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/api/save_search",
                body,
                token,
                $"tạo saved search seed cho tài khoản {taiKhoan.SoThuTu}");

            _nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
            {
                TaiKhoanTimKiemSeedId = IdTiepTheo(_nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed, x => x.TaiKhoanTimKiemSeedId),
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                SavedSearchIdServer = TienIchJson.DocChuoi(response.Data, "id", "saved_search_id"),
                Keyword = TienIchJson.DocChuoi(response.Data, "keyword") ?? keyword,
                TrangThai = "dang_luu",
                TaoBoiTest = true,
                TaoLuc = DateTimeOffset.Now
            });
            daCo++;
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoTheoDoiSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        if (taiKhoanDaDangKy.Count < 2)
        {
            return;
        }

        var seedDangTheoDoi = _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .ToList();
        var seedDangChan = _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .ToList();
        var blockerChinhId = seedDangChan
            .Where(x => !string.IsNullOrWhiteSpace(x.BlockerTaiKhoanIdServer))
            .GroupBy(x => x.BlockerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        var biChanTrongSeed = seedDangChan
            .Select(x => x.BlockedTaiKhoanIdServer)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet();
        var followerChinhId = seedDangTheoDoi
            .Where(x => !string.IsNullOrWhiteSpace(x.FollowerTaiKhoanIdServer))
            .GroupBy(x => x.FollowerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(followerChinhId) &&
            ((!string.IsNullOrWhiteSpace(blockerChinhId) && followerChinhId == blockerChinhId) ||
             biChanTrongSeed.Contains(followerChinhId)))
        {
            _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed.RemoveAll(x =>
                x.TrangThai == "dang_theo_doi" &&
                x.FollowerTaiKhoanIdServer == followerChinhId);
            seedDangTheoDoi = _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
                .Where(x => x.TrangThai == "dang_theo_doi")
                .ToList();
            followerChinhId = null;
        }

        var nguoiTheoDoi = !string.IsNullOrWhiteSpace(followerChinhId)
            ? taiKhoanDaDangKy.FirstOrDefault(x => x.TaiKhoanIdServer == followerChinhId)
            : taiKhoanDaDangKy.FirstOrDefault(x =>
                  (string.IsNullOrWhiteSpace(blockerChinhId) || x.TaiKhoanIdServer != blockerChinhId) &&
                  !biChanTrongSeed.Contains(x.TaiKhoanIdServer!))
              ?? taiKhoanDaDangKy.FirstOrDefault(x =>
                  string.IsNullOrWhiteSpace(blockerChinhId) || x.TaiKhoanIdServer != blockerChinhId)
              ?? taiKhoanDaDangKy[0];
        if (nguoiTheoDoi is null || string.IsNullOrWhiteSpace(nguoiTheoDoi.TaiKhoanIdServer))
        {
            return;
        }

        var followeeDaCo = seedDangTheoDoi
            .Where(x => x.FollowerTaiKhoanIdServer == nguoiTheoDoi.TaiKhoanIdServer)
            .Select(x => x.FolloweeTaiKhoanIdServer)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet();

        var token = await LayTokenSeedAsync(nguoiTheoDoi, $"tạo quan hệ follow seed cho tài khoản {nguoiTheoDoi.SoThuTu}");

        for (var i = 0; i < taiKhoanDaDangKy.Count && followeeDaCo.Count < SoQuanHeChinhMucTieu; i++)
        {
            var nguoiDuocTheoDoi = taiKhoanDaDangKy[i];
            if (nguoiTheoDoi.TaiKhoanIdServer == nguoiDuocTheoDoi.TaiKhoanIdServer ||
                string.IsNullOrWhiteSpace(nguoiDuocTheoDoi.TaiKhoanIdServer) ||
                followeeDaCo.Contains(nguoiDuocTheoDoi.TaiKhoanIdServer) ||
                biChanTrongSeed.Contains(nguoiDuocTheoDoi.TaiKhoanIdServer) ||
                CoQuanHeChan(nguoiTheoDoi.TaiKhoanIdServer, nguoiDuocTheoDoi.TaiKhoanIdServer))
            {
                continue;
            }

            var body = new Dictionary<string, object?>
            {
                ["followee_id"] = nguoiDuocTheoDoi.TaiKhoanIdServer,
                ["action"] = "follow"
            };

            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/set_user_follow",
                body,
                token,
                $"tạo quan hệ follow seed từ tài khoản {nguoiTheoDoi.SoThuTu} tới {nguoiDuocTheoDoi.SoThuTu}");

            _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
            {
                FollowerTaiKhoanIdServer = nguoiTheoDoi.TaiKhoanIdServer,
                FolloweeTaiKhoanIdServer = nguoiDuocTheoDoi.TaiKhoanIdServer,
                TheoDoiLuc = DateTimeOffset.Now,
                TrangThai = "dang_theo_doi"
            });
            followeeDaCo.Add(nguoiDuocTheoDoi.TaiKhoanIdServer);
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoChanSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        if (taiKhoanDaDangKy.Count < 2)
        {
            return;
        }

        var seedDangTheoDoi = _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .ToList();
        var capTheoDoi = seedDangTheoDoi
            .Where(x => !string.IsNullOrWhiteSpace(x.FollowerTaiKhoanIdServer) && !string.IsNullOrWhiteSpace(x.FolloweeTaiKhoanIdServer))
            .Select(x => (FollowerTaiKhoanIdServer: x.FollowerTaiKhoanIdServer!, FolloweeTaiKhoanIdServer: x.FolloweeTaiKhoanIdServer!))
            .ToHashSet();
        var seedDangChan = _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .ToList();
        var daCo = seedDangChan
            .Where(x => !string.IsNullOrWhiteSpace(x.BlockerTaiKhoanIdServer) && !string.IsNullOrWhiteSpace(x.BlockedTaiKhoanIdServer))
            .Select(x => (BlockerTaiKhoanIdServer: x.BlockerTaiKhoanIdServer!, BlockedTaiKhoanIdServer: x.BlockedTaiKhoanIdServer!))
            .ToHashSet();
        var followerChinhId = seedDangTheoDoi
            .Where(x => !string.IsNullOrWhiteSpace(x.FollowerTaiKhoanIdServer))
            .GroupBy(x => x.FollowerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        var blockerChinhId = seedDangChan
            .Where(x => !string.IsNullOrWhiteSpace(x.BlockerTaiKhoanIdServer))
            .GroupBy(x => x.BlockerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        var nguoiChan = !string.IsNullOrWhiteSpace(blockerChinhId)
            ? taiKhoanDaDangKy.FirstOrDefault(x => x.TaiKhoanIdServer == blockerChinhId)
            : taiKhoanDaDangKy.FirstOrDefault(x => string.IsNullOrWhiteSpace(followerChinhId) || x.TaiKhoanIdServer != followerChinhId);
        if (nguoiChan is null || string.IsNullOrWhiteSpace(nguoiChan.TaiKhoanIdServer))
        {
            return;
        }

        var daDonBlockRaiRac = false;
        for (var i = _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed.Count - 1; i >= 0; i--)
        {
            var item = _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed[i];
            if (item.TrangThai != "dang_chan" || item.BlockerTaiKhoanIdServer != nguoiChan.TaiKhoanIdServer)
            {
                _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed.RemoveAt(i);
                daDonBlockRaiRac = true;
            }
        }

        seedDangChan = _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan" && x.BlockerTaiKhoanIdServer == nguoiChan.TaiKhoanIdServer)
            .ToList();
        daCo = seedDangChan
            .Where(x => !string.IsNullOrWhiteSpace(x.BlockerTaiKhoanIdServer) && !string.IsNullOrWhiteSpace(x.BlockedTaiKhoanIdServer))
            .Select(x => (BlockerTaiKhoanIdServer: x.BlockerTaiKhoanIdServer!, BlockedTaiKhoanIdServer: x.BlockedTaiKhoanIdServer!))
            .ToHashSet();

        var biChanDaCo = seedDangChan
            .Where(x => x.BlockerTaiKhoanIdServer == nguoiChan.TaiKhoanIdServer)
            .Select(x => x.BlockedTaiKhoanIdServer)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet();
        var followeeCuaFollowerChinh = seedDangTheoDoi
            .Where(x => string.IsNullOrWhiteSpace(followerChinhId) || x.FollowerTaiKhoanIdServer == followerChinhId)
            .Select(x => x.FolloweeTaiKhoanIdServer)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet();

        for (var i = 0; i < taiKhoanDaDangKy.Count && biChanDaCo.Count < SoQuanHeChinhMucTieu; i++)
        {
            var nguoiBiChan = taiKhoanDaDangKy[i];
            if (nguoiChan.TaiKhoanIdServer == nguoiBiChan.TaiKhoanIdServer ||
                string.IsNullOrWhiteSpace(nguoiBiChan.TaiKhoanIdServer) ||
                biChanDaCo.Contains(nguoiBiChan.TaiKhoanIdServer) ||
                followeeCuaFollowerChinh.Contains(nguoiBiChan.TaiKhoanIdServer) ||
                daCo.Contains((nguoiChan.TaiKhoanIdServer, nguoiBiChan.TaiKhoanIdServer)) ||
                capTheoDoi.Contains((nguoiChan.TaiKhoanIdServer, nguoiBiChan.TaiKhoanIdServer)) ||
                capTheoDoi.Contains((nguoiBiChan.TaiKhoanIdServer, nguoiChan.TaiKhoanIdServer)))
            {
                continue;
            }

            if (await TaoQuanHeChanAsync(nguoiChan, nguoiBiChan, daCo))
            {
                biChanDaCo.Add(nguoiBiChan.TaiKhoanIdServer);
            }
        }

        if (daDonBlockRaiRac || biChanDaCo.Count > 0)
        {
            await _nguCanh.KhoSeed.LuuAsync();
        }
    }

    private async Task<bool> TaoQuanHeChanAsync(
        TaiKhoanSignupThanhCongSeed nguoiChan,
        TaiKhoanSignupThanhCongSeed nguoiBiChan,
        HashSet<(string BlockerTaiKhoanIdServer, string BlockedTaiKhoanIdServer)> daCo)
    {
        if (string.IsNullOrWhiteSpace(nguoiChan.TaiKhoanIdServer) ||
            string.IsNullOrWhiteSpace(nguoiBiChan.TaiKhoanIdServer) ||
            nguoiChan.TaiKhoanIdServer == nguoiBiChan.TaiKhoanIdServer ||
            daCo.Contains((nguoiChan.TaiKhoanIdServer, nguoiBiChan.TaiKhoanIdServer)))
        {
            return false;
        }

        var token = await LayTokenSeedAsync(nguoiChan, $"tạo quan hệ block seed cho tài khoản {nguoiChan.SoThuTu}");

        var body = new Dictionary<string, object?>
        {
            ["user_id"] = nguoiBiChan.TaiKhoanIdServer,
            ["type"] = 0
        };

        var response = await GuiApiSeedAsync(
            HttpMethod.Post,
            "/set_user_block",
            body,
            token,
            $"tạo quan hệ block seed từ tài khoản {nguoiChan.SoThuTu} tới {nguoiBiChan.SoThuTu}");

        _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
        {
            BlockerTaiKhoanIdServer = nguoiChan.TaiKhoanIdServer,
            BlockedTaiKhoanIdServer = nguoiBiChan.TaiKhoanIdServer,
            ChanLuc = DateTimeOffset.Now,
            TrangThai = "dang_chan"
        });
        daCo.Add((nguoiChan.TaiKhoanIdServer, nguoiBiChan.TaiKhoanIdServer));
        return true;
    }

    private bool CoQuanHeChan(string? tkIdA, string? tkIdB)
    {
        if (string.IsNullOrWhiteSpace(tkIdA) || string.IsNullOrWhiteSpace(tkIdB))
        {
            return false;
        }

        return _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed.Any(x =>
            x.TrangThai == "dang_chan" &&
            ((x.BlockerTaiKhoanIdServer == tkIdA && x.BlockedTaiKhoanIdServer == tkIdB) ||
             (x.BlockerTaiKhoanIdServer == tkIdB && x.BlockedTaiKhoanIdServer == tkIdA)));
    }

    private List<TaiKhoanSignupThanhCongSeed> LayTaiKhoanDaDangKySanSang()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer))
            .OrderBy(x => x.SoThuTu)
            .ToList();
    }

    private static IEnumerable<JsonObject> LayObjectTuNode(JsonNode? node)
    {
        if (node is JsonArray array)
        {
            return array.OfType<JsonObject>();
        }

        if (node is JsonObject obj)
        {
            foreach (var tenMang in new[] { "items", "rows", "list", "data", "results", "notifications", "brands", "categories" })
            {
                if (obj[tenMang] is JsonArray mang)
                {
                    return mang.OfType<JsonObject>();
                }
            }

            return [obj];
        }

        return [];
    }

    private static IEnumerable<JsonObject> LayObjectDeQuy(JsonNode? node)
    {
        foreach (var obj in LayObjectTuNode(node))
        {
            yield return obj;

            foreach (var tenMangCon in new[] { "children", "childs", "child", "sub_categories", "subCategories" })
            {
                if (obj[tenMangCon] is null)
                {
                    continue;
                }

                foreach (var con in LayObjectDeQuy(obj[tenMangCon]))
                {
                    yield return con;
                }
            }
        }
    }

    private static string? DocChuoiSau(JsonNode? node, params string[] tenTruong)
    {
        var trucTiep = TienIchJson.DocChuoi(node, tenTruong);
        if (!string.IsNullOrWhiteSpace(trucTiep))
        {
            return trucTiep;
        }

        if (node is not JsonObject obj)
        {
            return null;
        }

        foreach (var tenNodeCon in new[] { "product", "item", "address", "message", "notification" })
        {
            var tuNodeCon = TienIchJson.DocChuoi(obj[tenNodeCon], tenTruong);
            if (!string.IsNullOrWhiteSpace(tuNodeCon))
            {
                return tuNodeCon;
            }
        }

        return null;
    }

    private static object? IdChoBody(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return long.TryParse(id, out var so) ? so : id;
    }

    private static int IdTiepTheo<T>(IEnumerable<T> danhSach, Func<T, int> layId)
    {
        return danhSach.Select(layId).DefaultIfEmpty(0).Max() + 1;
    }
}


