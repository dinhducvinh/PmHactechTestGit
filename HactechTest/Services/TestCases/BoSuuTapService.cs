using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.KichBan;
using HactechTest.Models.TestCases;
using HactechTest.Repositories.TestCases;
using HactechTest.Services.App;

namespace HactechTest.Services.TestCases;

public sealed class BoSuuTapService
{
    private readonly IReadOnlyList<KichBanApi> _kichBanCode = BoKichBanApi.TaoTatCaKichBan();
    private const string LoiChuaCoDatabase = "Chưa kết nối được database HactechTestDb nên không thể thao tác test case cơ bản.";

    public IReadOnlyList<KichBanApi> LayKichBanCode()
    {
        return _kichBanCode;
    }

    public async Task<List<TestCaseDong>> LayDanhSachTestCaseDongAsync(CancellationToken ct = default)
    {
        var repository = AppHost.TaoKhiDatabaseSanSang(cs => new TestCaseDongRepository(cs));
        return repository is null ? [] : await repository.LayDanhSachAsync(ct);
    }

    public async Task<TestCaseDong?> LayTestCaseDongAsync(int id, CancellationToken ct = default)
    {
        return await TaoRepositoryBatBuoc().LayTheoIdAsync(id, ct);
    }

    public async Task LuuTestCaseDongAsync(TestCaseDong testCase, CancellationToken ct = default)
    {
        await TaoRepositoryBatBuoc().LuuAsync(testCase, ct);
    }

    public async Task XoaTestCaseDongAsync(int id, CancellationToken ct = default)
    {
        await TaoRepositoryBatBuoc().XoaAsync(id, ct);
    }

    public bool CoTheThaoTacTestCaseDong()
    {
        return AppHost.TaoKhiDatabaseSanSang(cs => new TestCaseDongRepository(cs)) is not null;
    }

    private static TestCaseDongRepository TaoRepositoryBatBuoc()
    {
        return AppHost.TaoBatBuocKhiDatabaseSanSang(
            cs => new TestCaseDongRepository(cs),
            LoiChuaCoDatabase);
    }
}
