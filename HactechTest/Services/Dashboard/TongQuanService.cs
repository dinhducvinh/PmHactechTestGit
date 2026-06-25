using HactechTest.Models.Dashboard;
using HactechTest.Repositories.Dashboard;
using HactechTest.Services.App;

namespace HactechTest.Services.Dashboard;

public sealed class TongQuanService
{
    public const string TatCaNguoiChay = "(tat ca)";

    public async Task<TongQuanThongKe> LayThongKeAsync(BoLocTongQuan boLoc, CancellationToken ct = default)
    {
        var repository = AppHost.TaoKhiDatabaseSanSang(cs => new TongQuanRepository(cs));
        if (repository is null)
        {
            return new TongQuanThongKe(0, 0, 0);
        }

        return await repository.LayThongKeAsync(boLoc, ct);
    }

    public async Task<IReadOnlyList<string>> LayNguoiChayAsync(CancellationToken ct = default)
    {
        var danhSach = new List<string> { TatCaNguoiChay };
        var repository = AppHost.TaoKhiDatabaseSanSang(cs => new TongQuanRepository(cs));
        if (repository is null)
        {
            return danhSach;
        }

        danhSach.AddRange(await repository.LayNguoiChayAsync(ct));
        return danhSach;
    }

    public static string? ChuanHoaNguoiChay(string? nguoiChay)
    {
        return !string.IsNullOrWhiteSpace(nguoiChay) &&
            !string.Equals(nguoiChay, TatCaNguoiChay, StringComparison.OrdinalIgnoreCase)
                ? nguoiChay
                : null;
    }
}
