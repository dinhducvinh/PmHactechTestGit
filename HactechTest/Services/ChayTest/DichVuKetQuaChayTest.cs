using HactechTest.ApiShopTesting.Core;
using HactechTest.Models.History;
using HactechTest.Repositories.History;
using HactechTest.Services.App;
using HactechTest.Services.Reports;

namespace HactechTest.Services.ChayTest;

public sealed record TongKetChayTest(int Tong, int Dat, int KhongDat, decimal TyLeDat, int ThoiGianTrungBinhMs)
{
    public string TaoNoiDungHienThi()
    {
        return $"Tổng: {Tong}   Đạt: {Dat}   Không đạt: {KhongDat}   Tỷ lệ: {TyLeDat}%   TB: {ThoiGianTrungBinhMs} ms";
    }
}

public sealed record ThongTinPhienChayHienTai(
    DateTimeOffset? BatDauLuc,
    DateTimeOffset? KetThucLuc,
    string BaseUrl,
    string CheDoChay,
    string CheDoLoi);

public sealed class DichVuKetQuaChayTest
{
    public TongKetChayTest TaoTongKet(IReadOnlyList<KetQuaChay> ketQua)
    {
        var tong = ketQua.Count;
        var dat = ketQua.Count(item => item.TrangThai == TrangThaiKetQua.Dat);
        var khongDat = tong - dat;
        var tyLe = tong == 0 ? 0 : Math.Round(100m * dat / tong, 2);
        var thoiGianTrungBinh = tong == 0 ? 0 : (int)ketQua.Average(item => item.ThoiGian.TotalMilliseconds);

        return new TongKetChayTest(tong, dat, khongDat, tyLe, thoiGianTrungBinh);
    }

    public async Task<int> LuuPhienChayAsync(
        string connectionString,
        IReadOnlyList<KetQuaChay> ketQua,
        ThongTinPhienChayHienTai thongTin,
        CancellationToken ct = default)
    {
        var repository = new PhienChayRepository(connectionString);
        var ketQuaLuu = ketQua
            .Select((item, index) => BoChuyenDoiKetQuaChayTest.TaoKetQuaLuuPhien(
                item,
                index + 1,
                thongTin.BaseUrl))
            .ToList();

        return await repository.LuuPhienChayAsync(
            LayTenDangNhapNguoiThucHien(),
            Environment.MachineName,
            Environment.OSVersion.VersionString,
            thongTin.CheDoChay,
            thongTin.CheDoLoi,
            thongTin.BaseUrl,
            ketQuaLuu,
            ct);
    }

    public ThongTinBaoCaoPhienChay TaoThongTinBaoCao(ThongTinPhienChayHienTai thongTin)
    {
        return new ThongTinBaoCaoPhienChay(
            thongTin.BatDauLuc,
            thongTin.KetThucLuc,
            LayTenDangNhapNguoiThucHien(),
            Environment.MachineName,
            Environment.OSVersion.VersionString,
            thongTin.BaseUrl,
            thongTin.CheDoChay,
            thongTin.CheDoLoi);
    }

    public static string LayTenDangNhapNguoiThucHien()
    {
        if (AppHost.IsInitialized && AppHost.Instance.TaiKhoanDangNhap is { } taiKhoan)
        {
            return taiKhoan.TenDangNhap;
        }

        return Environment.UserName;
    }
}
