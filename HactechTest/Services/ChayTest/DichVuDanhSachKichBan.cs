using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.KichBan;
using HactechTest.Services.DynamicTests;

namespace HactechTest.Services.ChayTest;

public sealed class DichVuDanhSachKichBan
{
    private readonly IReadOnlyList<KichBanApi> _kichBanCode = BoKichBanApi.TaoTatCaKichBan();

    public async Task<IReadOnlyList<KichBanApi>> LayTatCaAsync(
        string? chuoiKetNoi,
        CancellationToken ct = default)
    {
        var danhSach = new List<KichBanApi>(_kichBanCode);
        if (string.IsNullOrWhiteSpace(chuoiKetNoi))
        {
            return danhSach;
        }

        var store = new TestCaseDongStore(chuoiKetNoi);
        var testCaseDong = await store.LayDanhSachAsync(ct);
        danhSach.AddRange(testCaseDong.Select(BoKichBanDong.TaoKichBan));
        return danhSach;
    }
}
