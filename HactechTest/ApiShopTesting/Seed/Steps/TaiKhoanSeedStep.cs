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

        var mucTieu = 2;
        var daCo = _nguCanh.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed.Count(x => x.TrangThai == "dang_luu");
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

            _nguCanh.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
            {
                TaiKhoanTimKiemSeedId = IdTiepTheo(_nguCanh.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed, x => x.TaiKhoanTimKiemSeedId),
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                SavedSearchIdServer = response.Data?["id"]?.GetValue<int>(),
                Keyword = response.Data?["keyword"]?.ToString() ?? keyword,
                TrangThai = "dang_luu",
                TaoBoiTest = true,
                TaoLuc = DateTimeOffset.Now
            });
            daCo++;
        }

        await _nguCanh.CapNhatDB.LuuAsync();
    }
}


