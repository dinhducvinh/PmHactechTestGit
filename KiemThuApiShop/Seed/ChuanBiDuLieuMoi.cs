using System.Text.Json.Nodes;
using KiemThuApiShop.Core;

namespace KiemThuApiShop.Seed;

public sealed class ChuanBiDuLieuMoi
{
    private readonly NguCanhKiemThu _nguCanh;

    public ChuanBiDuLieuMoi(NguCanhKiemThu nguCanh)
    {
        _nguCanh = nguCanh;
    }

    public async Task ChuanBiAsync()
    {
        var duLieuConThieu = KiemTraDuLieuConThieu();

        if (duLieuConThieu.Count == 0)
        {
            Console.WriteLine("Dữ liệu mồi trong SQL Server đã đủ, bỏ qua bước chuẩn bị.");
            return;
        }

        Console.WriteLine("Dữ liệu mồi còn thiếu, chỉ gọi API để bổ sung các phần thiếu:");
        foreach (var item in duLieuConThieu)
        {
            Console.WriteLine($"- {item}");
        }

        try
        {
            if (ThieuTaiKhoanDaDangKy())
            {
                await DangKyTaiKhoanSeedAsync();
            }

            if (ThieuDanhMuc())
            {
                await DongBoDanhMucAsync();
            }

            if (ThieuThuongHieu())
            {
                await DongBoThuongHieuAsync();
            }

            if (ThieuDiaChi())
            {
                await TaoDiaChiSeedAsync();
            }

            if (ThieuSanPham())
            {
                await TaoSanPhamSeedAsync();
            }

            if (ThieuBinhLuan())
            {
                await TaoBinhLuanSeedAsync();
            }

            if (ThieuLike())
            {
                await TaoLikeSeedAsync();
            }

            if (ThieuTheoDoi())
            {
                await TaoTheoDoiSeedAsync();
            }

            if (ThieuChan())
            {
                await TaoChanSeedAsync();
            }

            var sauChuanBiConThieu = KiemTraDuLieuConThieu();
            if (sauChuanBiConThieu.Count > 0)
            {
                Console.WriteLine("Sau bước chuẩn bị vẫn còn thiếu dữ liệu mồi:");
                foreach (var item in sauChuanBiConThieu)
                {
                    Console.WriteLine($"- {item}");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Không kết nối được API để chuẩn bị seed: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Quá thời gian chờ khi chuẩn bị seed. Các case cần dữ liệu mồi có thể bị bỏ qua.");
        }
    }

    private List<string> KiemTraDuLieuConThieu()
    {
        var thieu = new List<string>();

        if (ThieuTaiKhoanDaDangKy())
        {
            thieu.Add($"Thiếu tài khoản đã đăng ký trong taikhoan_seed: cần tối thiểu {_nguCanh.CauHinh.SoTaiKhoanDangKyTruoc}.");
        }

        if (ThieuDanhMuc())
        {
            thieu.Add("Thiếu danh mục san_sang trong danhmuc_seed.");
        }

        if (ThieuThuongHieu())
        {
            thieu.Add("Thiếu thương hiệu san_sang trong thuonghieu_seed.");
        }

        if (ThieuDiaChi())
        {
            thieu.Add("Thiếu địa chỉ san_sang cho tài khoản đã đăng ký trong diachitk_seed.");
        }

        if (ThieuSanPham())
        {
            thieu.Add("Thiếu sản phẩm seed san_sang đủ các loại: co_anh, nhieu_variant, cho_update, cho_delete, cho_like_comment.");
        }

        if (ThieuBinhLuan())
        {
            thieu.Add("Thiếu sản phẩm có tối thiểu 6 bình luận san_sang trong binhluan_sp_seed.");
        }

        if (ThieuLike())
        {
            thieu.Add("Thiếu quan hệ like dang_like trong tk_thich_sanpham_seed.");
        }

        if (ThieuTheoDoi())
        {
            thieu.Add("Thiếu 10 quan hệ theo dõi dang_theo_doi trong tk_theodoi_seed.");
        }

        if (ThieuSoLuongChan())
        {
            thieu.Add("Thiếu 10 quan hệ chặn dang_chan trong tk_chan_seed.");
        }

        if (ThieuChanChoBinhLuanBiChan())
        {
            thieu.Add("Thiếu quan hệ seller chặn user cho case PRODUCT-COMMENT-SET-19 trong tk_chan_seed.");
        }

        return thieu;
    }

    private bool ThieuTaiKhoanDaDangKy()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed.Count(x =>
            x.TrangThai == "san_sang" &&
            x.TrangThaiDangKy == "da_dang_ky") < _nguCanh.CauHinh.SoTaiKhoanDangKyTruoc;
    }

    private bool ThieuDanhMuc()
    {
        return !_nguCanh.KhoSeed.DuLieu.DanhMucSeed.Any(x => x.TrangThai == "san_sang");
    }

    private bool ThieuThuongHieu()
    {
        return !_nguCanh.KhoSeed.DuLieu.ThuongHieuSeed.Any(x => x.TrangThai == "san_sang");
    }

    private bool ThieuDiaChi()
    {
        var soTaiKhoanDaDangKy = _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed.Count(x =>
            x.TrangThai == "san_sang" &&
            x.TrangThaiDangKy == "da_dang_ky");
        var soDiaChiCanCo = Math.Min(3, soTaiKhoanDaDangKy);

        return soDiaChiCanCo > 0 &&
            _nguCanh.KhoSeed.DuLieu.DiaChiTaiKhoanSeed
                .Where(x => x.TrangThai == "san_sang")
                .Select(x => x.TkSeedId)
                .Distinct()
                .Count() < soDiaChiCanCo;
    }

    private bool ThieuSanPham()
    {
        var loaiCanCo = new[] { "co_anh", "nhieu_variant", "cho_update", "cho_delete", "cho_like_comment" };
        return loaiCanCo.Any(loai => _nguCanh.KhoSeed.LaySanPhamTheoLoai(loai) is null);
    }

    private bool ThieuBinhLuan()
    {
        return !_nguCanh.KhoSeed.DuLieu.BinhLuanSanPhamSeed
            .Where(x => x.TrangThai == "san_sang")
            .GroupBy(x => x.SpId)
            .Any(x => x.Count() >= 6);
    }

    private bool ThieuLike()
    {
        return _nguCanh.KhoSeed.LayLikeBatKy() is null;
    }

    private bool ThieuTheoDoi()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed.Count(x => x.TrangThai == "dang_theo_doi") < 10;
    }

    private bool ThieuChan()
    {
        return ThieuSoLuongChan() || ThieuChanChoBinhLuanBiChan();
    }

    private bool ThieuSoLuongChan()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed.Count(x => x.TrangThai == "dang_chan") < 10;
    }

    private bool ThieuChanChoBinhLuanBiChan()
    {
        return _nguCanh.KhoSeed.LaySeedSellerChanUserChoBinhLuan() is null;
    }

    private async Task DangKyTaiKhoanSeedAsync()
    {
        var daDangKy = _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed.Count(x => x.TrangThaiDangKy == "da_dang_ky");
        var canDangKy = Math.Max(0, _nguCanh.CauHinh.SoTaiKhoanDangKyTruoc - daDangKy);

        for (var i = 0; i < canDangKy; i++)
        {
            var taiKhoan = await _nguCanh.DangKyTaiKhoanMoiAsync();
            if (taiKhoan is null)
            {
                Console.WriteLine("Chưa đăng ký đủ tài khoản seed vì API signup chưa trả thành công.");
                break;
            }
        }
    }

    private async Task DongBoDanhMucAsync()
    {
        var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/api/get_categories", new Dictionary<string, object?>()));
        if (response.MaSoSanh != "1000" || response.Data is not JsonArray mang)
        {
            return;
        }

        _nguCanh.KhoSeed.DuLieu.DanhMucSeed.Clear();
        foreach (var item in mang)
        {
            var id = TienIchJson.DocChuoi(item, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.DanhMucSeed.Add(new DanhMucSeed
            {
                DmId = id,
                TenDanhMuc = TienIchJson.DocChuoi(item, "name") ?? $"Danh mục {id}",
                DmChaId = TienIchJson.DocChuoi(item, "parent_id"),
                CoDanhMucCon = TienIchJson.DocBool(item, "has_child") ?? false,
                CoThuongHieu = TienIchJson.DocBool(item, "has_brand") ?? false,
                CoKichCo = TienIchJson.DocBool(item, "has_size") ?? false,
                YeuCauCanNang = TienIchJson.DocBool(item, "require_weight") ?? false,
                Sort = (int?)TienIchJson.DocSoNguyen(item, "sort"),
                TrangThai = "san_sang",
                DongBoLuc = DateTimeOffset.Now
            });
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task DongBoThuongHieuAsync()
    {
        var body = new Dictionary<string, object?> { ["category_id"] = 0, ["index"] = 0, ["count"] = 1000 };
        var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/api/get_list_brands", body));
        if (response.MaSoSanh != "1000" || response.Data is not JsonArray mang)
        {
            return;
        }

        _nguCanh.KhoSeed.DuLieu.ThuongHieuSeed.Clear();
        foreach (var item in mang)
        {
            var id = TienIchJson.DocChuoi(item, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.ThuongHieuSeed.Add(new ThuongHieuSeed
            {
                ThuongHieuId = id,
                TenThuongHieu = TienIchJson.DocChuoi(item, "brand_name", "name") ?? $"Thương hiệu {id}",
                DmId = TienIchJson.DocChuoi(item, "category_id", "dm_id"),
                TrangThai = "san_sang",
                DongBoLuc = DateTimeOffset.Now
            });
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoDiaChiSeedAsync()
    {
        var taiKhoanDaDangKy = _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed
            .Where(x => x.TrangThaiDangKy == "da_dang_ky")
            .Take(3)
            .ToList();

        foreach (var taiKhoan in taiKhoanDaDangKy)
        {
            if (_nguCanh.KhoSeed.LayDiaChiTheoTaiKhoan(taiKhoan.TkSeedId) is not null)
            {
                continue;
            }

            var token = await _nguCanh.DangNhapLayTokenAsync(taiKhoan);
            if (token is null)
            {
                continue;
            }

            var body = new Dictionary<string, object?>
            {
                ["receiver_name"] = $"Người nhận seed {taiKhoan.TkSeedId}",
                ["phone"] = taiKhoan.Sdt,
                ["full_address"] = $"Số {taiKhoan.TkSeedId}, phường test, Hà Nội",
                ["is_default"] = true,
                ["ward_id"] = 1,
                ["lat"] = 21.0278,
                ["lng"] = 105.8342,
                ["address_name"] = "Địa chỉ test",
                ["address_detail"] = "Dữ liệu mồi tự động"
            };

            var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/addresses/create", body, token));
            if (response.MaSoSanh != "1000")
            {
                continue;
            }

            var diaChiId = TienIchJson.DocChuoi(response.Data, "id", "address_id", "diachi_id");
            if (string.IsNullOrWhiteSpace(diaChiId))
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
            {
                TkSeedId = taiKhoan.TkSeedId,
                TkId = taiKhoan.TkId,
                DiaChiId = diaChiId,
                TenNguoiNhan = $"Người nhận seed {taiKhoan.TkSeedId}",
                Sdt = taiKhoan.Sdt,
                DiaChiDayDu = $"Số {taiKhoan.TkSeedId}, phường test, Hà Nội",
                TaoLuc = DateTimeOffset.Now
            });
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoSanPhamSeedAsync()
    {
        var danhMuc = _nguCanh.KhoSeed.LayDanhMucBatKy();
        if (danhMuc is null)
        {
            return;
        }

        var thuongHieu = _nguCanh.KhoSeed.LayThuongHieuBatKy();
        var cacLoai = new[] { "co_anh", "nhieu_variant", "cho_update", "cho_delete", "cho_like_comment" };

        foreach (var loai in cacLoai)
        {
            if (_nguCanh.KhoSeed.LaySanPhamTheoLoai(loai) is not null)
            {
                continue;
            }

            var seller = _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed
                .FirstOrDefault(x => x.TrangThaiDangKy == "da_dang_ky" && _nguCanh.KhoSeed.LayDiaChiTheoTaiKhoan(x.TkSeedId) is not null);
            if (seller is null)
            {
                return;
            }

            var diaChi = _nguCanh.KhoSeed.LayDiaChiTheoTaiKhoan(seller.TkSeedId);
            var token = await _nguCanh.DangNhapLayTokenAsync(seller);
            if (diaChi is null || token is null)
            {
                return;
            }

            var body = NguCanhKiemThu.TaoBodySanPhamHopLe(
                $"Sản phẩm seed {loai} {DateTimeOffset.Now:yyyyMMddHHmmss}",
                danhMuc,
                thuongHieu,
                diaChi,
                dungVideo: loai == "co_video",
                nhieuVariant: loai == "nhieu_variant");

            var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/api/add_product", body, token));
            if (response.MaSoSanh != "1000")
            {
                continue;
            }

            var spId = TienIchJson.DocChuoi(response.Data, "id", "product_id", "sp_id");
            if (string.IsNullOrWhiteSpace(spId))
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.SanPhamSeed.Add(new SanPhamSeed
            {
                SpSeedId = _nguCanh.KhoSeed.DuLieu.SanPhamSeed.Count + 1,
                SpId = spId,
                SellerTkSeedId = seller.TkSeedId,
                SellerTkId = seller.TkId,
                DmId = danhMuc.DmId,
                ThuongHieuId = thuongHieu?.ThuongHieuId,
                DiaChiId = diaChi.DiaChiId,
                TenSp = TienIchJson.DocChuoi(response.Data, "name", "title") ?? $"Sản phẩm seed {loai}",
                Gia = 150000,
                LoaiSeed = loai,
                TrangThai = "san_sang",
                XacMinhLuc = DateTimeOffset.Now
            });
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoBinhLuanSeedAsync()
    {
        if (_nguCanh.KhoSeed.LayBinhLuanBatKy() is not null)
        {
            return;
        }

        var sanPham = _nguCanh.KhoSeed.LaySanPhamTheoLoai("cho_like_comment") ?? _nguCanh.KhoSeed.LaySanPhamBatKy();
        if (sanPham is null)
        {
            return;
        }

        var taiKhoan = _nguCanh.KhoSeed.LayTaiKhoanKhac(sanPham.SellerTkSeedId) ?? _nguCanh.KhoSeed.LayTaiKhoanDaDangKy();
        if (taiKhoan is null)
        {
            return;
        }

        var token = await _nguCanh.DangNhapLayTokenAsync(taiKhoan);
        if (token is null)
        {
            return;
        }

        for (var i = 1; i <= 6; i++)
        {
            if (!long.TryParse(sanPham.SpId, out var spId))
            {
                return;
            }

            var noiDung = $"Bình luận seed {i} cho sản phẩm {sanPham.SpId}";
            var body = new Dictionary<string, object?>
            {
                ["product_id"] = spId,
                ["content"] = noiDung,
                ["index"] = 0,
                ["count"] = 10
            };

            var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/api/set_comments_product", body, token));
            if (response.MaSoSanh != "1000")
            {
                break;
            }

            _nguCanh.KhoSeed.DuLieu.BinhLuanSanPhamSeed.Add(new BinhLuanSanPhamSeed
            {
                BlSeedId = _nguCanh.KhoSeed.DuLieu.BinhLuanSanPhamSeed.Count + 1,
                SpSeedId = sanPham.SpSeedId,
                SpId = sanPham.SpId,
                TkSeedId = taiKhoan.TkSeedId,
                TkId = taiKhoan.TkId,
                BlId = TienIchJson.DocChuoi(response.Data, "id", "comment_id"),
                NoiDung = noiDung,
                TaoLuc = DateTimeOffset.Now
            });
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoLikeSeedAsync()
    {
        if (_nguCanh.KhoSeed.LayLikeBatKy() is not null)
        {
            return;
        }

        var sanPham = _nguCanh.KhoSeed.LaySanPhamTheoLoai("cho_like_comment") ?? _nguCanh.KhoSeed.LaySanPhamBatKy();
        if (sanPham is null)
        {
            return;
        }

        var taiKhoan = _nguCanh.KhoSeed.LayTaiKhoanKhac(sanPham.SellerTkSeedId) ?? _nguCanh.KhoSeed.LayTaiKhoanDaDangKy();
        if (taiKhoan is null)
        {
            return;
        }

        var token = await _nguCanh.DangNhapLayTokenAsync(taiKhoan);
        if (token is null)
        {
            return;
        }

        if (!long.TryParse(sanPham.SpId, out var spId))
        {
            return;
        }

        var body = new Dictionary<string, object?> { ["product_id"] = spId };
        var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/api/like_product", body, token));
        if (response.MaSoSanh != "1000")
        {
            return;
        }

        _nguCanh.KhoSeed.DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
        {
            TkSeedId = taiKhoan.TkSeedId,
            TkId = taiKhoan.TkId,
            SpSeedId = sanPham.SpSeedId,
            SpId = sanPham.SpId,
            TrangThai = "dang_like",
            TaoLuc = DateTimeOffset.Now
        });

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoTheoDoiSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        if (taiKhoanDaDangKy.Count < 2)
        {
            return;
        }

        var mucTieu = 10;
        var daCo = _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .Select(x => (x.TkSeedId, x.FolloweeTkSeedId))
            .ToHashSet();

        for (var i = 0; i < taiKhoanDaDangKy.Count && daCo.Count < mucTieu; i++)
        {
            var nguoiTheoDoi = taiKhoanDaDangKy[i];
            var nguoiDuocTheoDoi = taiKhoanDaDangKy[(i + 1) % taiKhoanDaDangKy.Count];

            if (nguoiTheoDoi.TkSeedId == nguoiDuocTheoDoi.TkSeedId ||
                string.IsNullOrWhiteSpace(nguoiDuocTheoDoi.TkId) ||
                daCo.Contains((nguoiTheoDoi.TkSeedId, nguoiDuocTheoDoi.TkSeedId)))
            {
                continue;
            }

            var token = await _nguCanh.DangNhapLayTokenAsync(nguoiTheoDoi);
            if (token is null)
            {
                continue;
            }

            var body = new Dictionary<string, object?>
            {
                ["followee_id"] = nguoiDuocTheoDoi.TkId,
                ["action"] = "follow"
            };

            var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/set_user_follow", body, token));
            if (response.MaSoSanh != "1000")
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
            {
                TkSeedId = nguoiTheoDoi.TkSeedId,
                TkId = nguoiTheoDoi.TkId,
                FolloweeTkSeedId = nguoiDuocTheoDoi.TkSeedId,
                FolloweeTkId = nguoiDuocTheoDoi.TkId,
                TrangThai = "dang_theo_doi"
            });
            daCo.Add((nguoiTheoDoi.TkSeedId, nguoiDuocTheoDoi.TkSeedId));
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

        var mucTieu = 10;
        var capTheoDoi = _nguCanh.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .Select(x => (x.TkSeedId, x.FolloweeTkSeedId))
            .ToHashSet();
        var daCo = _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .Select(x => (x.ChanTkSeedId, x.BiChanTkSeedId))
            .ToHashSet();

        await TaoChanSellerChoBinhLuanAsync(taiKhoanDaDangKy, capTheoDoi, daCo);

        for (var i = 0; i < taiKhoanDaDangKy.Count && daCo.Count < mucTieu; i++)
        {
            var nguoiChan = taiKhoanDaDangKy[i];
            var nguoiBiChan = taiKhoanDaDangKy[(i + 2) % taiKhoanDaDangKy.Count];

            if (nguoiChan.TkSeedId == nguoiBiChan.TkSeedId ||
                string.IsNullOrWhiteSpace(nguoiBiChan.TkId) ||
                daCo.Contains((nguoiChan.TkSeedId, nguoiBiChan.TkSeedId)) ||
                capTheoDoi.Contains((nguoiChan.TkSeedId, nguoiBiChan.TkSeedId)) ||
                capTheoDoi.Contains((nguoiBiChan.TkSeedId, nguoiChan.TkSeedId)))
            {
                continue;
            }

            await TaoQuanHeChanAsync(nguoiChan, nguoiBiChan, daCo);
        }

        await _nguCanh.KhoSeed.LuuAsync();
    }

    private async Task TaoChanSellerChoBinhLuanAsync(
        IReadOnlyList<TaiKhoanSeed> taiKhoanDaDangKy,
        HashSet<(int TkSeedId, int FolloweeTkSeedId)> capTheoDoi,
        HashSet<(int ChanTkSeedId, int BiChanTkSeedId)> daCo)
    {
        if (_nguCanh.KhoSeed.LaySeedSellerChanUserChoBinhLuan() is not null)
        {
            return;
        }

        var sanPham = _nguCanh.KhoSeed.LaySanPhamTheoLoai("cho_like_comment") ?? _nguCanh.KhoSeed.LaySanPhamBatKy();
        if (sanPham is null)
        {
            return;
        }

        var seller = _nguCanh.KhoSeed.LayTaiKhoanTheoSeedId(sanPham.SellerTkSeedId);
        if (seller is null || string.IsNullOrWhiteSpace(seller.TkId))
        {
            return;
        }

        var userBiChan = taiKhoanDaDangKy.FirstOrDefault(x =>
                x.TkSeedId != seller.TkSeedId &&
                !string.IsNullOrWhiteSpace(x.TkId) &&
                !daCo.Contains((seller.TkSeedId, x.TkSeedId)) &&
                !capTheoDoi.Contains((seller.TkSeedId, x.TkSeedId)) &&
                !capTheoDoi.Contains((x.TkSeedId, seller.TkSeedId)))
            ?? taiKhoanDaDangKy.FirstOrDefault(x =>
                x.TkSeedId != seller.TkSeedId &&
                !string.IsNullOrWhiteSpace(x.TkId) &&
                !daCo.Contains((seller.TkSeedId, x.TkSeedId)));

        if (userBiChan is null)
        {
            return;
        }

        await TaoQuanHeChanAsync(seller, userBiChan, daCo);
    }

    private async Task<bool> TaoQuanHeChanAsync(
        TaiKhoanSeed nguoiChan,
        TaiKhoanSeed nguoiBiChan,
        HashSet<(int ChanTkSeedId, int BiChanTkSeedId)> daCo)
    {
        if (nguoiChan.TkSeedId == nguoiBiChan.TkSeedId ||
            string.IsNullOrWhiteSpace(nguoiChan.TkId) ||
            string.IsNullOrWhiteSpace(nguoiBiChan.TkId) ||
            daCo.Contains((nguoiChan.TkSeedId, nguoiBiChan.TkSeedId)))
        {
            return false;
        }

        var token = await _nguCanh.DangNhapLayTokenAsync(nguoiChan);
        if (token is null)
        {
            return false;
        }

        var body = new Dictionary<string, object?>
        {
            ["user_id"] = nguoiBiChan.TkId,
            ["type"] = 0
        };

        var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/set_user_block", body, token));
        if (response.MaSoSanh != "1000")
        {
            return false;
        }

        _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
        {
            ChanTkSeedId = nguoiChan.TkSeedId,
            ChanTkId = nguoiChan.TkId,
            BiChanTkSeedId = nguoiBiChan.TkSeedId,
            BiChanTkId = nguoiBiChan.TkId,
            TrangThai = "dang_chan"
        });
        daCo.Add((nguoiChan.TkSeedId, nguoiBiChan.TkSeedId));
        return true;
    }

    private List<TaiKhoanSeed> LayTaiKhoanDaDangKySanSang()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed
            .Where(x =>
                x.TrangThai == "san_sang" &&
                x.TrangThaiDangKy == "da_dang_ky" &&
                !string.IsNullOrWhiteSpace(x.TkId))
            .OrderBy(x => x.TkSeedId)
            .ToList();
    }
}
