using HactechTest.ApiShopTesting.Core;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task TaoTheoDoiSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        if (taiKhoanDaDangKy.Count < 2)
        {
            return;
        }

        var seedDangTheoDoi = _nguCanh.CapNhatDB.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .ToList();
        var seedDangChan = _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .ToList();
        var blockerChinhId = seedDangChan
            .Where(x => x.BlockerTaiKhoanIdServer is > 0)
            .GroupBy(x => x.BlockerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        var biChanTrongSeed = seedDangChan
            .Select(x => x.BlockedTaiKhoanIdServer)
            .Where(x => x is > 0)
            .Select(x => x!.Value)
            .ToHashSet();
        var followerChinhId = seedDangTheoDoi
            .Where(x => x.FollowerTaiKhoanIdServer is > 0)
            .GroupBy(x => x.FollowerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        if (followerChinhId is > 0 &&
            ((blockerChinhId is > 0 && followerChinhId == blockerChinhId) ||
             biChanTrongSeed.Contains(followerChinhId.Value)))
        {
            _nguCanh.CapNhatDB.DuLieu.TaiKhoanTheoDoiSeed.RemoveAll(x =>
                x.TrangThai == "dang_theo_doi" &&
                x.FollowerTaiKhoanIdServer == followerChinhId);
            seedDangTheoDoi = _nguCanh.CapNhatDB.DuLieu.TaiKhoanTheoDoiSeed
                .Where(x => x.TrangThai == "dang_theo_doi")
                .ToList();
            followerChinhId = null;
        }

        var nguoiTheoDoi = followerChinhId is > 0
            ? taiKhoanDaDangKy.FirstOrDefault(x => x.TaiKhoanIdServer == followerChinhId)
            : taiKhoanDaDangKy.FirstOrDefault(x =>
                  (blockerChinhId is not > 0 || x.TaiKhoanIdServer != blockerChinhId) &&
                  !biChanTrongSeed.Contains(x.TaiKhoanIdServer))
              ?? taiKhoanDaDangKy.FirstOrDefault(x =>
                  blockerChinhId is not > 0 || x.TaiKhoanIdServer != blockerChinhId)
              ?? taiKhoanDaDangKy[0];
        if (nguoiTheoDoi is null || nguoiTheoDoi.TaiKhoanIdServer is not > 0)
        {
            return;
        }

        var followeeDaCo = seedDangTheoDoi
            .Where(x => x.FollowerTaiKhoanIdServer == nguoiTheoDoi.TaiKhoanIdServer)
            .Select(x => x.FolloweeTaiKhoanIdServer)
            .Where(x => x is > 0)
            .Select(x => x!.Value)
            .ToHashSet();

        var token = await LayTokenSeedAsync(nguoiTheoDoi, $"tạo quan hệ follow seed cho tài khoản {nguoiTheoDoi.SoThuTu}");

        for (var i = 0; i < taiKhoanDaDangKy.Count && followeeDaCo.Count < YeuCauDuLieuSeed.SoQuanHeChinhMucTieu; i++)
        {
            var nguoiDuocTheoDoi = taiKhoanDaDangKy[i];
            if (nguoiTheoDoi.TaiKhoanIdServer == nguoiDuocTheoDoi.TaiKhoanIdServer ||
                nguoiDuocTheoDoi.TaiKhoanIdServer is not > 0 ||
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

            _nguCanh.CapNhatDB.DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
            {
                FollowerTaiKhoanIdServer = nguoiTheoDoi.TaiKhoanIdServer,
                FolloweeTaiKhoanIdServer = nguoiDuocTheoDoi.TaiKhoanIdServer,
                TheoDoiLuc = DateTimeOffset.Now,
                TrangThai = "dang_theo_doi"
            });
            followeeDaCo.Add(nguoiDuocTheoDoi.TaiKhoanIdServer);
        }

        await _nguCanh.CapNhatDB.LuuAsync();
    }

    private async Task TaoChanSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        if (taiKhoanDaDangKy.Count < 2)
        {
            return;
        }

        var seedDangTheoDoi = _nguCanh.CapNhatDB.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x => x.TrangThai == "dang_theo_doi")
            .ToList();
        var capTheoDoi = seedDangTheoDoi
            .Where(x => x.FollowerTaiKhoanIdServer is > 0 && x.FolloweeTaiKhoanIdServer is > 0)
            .Select(x => (FollowerTaiKhoanIdServer: x.FollowerTaiKhoanIdServer!.Value, FolloweeTaiKhoanIdServer: x.FolloweeTaiKhoanIdServer!.Value))
            .ToHashSet();
        var seedDangChan = _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .ToList();
        var daCo = seedDangChan
            .Where(x => x.BlockerTaiKhoanIdServer is > 0 && x.BlockedTaiKhoanIdServer is > 0)
            .Select(x => (BlockerTaiKhoanIdServer: x.BlockerTaiKhoanIdServer!.Value, BlockedTaiKhoanIdServer: x.BlockedTaiKhoanIdServer!.Value))
            .ToHashSet();
        var followerChinhId = seedDangTheoDoi
            .Where(x => x.FollowerTaiKhoanIdServer is > 0)
            .GroupBy(x => x.FollowerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        var blockerChinhId = seedDangChan
            .Where(x => x.BlockerTaiKhoanIdServer is > 0)
            .GroupBy(x => x.BlockerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        var nguoiChan = blockerChinhId is > 0
            ? taiKhoanDaDangKy.FirstOrDefault(x => x.TaiKhoanIdServer == blockerChinhId)
            : taiKhoanDaDangKy.FirstOrDefault(x => followerChinhId is not > 0 || x.TaiKhoanIdServer != followerChinhId);
        if (nguoiChan is null || nguoiChan.TaiKhoanIdServer is not > 0)
        {
            return;
        }

        var daDonBlockRaiRac = false;
        for (var i = _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed.Count - 1; i >= 0; i--)
        {
            var item = _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed[i];
            if (item.TrangThai != "dang_chan" || item.BlockerTaiKhoanIdServer != nguoiChan.TaiKhoanIdServer)
            {
                _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed.RemoveAt(i);
                daDonBlockRaiRac = true;
            }
        }

        seedDangChan = _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan" && x.BlockerTaiKhoanIdServer == nguoiChan.TaiKhoanIdServer)
            .ToList();
        daCo = seedDangChan
            .Where(x => x.BlockerTaiKhoanIdServer is > 0 && x.BlockedTaiKhoanIdServer is > 0)
            .Select(x => (BlockerTaiKhoanIdServer: x.BlockerTaiKhoanIdServer!.Value, BlockedTaiKhoanIdServer: x.BlockedTaiKhoanIdServer!.Value))
            .ToHashSet();

        var biChanDaCo = seedDangChan
            .Where(x => x.BlockerTaiKhoanIdServer == nguoiChan.TaiKhoanIdServer)
            .Select(x => x.BlockedTaiKhoanIdServer)
            .Where(x => x is > 0)
            .Select(x => x!.Value)
            .ToHashSet();
        var followeeCuaFollowerChinh = seedDangTheoDoi
            .Where(x => followerChinhId is not > 0 || x.FollowerTaiKhoanIdServer == followerChinhId)
            .Select(x => x.FolloweeTaiKhoanIdServer)
            .Where(x => x is > 0)
            .Select(x => x!.Value)
            .ToHashSet();

        for (var i = 0; i < taiKhoanDaDangKy.Count && biChanDaCo.Count < YeuCauDuLieuSeed.SoQuanHeChinhMucTieu; i++)
        {
            var nguoiBiChan = taiKhoanDaDangKy[i];
            if (nguoiChan.TaiKhoanIdServer == nguoiBiChan.TaiKhoanIdServer ||
                nguoiBiChan.TaiKhoanIdServer is not > 0 ||
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
            await _nguCanh.CapNhatDB.LuuAsync();
        }
    }

    private async Task<bool> TaoQuanHeChanAsync(
        TaiKhoanSignupThanhCongSeed nguoiChan,
        TaiKhoanSignupThanhCongSeed nguoiBiChan,
        HashSet<(int BlockerTaiKhoanIdServer, int BlockedTaiKhoanIdServer)> daCo)
    {
        if (nguoiChan.TaiKhoanIdServer is not > 0 ||
            nguoiBiChan.TaiKhoanIdServer is not > 0 ||
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

        _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
        {
            BlockerTaiKhoanIdServer = nguoiChan.TaiKhoanIdServer,
            BlockedTaiKhoanIdServer = nguoiBiChan.TaiKhoanIdServer,
            ChanLuc = DateTimeOffset.Now,
            TrangThai = "dang_chan"
        });
        daCo.Add((nguoiChan.TaiKhoanIdServer, nguoiBiChan.TaiKhoanIdServer));
        return true;
    }

    private bool CoQuanHeChan(int? tkIdA, int? tkIdB)
    {
        if (tkIdA is not > 0 || tkIdB is not > 0)
        {
            return false;
        }

        return _nguCanh.CapNhatDB.DuLieu.TaiKhoanChanSeed.Any(x =>
            x.TrangThai == "dang_chan" &&
            ((x.BlockerTaiKhoanIdServer == tkIdA.Value && x.BlockedTaiKhoanIdServer == tkIdB.Value) ||
             (x.BlockerTaiKhoanIdServer == tkIdB.Value && x.BlockedTaiKhoanIdServer == tkIdA.Value)));
    }
}
