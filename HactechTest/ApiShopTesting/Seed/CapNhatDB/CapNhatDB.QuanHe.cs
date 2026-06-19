using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    internal async Task ThemQuanHeTheoDoiAsync(
        TaiKhoanSignupThanhCongSeed taiKhoanTheoDoi,
        TaiKhoanSignupThanhCongSeed taiKhoanDuocTheoDoi)
    {
        DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
        {
            FollowerTaiKhoanIdServer = taiKhoanTheoDoi.TaiKhoanIdServer,
            FolloweeTaiKhoanIdServer = taiKhoanDuocTheoDoi.TaiKhoanIdServer,
            TheoDoiLuc = DateTimeOffset.Now,
            TrangThai = "dang_theo_doi"
        });
        await LuuAsync(BangDuLieuSeed.TheoDoi);
    }

    internal async Task XoaQuanHeTheoDoiDangHoatDongAsync(TaiKhoanTheoDoiSeed quanHe)
    {
        DuLieu.TaiKhoanTheoDoiSeed.RemoveAll(x =>
            QuanHeTheoDoiDangHoatDong(x) &&
            x.FollowerTaiKhoanIdServer == quanHe.FollowerTaiKhoanIdServer &&
            x.FolloweeTaiKhoanIdServer == quanHe.FolloweeTaiKhoanIdServer);
        await LuuAsync(BangDuLieuSeed.TheoDoi);
    }

    internal async Task XoaQuanHeTheoDoiHaiChieuAsync(
        TaiKhoanSignupThanhCongSeed taiKhoanThuNhat,
        TaiKhoanSignupThanhCongSeed taiKhoanThuHai)
    {
        await XoaQuanHeTheoDoiHaiChieuAsync(
            taiKhoanThuNhat.TaiKhoanIdServer,
            taiKhoanThuHai.TaiKhoanIdServer);
    }

    internal async Task XoaQuanHeTheoDoiHaiChieuAsync(int? taiKhoanIdThuNhat, int? taiKhoanIdThuHai)
    {
        XoaQuanHeTheoDoiHaiChieuTrongBoNho(taiKhoanIdThuNhat, taiKhoanIdThuHai);
        await LuuAsync(BangDuLieuSeed.TheoDoi);
    }

    internal async Task ThemQuanHeChanAsync(
        TaiKhoanSignupThanhCongSeed taiKhoanChan,
        TaiKhoanSignupThanhCongSeed taiKhoanBiChan)
    {
        DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
        {
            BlockerTaiKhoanIdServer = taiKhoanChan.TaiKhoanIdServer,
            BlockedTaiKhoanIdServer = taiKhoanBiChan.TaiKhoanIdServer,
            ChanLuc = DateTimeOffset.Now,
            TrangThai = "dang_chan"
        });
        XoaQuanHeTheoDoiHaiChieuTrongBoNho(
            taiKhoanChan.TaiKhoanIdServer,
            taiKhoanBiChan.TaiKhoanIdServer);
        await LuuAsync(BangDuLieuSeed.Chan, BangDuLieuSeed.TheoDoi);
    }

    internal async Task XoaQuanHeChanDangHoatDongAsync(TaiKhoanChanSeed quanHe)
    {
        DuLieu.TaiKhoanChanSeed.RemoveAll(x =>
            QuanHeChanDangHoatDong(x) &&
            x.BlockerTaiKhoanIdServer == quanHe.BlockerTaiKhoanIdServer &&
            x.BlockedTaiKhoanIdServer == quanHe.BlockedTaiKhoanIdServer);
        await LuuAsync(BangDuLieuSeed.Chan);
    }

    private int XoaQuanHeTheoDoiHaiChieuTrongBoNho(int? taiKhoanIdThuNhat, int? taiKhoanIdThuHai)
    {
        if (taiKhoanIdThuNhat is not > 0 ||
            taiKhoanIdThuHai is not > 0 ||
            taiKhoanIdThuNhat == taiKhoanIdThuHai)
        {
            return 0;
        }

        return DuLieu.TaiKhoanTheoDoiSeed.RemoveAll(x =>
            QuanHeTheoDoiDangHoatDong(x) &&
            ((x.FollowerTaiKhoanIdServer == taiKhoanIdThuNhat &&
              x.FolloweeTaiKhoanIdServer == taiKhoanIdThuHai) ||
             (x.FollowerTaiKhoanIdServer == taiKhoanIdThuHai &&
              x.FolloweeTaiKhoanIdServer == taiKhoanIdThuNhat)));
    }
}
