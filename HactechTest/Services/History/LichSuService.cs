using HactechTest.ApiShopTesting.Core;
using HactechTest.Models.History;
using HactechTest.Repositories.History;
using HactechTest.Services.App;
using HactechTest.Services.Reports;

namespace HactechTest.Services.History;

public sealed class LichSuService
{
    public async Task<List<PhienChayDaLuu>> LayDanhSachAsync(string keyword, CancellationToken ct = default)
    {
        var repository = AppHost.TaoKhiDatabaseSanSang(cs => new PhienChayRepository(cs));
        return repository is null ? [] : await repository.LayDanhSachAsync(keyword, ct);
    }

    public async Task<List<ChiTietPhienChayDaLuu>> LayChiTietAsync(int phienChayId, CancellationToken ct = default)
    {
        var repository = AppHost.TaoKhiDatabaseSanSang(cs => new PhienChayRepository(cs));
        return repository is null ? [] : await repository.LayChiTietAsync(phienChayId, ct);
    }

    public async Task<List<ChiTietPhienChayDaLuu>> LayChiTietTomTatAsync(int phienChayId, CancellationToken ct = default)
    {
        var repository = AppHost.TaoKhiDatabaseSanSang(cs => new PhienChayRepository(cs));
        return repository is null ? [] : await repository.LayChiTietTomTatAsync(phienChayId, ct);
    }

    public async Task XoaPhienAsync(int phienChayId, CancellationToken ct = default)
    {
        var repository = AppHost.TaoKhiDatabaseSanSang(cs => new PhienChayRepository(cs));
        if (repository is not null)
        {
            await repository.XoaPhienAsync(phienChayId, ct);
        }
    }

    public DuLieuBaoCaoPhienChay TaoDuLieuBaoCao(PhienChayDaLuu session, IReadOnlyList<ChiTietPhienChayDaLuu> chiTiet)
    {
        var baoCaoService = new BaoCaoPhienChayService();
        return baoCaoService.TaoDuLieuBaoCaoTuDongKetQua(
            TaoThongTinBaoCao(session),
            TaoDongBaoCao(chiTiet));
    }

    private static ThongTinBaoCaoPhienChay TaoThongTinBaoCao(PhienChayDaLuu session)
    {
        return new ThongTinBaoCaoPhienChay(
            new DateTimeOffset(session.RunAt),
            null,
            session.NguoiChay,
            session.Machine,
            session.OperatingSystem,
            session.BaseUrl,
            session.CheDoChay,
            session.CheDoLoi);
    }

    private static List<DongBaoCaoTestCase> TaoDongBaoCao(IReadOnlyList<ChiTietPhienChayDaLuu> chiTiet)
    {
        return chiTiet.Select((item, index) =>
        {
            var (ma, tenHienThi) = TachMaVaTenTestCase(item.TestCaseName);
            var trangThai = ChuyenTrangThai(item.Result);
            return new DongBaoCaoTestCase
            {
                Stt = item.SequenceNo > 0 ? item.SequenceNo : index + 1,
                Ma = ma,
                Nhom = TaoNhomTuMa(ma),
                TenHienThi = tenHienThi,
                TrangThai = string.IsNullOrWhiteSpace(item.Result)
                    ? DinhDangKetQuaKiemThu.TrangThaiHienThi(trangThai)
                    : item.Result,
                MaMongDoi = item.ExpectedStatus,
                MaThucTe = item.ActualStatus,
                HttpStatus = LayHttpStatus(item.ActualStatus),
                ThoiGianMs = item.DurationMs,
                Endpoint = TaoEndpoint(item),
                RequestBodyJson = item.RequestBody,
                ThongDiep = item.Reason,
                ResponseRutGon = item.ResponseBody,
                TrangThaiGoc = trangThai
            };
        }).ToList();
    }

    private static (string Ma, string TenHienThi) TachMaVaTenTestCase(string tenTestCase)
    {
        var parts = tenTestCase.Split(" - ", 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2
            ? (parts[0], parts[1])
            : ("", tenTestCase);
    }

    private static string TaoNhomTuMa(string ma)
    {
        if (string.IsNullOrWhiteSpace(ma))
        {
            return "Lịch sử";
        }

        var nhom = ma.Split('-', 2, StringSplitOptions.TrimEntries)[0];
        return string.IsNullOrWhiteSpace(nhom)
            ? "Lịch sử"
            : char.ToUpperInvariant(nhom[0]) + nhom[1..].ToLowerInvariant();
    }

    private static TrangThaiKetQua ChuyenTrangThai(string ketQua)
    {
        return ketQua.Trim().ToUpperInvariant() switch
        {
            "PASS" => TrangThaiKetQua.Dat,
            "SKIP" => TrangThaiKetQua.BoQua,
            "FAIL" => TrangThaiKetQua.ThatBai,
            _ => TrangThaiKetQua.LoiMoiTruong
        };
    }

    private static int? LayHttpStatus(string actualStatus)
    {
        var raw = actualStatus.Trim();
        if (raw.StartsWith("HTTP_", StringComparison.OrdinalIgnoreCase))
        {
            raw = raw[5..];
        }
        else if (raw.StartsWith("HTTP ", StringComparison.OrdinalIgnoreCase))
        {
            raw = raw[5..];
        }
        else
        {
            return null;
        }

        return int.TryParse(raw, out var status) ? status : null;
    }

    private static string? TaoEndpoint(ChiTietPhienChayDaLuu chiTiet)
    {
        if (string.IsNullOrWhiteSpace(chiTiet.HttpMethod))
        {
            return string.IsNullOrWhiteSpace(chiTiet.Url) ? null : chiTiet.Url;
        }

        return string.IsNullOrWhiteSpace(chiTiet.Url)
            ? chiTiet.HttpMethod
            : $"{chiTiet.HttpMethod} {chiTiet.Url}";
    }
}
