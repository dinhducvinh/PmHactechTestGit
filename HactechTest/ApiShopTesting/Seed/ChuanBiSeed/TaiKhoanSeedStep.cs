using HactechTest.ApiShopTesting.Core;


namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task DangKyTaiKhoanChuaDangKyAsync()
    {
        var daDangKy = _nguCanh.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed.Count(x =>
            x.TaiKhoanIdServer is > 0);
        var canDangKy = Math.Max(0, YeuCauDuLieuSeed.SoTaiKhoanDaDangKyToiThieu - daDangKy);

        for (var i = 0; i < canDangKy; i++)
        {
            try
            {
                await HelperTC.DangKyTaiKhoanMoiAsync(_nguCanh);
            }
            catch (BoQuaKiemThuException ex)
            {
                throw new LoiChuanBiKiemThuException(
                    $"Không thể đăng ký tài khoản seed để chuẩn bị đủ {YeuCauDuLieuSeed.SoTaiKhoanDaDangKyToiThieu} tài khoản mồi. Chi tiết: {ex.Message}");
            }
        }
    }

    private async Task TaoTimKiemSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        if (taiKhoanDaDangKy.Count == 0)
        {
            return;
        }

        var mucTieu = YeuCauDuLieuSeed.SoSavedSearchToiThieu;
        var daCo = _nguCanh.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed.Count(x =>
            x.TrangThai == "dang_luu" &&
            x.SavedSearchIdServer is > 0);
        var taiKhoanDaCoTimKiem = _nguCanh.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed
            .Where(x => x.TrangThai == "dang_luu" && x.TaiKhoanIdServer is > 0 && x.SavedSearchIdServer is > 0)
            .Select(x => x.TaiKhoanIdServer!.Value)
            .ToHashSet();
        var taiKhoanUuTien = taiKhoanDaDangKy
            .OrderBy(x => taiKhoanDaCoTimKiem.Contains(x.TaiKhoanIdServer) ? 1 : 0)
            .ThenBy(x => x.SoThuTu)
            .ThenBy(x => x.TaiKhoanIdServer)
            .ToList();

        var coThayDoi = false;
        for (var i = 0; i < taiKhoanUuTien.Count && daCo < mucTieu; i++)
        {
            var taiKhoan = taiKhoanUuTien[i];
            if (taiKhoanDaCoTimKiem.Contains(taiKhoan.TaiKhoanIdServer) &&
                taiKhoanDaCoTimKiem.Count < taiKhoanDaDangKy.Count)
            {
                continue;
            }

            var token = await LayTokenSeedAsync(taiKhoan, $"tạo saved search seed cho tài khoản {taiKhoan.SoThuTu}");

            var keyword = $"search_seed_{taiKhoan.SoThuTu}_{DateTimeOffset.Now:yyyyMMddHHmmss}";
            var body = new Dictionary<string, object?> { ["keyword"] = keyword };
            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/api/save_search",
                body,
                token,
                $"tạo saved search seed cho tài khoản {taiKhoan.SoThuTu}");

            var savedSearchIdServer =
                HelperTC.DocIdSau(response.Data, "id") ??
                HelperTC.DocIdSau(response.Data, "saved_search_id");
            if (savedSearchIdServer is not > 0)
            {
                throw new LoiChuanBiKiemThuException(
                    $"API /api/save_search trả thành công nhưng không có id saved search cho tài khoản seed {taiKhoan.SoThuTu}. Response: {HelperTC.RutGon(response.NoiDungRaw)}");
            }

            _nguCanh.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
            {
                TaiKhoanTimKiemSeedId = IdTiepTheo(_nguCanh.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed, x => x.TaiKhoanTimKiemSeedId),
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                SavedSearchIdServer = savedSearchIdServer,
                Keyword = response.Data?["keyword"]?.ToString() ?? keyword,
                TrangThai = "dang_luu",
                TaoBoiTest = true,
                TaoLuc = DateTimeOffset.Now
            });
            daCo++;
            taiKhoanDaCoTimKiem.Add(taiKhoan.TaiKhoanIdServer);
            coThayDoi = true;
        }

        if (coThayDoi)
        {
            await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.TimKiem);
        }
    }
}


