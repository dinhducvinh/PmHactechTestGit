using KiemThuApiShop.Core;

namespace KiemThuApiShop.Seed;

public sealed class ChuanBiDuLieuMoi
{
    private const int SoQuanHeChinhToiThieu = 2;
    private const int SoQuanHeChinhMucTieu = 3;

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
            InBaoCaoDuLieuMoi("Dữ liệu mồi trong SQL Server đã đủ, bỏ qua bước chuẩn bị.", duLieuConThieu);
            return;
        }

        InBaoCaoDuLieuMoi("Dữ liệu mồi còn thiếu, chỉ gọi API để bổ sung các phần thiếu:", duLieuConThieu);

        try
        {
            if (ThieuTaiKhoanDaDangKy())
            {
                await DangKyTaiKhoanSeedAsync();
            }

            if (ThieuTheoDoi())
            {
                await TaoTheoDoiSeedAsync();
            }

            if (ThieuTimKiem())
            {
                await TaoTimKiemSeedAsync();
            }

            if (ThieuChan())
            {
                await TaoChanSeedAsync();
            }

            var sauChuanBiConThieu = KiemTraDuLieuConThieu();
            InBaoCaoDuLieuMoi(
                sauChuanBiConThieu.Count == 0
                    ? "Sau bước chuẩn bị: dữ liệu mồi đã đủ."
                    : "Sau bước chuẩn bị vẫn còn thiếu dữ liệu mồi:",
                sauChuanBiConThieu);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Không kết nối được API để chuẩn bị seed: {ex.Message}");
            InBaoCaoDuLieuMoi("Dữ liệu mồi sau lỗi kết nối vẫn còn thiếu:", KiemTraDuLieuConThieu());
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Quá thời gian chờ khi chuẩn bị seed. Các case cần dữ liệu mồi có thể bị bỏ qua.");
            InBaoCaoDuLieuMoi("Dữ liệu mồi sau timeout vẫn còn thiếu:", KiemTraDuLieuConThieu());
        }
    }

    private void InBaoCaoDuLieuMoi(string tieuDe, IReadOnlyList<string> duLieuConThieu)
    {
        Console.WriteLine(tieuDe);
        InThongKeDuLieuMoi();

        if (duLieuConThieu.Count == 0)
        {
            Console.WriteLine("- Trạng thái: đã đủ dữ liệu mồi cần thiết.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine("- Các phần còn thiếu:");
        foreach (var item in duLieuConThieu)
        {
            Console.WriteLine($"  + {item}");
        }

        Console.WriteLine();
    }

    private void InThongKeDuLieuMoi()
    {
        var taiKhoanDaDangKy = _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed.Count(x =>
            x.TrangThai == "san_sang" &&
            x.TrangThaiDangKy == "da_dang_ky");
        var timKiemDangLuu = _nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Count(x => x.TrangThai == "dang_luu");
        var theoDoiFollowerChinh = DemQuanHeTheoDoiCuaFollowerChinh();
        var chanBlockerChinh = DemQuanHeChanCuaBlockerChinh();

        Console.WriteLine("- Hiện có:");
        Console.WriteLine($"  + Tài khoản đã đăng ký: {taiKhoanDaDangKy}/{_nguCanh.CauHinh.SoTaiKhoanDangKyTruoc}");
        Console.WriteLine($"  + Saved search dang_luu: {timKiemDangLuu}/2");
        Console.WriteLine($"  + Follow của follower_chinh: {theoDoiFollowerChinh}/{SoQuanHeChinhToiThieu}");
        Console.WriteLine($"  + Block của blocker_chinh: {chanBlockerChinh}/{SoQuanHeChinhToiThieu}");
    }

    private List<string> KiemTraDuLieuConThieu()
    {
        var thieu = new List<string>();

        if (ThieuTaiKhoanDaDangKy())
        {
            thieu.Add($"Thiếu tài khoản đã đăng ký trong taikhoan_seed: cần tối thiểu {_nguCanh.CauHinh.SoTaiKhoanDangKyTruoc}.");
        }

        if (ThieuTheoDoi())
        {
            thieu.Add("Thiếu 2-3 quan hệ theo dõi dang_theo_doi của follower_chinh trong tk_theodoi_seed.");
        }

        if (ThieuTimKiem())
        {
            thieu.Add("Thiếu lịch sử tìm kiếm dang_luu trong tk_timkiem_seed.");
        }

        if (ThieuChan())
        {
            thieu.Add("Thiếu 2-3 quan hệ chặn dang_chan của blocker_chinh trong tk_chan_seed.");
        }

        return thieu;
    }

    private bool ThieuTaiKhoanDaDangKy()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanSeed.Count(x =>
            x.TrangThai == "san_sang" &&
            x.TrangThaiDangKy == "da_dang_ky") < _nguCanh.CauHinh.SoTaiKhoanDangKyTruoc;
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
            .GroupBy(x => x.TkSeedId)
            .Select(x => x.Count())
            .DefaultIfEmpty(0)
            .Max();
    }

    private int DemQuanHeChanCuaBlockerChinh()
    {
        return _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .GroupBy(x => x.ChanTkSeedId)
            .Select(x => x.Count())
            .DefaultIfEmpty(0)
            .Max();
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
            var token = await _nguCanh.DangNhapLayTokenAsync(taiKhoan);
            if (token is null)
            {
                continue;
            }

            var keyword = $"search_seed_{taiKhoan.TkSeedId}_{DateTimeOffset.Now:yyyyMMddHHmmss}";
            var body = new Dictionary<string, object?> { ["keyword"] = keyword };
            var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/api/save_search", body, token));
            if (response.MaSoSanh != "1000")
            {
                continue;
            }

            _nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
            {
                TkTimKiemSeedId = _nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Count == 0
                    ? 1
                    : _nguCanh.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Max(x => x.TkTimKiemSeedId) + 1,
                TkSeedId = taiKhoan.TkSeedId,
                TkId = taiKhoan.TkId,
                SavedSearchId = TienIchJson.DocChuoi(response.Data, "id", "saved_search_id"),
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
        var followerChinhSeedId = seedDangTheoDoi
            .GroupBy(x => x.TkSeedId)
            .OrderByDescending(x => x.Count())
            .Select(x => (int?)x.Key)
            .FirstOrDefault();
        var nguoiTheoDoi = followerChinhSeedId.HasValue
            ? taiKhoanDaDangKy.FirstOrDefault(x => x.TkSeedId == followerChinhSeedId.Value)
            : taiKhoanDaDangKy[0];
        if (nguoiTheoDoi is null || string.IsNullOrWhiteSpace(nguoiTheoDoi.TkId))
        {
            return;
        }

        var followeeDaCo = seedDangTheoDoi
            .Where(x => x.TkSeedId == nguoiTheoDoi.TkSeedId)
            .Select(x => x.FolloweeTkSeedId)
            .ToHashSet();

        var token = await _nguCanh.DangNhapLayTokenAsync(nguoiTheoDoi);
        if (token is null)
        {
            return;
        }

        for (var i = 0; i < taiKhoanDaDangKy.Count && followeeDaCo.Count < SoQuanHeChinhMucTieu; i++)
        {
            var nguoiDuocTheoDoi = taiKhoanDaDangKy[i];
            if (nguoiTheoDoi.TkSeedId == nguoiDuocTheoDoi.TkSeedId ||
                string.IsNullOrWhiteSpace(nguoiDuocTheoDoi.TkId) ||
                followeeDaCo.Contains(nguoiDuocTheoDoi.TkSeedId))
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
                TheoDoiLuc = DateTimeOffset.Now,
                TrangThai = "dang_theo_doi"
            });
            followeeDaCo.Add(nguoiDuocTheoDoi.TkSeedId);
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
            .Select(x => (x.TkSeedId, x.FolloweeTkSeedId))
            .ToHashSet();
        var seedDangChan = _nguCanh.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .ToList();
        var daCo = seedDangChan
            .Select(x => (x.ChanTkSeedId, x.BiChanTkSeedId))
            .ToHashSet();
        var followerChinhSeedId = seedDangTheoDoi
            .GroupBy(x => x.TkSeedId)
            .OrderByDescending(x => x.Count())
            .Select(x => (int?)x.Key)
            .FirstOrDefault();
        var blockerChinhSeedId = seedDangChan
            .GroupBy(x => x.ChanTkSeedId)
            .OrderByDescending(x => x.Count())
            .Select(x => (int?)x.Key)
            .FirstOrDefault();
        var nguoiChan = blockerChinhSeedId.HasValue
            ? taiKhoanDaDangKy.FirstOrDefault(x => x.TkSeedId == blockerChinhSeedId.Value)
            : taiKhoanDaDangKy.FirstOrDefault(x => !followerChinhSeedId.HasValue || x.TkSeedId != followerChinhSeedId.Value);
        if (nguoiChan is null)
        {
            return;
        }

        var biChanDaCo = seedDangChan
            .Where(x => x.ChanTkSeedId == nguoiChan.TkSeedId)
            .Select(x => x.BiChanTkSeedId)
            .ToHashSet();
        var followeeCuaFollowerChinh = seedDangTheoDoi
            .Where(x => !followerChinhSeedId.HasValue || x.TkSeedId == followerChinhSeedId.Value)
            .Select(x => x.FolloweeTkSeedId)
            .ToHashSet();

        for (var i = 0; i < taiKhoanDaDangKy.Count && biChanDaCo.Count < SoQuanHeChinhMucTieu; i++)
        {
            var nguoiBiChan = taiKhoanDaDangKy[i];
            if (nguoiChan.TkSeedId == nguoiBiChan.TkSeedId ||
                string.IsNullOrWhiteSpace(nguoiBiChan.TkId) ||
                biChanDaCo.Contains(nguoiBiChan.TkSeedId) ||
                followeeCuaFollowerChinh.Contains(nguoiBiChan.TkSeedId) ||
                daCo.Contains((nguoiChan.TkSeedId, nguoiBiChan.TkSeedId)) ||
                capTheoDoi.Contains((nguoiChan.TkSeedId, nguoiBiChan.TkSeedId)) ||
                capTheoDoi.Contains((nguoiBiChan.TkSeedId, nguoiChan.TkSeedId)))
            {
                continue;
            }

            if (await TaoQuanHeChanAsync(nguoiChan, nguoiBiChan, daCo))
            {
                biChanDaCo.Add(nguoiBiChan.TkSeedId);
            }
        }

        await _nguCanh.KhoSeed.LuuAsync();
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
            ChanLuc = DateTimeOffset.Now,
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
