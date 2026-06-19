using System.Diagnostics;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;
using HactechTest.Services.DynamicTests;

namespace HactechTest.Services.ChayTest;

public sealed record YeuCauChayTestApi(
    CauHinhChay CauHinh,
    IReadOnlyList<KichBanApi> DanhSachTestCase,
    bool DungKhiLoi);

public sealed record TienTrinhChayTestApi(
    int DangChayThu,
    int TongSo,
    string TrangThai,
    KichBanApi? KichBan = null,
    KetQuaChay? KetQua = null);

public sealed record KetQuaPhienChayApi(
    DateTimeOffset BatDauLuc,
    DateTimeOffset KetThucLuc,
    IReadOnlyList<KetQuaChay> KetQua,
    bool BiHuy,
    bool DungDoLoi);

public sealed class DichVuChayTestApi
{
    public async Task<KetQuaPhienChayApi> ChayAsync(
        YeuCauChayTestApi yeuCau,
        IProgress<TienTrinhChayTestApi>? tienTrinh = null,
        CancellationToken ct = default)
    {
        var batDauLuc = DateTimeOffset.Now;
        var ketQuaPhien = new List<KetQuaChay>();
        var dungDoLoi = false;

        try
        {
            BaoTienTrinh(tienTrinh, 0, yeuCau.DanhSachTestCase.Count, "Đang nạp dữ liệu seed...");
            var duLieuSeed = await new SeedLoadCheck()
                .TaiAsync(yeuCau.CauHinh.ChuoiKetNoiSqlServer, ct);
            var capNhatDB = new CapNhatDB(yeuCau.CauHinh.ChuoiKetNoiSqlServer, duLieuSeed);

            using var mayKhach = new MayKhachApi(yeuCau.CauHinh);
            var nguCanh = new NguCanhKiemThu(yeuCau.CauHinh, mayKhach, capNhatDB);

            for (var i = 0; i < yeuCau.DanhSachTestCase.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var kichBan = yeuCau.DanhSachTestCase[i];
                BaoTienTrinh(
                    tienTrinh,
                    i,
                    yeuCau.DanhSachTestCase.Count,
                    $"Đang chạy {i + 1}/{yeuCau.DanhSachTestCase.Count}: {kichBan.Ma}",
                    kichBan);

                var ketQua = await ChayMotKichBanAsync(kichBan, nguCanh, ct);
                ketQuaPhien.Add(ketQua);
                BaoTienTrinh(
                    tienTrinh,
                    i + 1,
                    yeuCau.DanhSachTestCase.Count,
                    $"Đã chạy {i + 1}/{yeuCau.DanhSachTestCase.Count}: {kichBan.Ma}",
                    kichBan,
                    ketQua);

                if (yeuCau.DungKhiLoi && DinhDangKetQuaKiemThu.LaKhongDat(ketQua.TrangThai))
                {
                    dungDoLoi = true;
                    break;
                }
            }

            return new KetQuaPhienChayApi(
                batDauLuc,
                DateTimeOffset.Now,
                ketQuaPhien,
                BiHuy: false,
                DungDoLoi: dungDoLoi);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return new KetQuaPhienChayApi(
                batDauLuc,
                DateTimeOffset.Now,
                ketQuaPhien,
                BiHuy: true,
                DungDoLoi: dungDoLoi);
        }
    }

    private static async Task<KetQuaChay> ChayMotKichBanAsync(
        KichBanApi kichBan,
        NguCanhKiemThu nguCanh,
        CancellationToken cancellationToken)
    {
        var dongHo = Stopwatch.StartNew();
        YeuCauApi? yeuCau = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(kichBan.LyDoBoQuaCoDinh))
            {
                return TaoKetQua(kichBan, TrangThaiKetQua.BoQua, kichBan.LyDoBoQuaCoDinh!, dongHo.Elapsed);
            }

            if (kichBan.ChayRiengAsync is not null)
            {
                return await kichBan.ChayRiengAsync(nguCanh, cancellationToken);
            }

            yeuCau = await kichBan.TaoYeuCauAsync(nguCanh);
            cancellationToken.ThrowIfCancellationRequested();
            var response = await nguCanh.Api.GuiAsync(yeuCau, cancellationToken);
            dongHo.Stop();

            if (!BoKichBanDong.LaMaChapNhanBatKy(kichBan.MaChapNhan) &&
                !kichBan.MaChapNhan.Contains(response.MaSoSanh))
            {
                var thongDiep =
                    $"Server trả mã nghiệp vụ không đúng. Mong đợi: {string.Join(", ", kichBan.MaChapNhan)}; " +
                    $"thực tế: {response.MaSoSanh}. Message server: {response.Message ?? "(không có)"}.";
                return TaoKetQua(kichBan, TrangThaiKetQua.ThatBai, thongDiep, dongHo.Elapsed, yeuCau, response);
            }

            if (kichBan.KiemTraThemAsync is not null)
            {
                var kiemTraThem = await kichBan.KiemTraThemAsync(response, yeuCau, nguCanh);
                if (!kiemTraThem.Dat)
                {
                    return TaoKetQua(kichBan, TrangThaiKetQua.ThatBai,
                        $"Mã nghiệp vụ đúng nhưng dữ liệu trả về sai kỳ vọng: {kiemTraThem.Loi}",
                        dongHo.Elapsed, yeuCau, response);
                }
            }

            if (kichBan.SauKhiDatAsync is not null)
            {
                await kichBan.SauKhiDatAsync(response, yeuCau, nguCanh);
            }

            return TaoKetQua(kichBan, TrangThaiKetQua.Dat,
                "Đạt expected code và kiểm tra dữ liệu bổ sung.", dongHo.Elapsed, yeuCau, response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (BoQuaKiemThuException ex)
        {
            dongHo.Stop();
            return TaoKetQua(kichBan, TrangThaiKetQua.BoQua, ex.Message, dongHo.Elapsed, yeuCau);
        }
        catch (LoiChuanBiKiemThuException ex)
        {
            dongHo.Stop();
            return TaoKetQua(kichBan, TrangThaiKetQua.LoiChuanBi, ex.Message, dongHo.Elapsed, yeuCau);
        }
        catch (HttpRequestException ex)
        {
            dongHo.Stop();
            return TaoKetQua(kichBan, TrangThaiKetQua.LoiMoiTruong,
                $"Không gọi được API. Kiểm tra base URL/server/network. Chi tiết: {ex.Message}",
                dongHo.Elapsed, yeuCau);
        }
        catch (TaskCanceledException)
        {
            dongHo.Stop();
            return TaoKetQua(kichBan, TrangThaiKetQua.LoiMoiTruong,
                "Gọi API quá thời gian chờ. Kiểm tra server hoặc tăng timeout.",
                dongHo.Elapsed, yeuCau);
        }
        catch (Exception ex)
        {
            dongHo.Stop();
            return TaoKetQua(kichBan, TrangThaiKetQua.LoiChuanBi,
                $"Lỗi trong mã test, chưa kết luận được server đúng/sai: {ex.Message}",
                dongHo.Elapsed, yeuCau);
        }
    }

    private static KetQuaChay TaoKetQua(
        KichBanApi kichBan,
        TrangThaiKetQua trangThai,
        string thongDiep,
        TimeSpan thoiGian,
        YeuCauApi? yeuCau = null,
        PhanHoiApi? response = null)
    {
        return BoChuyenDoiKetQuaChayTest.TaoKetQua(
            kichBan,
            trangThai,
            thongDiep,
            thoiGian,
            yeuCau,
            response);
    }

    private static void BaoTienTrinh(
        IProgress<TienTrinhChayTestApi>? tienTrinh,
        int dangChayThu,
        int tongSo,
        string trangThai,
        KichBanApi? kichBan = null,
        KetQuaChay? ketQua = null)
    {
        tienTrinh?.Report(new TienTrinhChayTestApi(dangChayThu, tongSo, trangThai, kichBan, ketQua));
    }
}
