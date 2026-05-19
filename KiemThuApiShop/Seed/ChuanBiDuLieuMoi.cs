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
        Console.WriteLine("Đang chuẩn bị dữ liệu mồi tốt nhất có thể...");

        try
        {
            await DangKyTaiKhoanSeedAsync();
            await DongBoDanhMucAsync();
            await DongBoThuongHieuAsync();
            await TaoDiaChiSeedAsync();
            await TaoSanPhamSeedAsync();
            await TaoBinhLuanSeedAsync();
            await TaoLikeSeedAsync();
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
}
