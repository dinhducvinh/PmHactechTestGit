using System.Collections.Concurrent;
using System.Diagnostics;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private const int SoTaiKhoanKiemThuTai = 100;
    private const int SoLuongChaySongSong = 100;

    private static void ThemKichBanTai(List<KichBanApi> ds)
    {
        ThemKichBanTaiDangNhap(ds);
        ThemKichBanTaiAuthMe(ds);
        ThemKichBanTaiUserInfo(ds);
    }

    private static void ThemKichBanTaiDangNhap(List<KichBanApi> ds)
    {
        ThemTai(ds,
            "LOAD-LOGIN-100-01",
            "Đăng nhập đồng thời 100 tài khoản seed",
            "Gửi 100 request POST /auth/login cùng lúc, mỗi request dùng một tài khoản seed đã đăng ký khác nhau.",
            loiToiDaPhanTram: 10,
            tbToiDaMs: 3000,
            p95ToiDaMs: 8000,
            (ctx, ct) => ChayTaiAsync(
                ctx,
                "LOAD-LOGIN-100-01",
                "Đăng nhập đồng thời 100 tài khoản seed",
                "POST /auth/login x100",
                loiToiDaPhanTram: 10,
                tbToiDaMs: 3000,
                p95ToiDaMs: 8000,
                ct,
                (taiKhoan, _) => Task.FromResult(new YeuCauApi(
                    HttpMethod.Post,
                    "/auth/login",
                    TaoBodyDangNhap(taiKhoan)))));
    }

    private static void ThemKichBanTaiAuthMe(List<KichBanApi> ds)
    {
        ThemTai(ds,
            "LOAD-AUTH-ME-100-01",
            "100 tài khoản gửi GET /auth/me đồng thời",
            "Chuẩn bị token cho 100 tài khoản seed rồi gửi GET /auth/me cùng lúc bằng token riêng của từng tài khoản.",
            loiToiDaPhanTram: 10,
            tbToiDaMs: 2000,
            p95ToiDaMs: 6000,
            async (ctx, ct) =>
            {
                var taiKhoan = LayMotTramTaiKhoanDaDangKy(ctx);
                var tokenTheoTaiKhoan = await LayTokenChoTaiKhoanAsync(ctx, taiKhoan, ct);
                return await ChayTaiAsync(
                    ctx,
                    "LOAD-AUTH-ME-100-01",
                    "100 tài khoản gửi GET /auth/me đồng thời",
                    "GET /auth/me x100",
                    loiToiDaPhanTram: 10,
                    tbToiDaMs: 2000,
                    p95ToiDaMs: 6000,
                    ct,
                    (tk, _) => Task.FromResult(new YeuCauApi(
                        HttpMethod.Get,
                        "/auth/me",
                        token: tokenTheoTaiKhoan[tk.SoThuTu])),
                    taiKhoan);
            });
    }

    private static void ThemKichBanTaiUserInfo(List<KichBanApi> ds)
    {
        ThemTai(ds,
            "LOAD-USER-INFO-100-01",
            "100 tài khoản gửi POST /users/get_user_info đồng thời",
            "Chuẩn bị token cho 100 tài khoản seed rồi gửi POST /users/get_user_info cùng lúc, mỗi tài khoản đọc hộ sơ của chính mình.",
            loiToiDaPhanTram: 10,
            tbToiDaMs: 2500,
            p95ToiDaMs: 7000,
            async (ctx, ct) =>
            {
                var taiKhoan = LayMotTramTaiKhoanDaDangKy(ctx);
                var tokenTheoTaiKhoan = await LayTokenChoTaiKhoanAsync(ctx, taiKhoan, ct);
                return await ChayTaiAsync(
                    ctx,
                    "LOAD-USER-INFO-100-01",
                    "100 tài khoản gửi POST /users/get_user_info đồng thời",
                    "POST /users/get_user_info x100",
                    loiToiDaPhanTram: 10,
                    tbToiDaMs: 2500,
                    p95ToiDaMs: 7000,
                    ct,
                    (tk, _) => Task.FromResult(new YeuCauApi(
                        HttpMethod.Post,
                        "/users/get_user_info",
                        new Dictionary<string, object?>(),
                        tokenTheoTaiKhoan[tk.SoThuTu])),
                    taiKhoan);
            });
    }

    private static void ThemTai(
        List<KichBanApi> ds,
        string ma,
        string ten,
        string moTa,
        double loiToiDaPhanTram,
        int tbToiDaMs,
        int p95ToiDaMs,
        Func<NguCanhKiemThu, CancellationToken, Task<KetQuaChay>> chayRieng)
    {
        ds.Add(new KichBanApi
        {
            Ma = ma,
            Nhom = "Tải",
            TenHienThi = ten,
            MoTa = $"{moTa} PASS nếu lỗi <= {loiToiDaPhanTram:0.##}%, TB <= {tbToiDaMs} ms và P95 <= {p95ToiDaMs} ms.",
            TaoYeuCauAsync = _ => Req(HttpMethod.Get, "/"),
            ChayRiengAsync = chayRieng,
            MaChapNhan = Ok
        });
    }

    private static async Task<KetQuaChay> ChayTaiAsync(
        NguCanhKiemThu ctx,
        string ma,
        string ten,
        string endpoint,
        double loiToiDaPhanTram,
        int tbToiDaMs,
        int p95ToiDaMs,
        CancellationToken cancellationToken,
        Func<TaiKhoanSignupThanhCongSeed, int, Task<YeuCauApi>> taoYeuCau,
        IReadOnlyList<TaiKhoanSignupThanhCongSeed>? danhSachTaiKhoan = null)
    {
        var taiKhoan = danhSachTaiKhoan ?? LayMotTramTaiKhoanDaDangKy(ctx);
        var dongHoTong = Stopwatch.StartNew();
        var ketQua = await GuiSongSongAsync(ctx, taiKhoan, SoLuongChaySongSong, taoYeuCau, cancellationToken);
        dongHoTong.Stop();

        var tong = ketQua.Count;
        var dat = ketQua.Count(x => x.DatMa);
        var loi = tong - dat;
        var tiLeLoi = tong == 0 ? 100 : loi * 100d / tong;
        var tbMs = tong == 0 ? 0 : ketQua.Average(x => x.ThoiGian.TotalMilliseconds);
        var p95Ms = TinhP95Ms(ketQua);
        var datNguong = tiLeLoi <= loiToiDaPhanTram && tbMs <= tbToiDaMs && p95Ms <= p95ToiDaMs;
        var tatCaLoiMoiTruong = dat == 0 && ketQua.All(x => x.LoiMoiTruong);
        var thongDiep = TaoThongDiepTai(ketQua, loiToiDaPhanTram, tbToiDaMs, p95ToiDaMs, dongHoTong.Elapsed);

        return new KetQuaChay
        {
            Ma = ma,
            Nhom = "Tải",
            TenHienThi = ten,
            TrangThai = tatCaLoiMoiTruong
                ? TrangThaiKetQua.LoiMoiTruong
                : datNguong ? TrangThaiKetQua.Dat : TrangThaiKetQua.ThatBai,
            ThongDiep = thongDiep,
            MaMongDoi = $"100 request, code 1000, lỗi <= {loiToiDaPhanTram:0.##}%, TB <= {tbToiDaMs} ms, P95 <= {p95ToiDaMs} ms",
            MaThucTe = $"Đạt {dat}/{tong}; lỗi {tiLeLoi:0.##}%; TB {tbMs:0} ms; P95 {p95Ms:0} ms",
            ThoiGian = dongHoTong.Elapsed,
            Endpoint = $"{endpoint}, song song {SoLuongChaySongSong}",
            ResponseRutGon = thongDiep
        };
    }

    private static IReadOnlyList<TaiKhoanSignupThanhCongSeed> LayMotTramTaiKhoanDaDangKy(NguCanhKiemThu ctx)
    {
        var taiKhoan = ctx.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x =>
                x.TaiKhoanIdServer is > 0 &&
                !string.IsNullOrWhiteSpace(x.SoDienThoai) &&
                !string.IsNullOrWhiteSpace(x.MatKhauHienTai))
            .OrderBy(x => x.SoThuTu)
            .Take(SoTaiKhoanKiemThuTai)
            .ToList();

        if (taiKhoan.Count < SoTaiKhoanKiemThuTai)
        {
            throw new LoiChuanBiKiemThuException(
                $"Cần {SoTaiKhoanKiemThuTai} tài khoản seed đã đăng ký để chạy test tải, hiện chỉ có {taiKhoan.Count}. Hãy bấm Kiểm tra seed để chuẩn bị đủ dữ liệu mới.");
        }

        return taiKhoan;
    }

    private static async Task<Dictionary<int, string>> LayTokenChoTaiKhoanAsync(
        NguCanhKiemThu ctx,
        IReadOnlyList<TaiKhoanSignupThanhCongSeed> taiKhoan,
        CancellationToken cancellationToken)
    {
        var tokenTheoTaiKhoan = new ConcurrentDictionary<int, string>();
        var loi = new ConcurrentBag<string>();
        using var gioiHan = new SemaphoreSlim(20);

        var congViec = taiKhoan.Select(async tk =>
        {
            await gioiHan.WaitAsync(cancellationToken);
            try
            {
                var response = await ctx.Api.GuiAsync(
                    new YeuCauApi(HttpMethod.Post, "/auth/login", TaoBodyDangNhap(tk)),
                    cancellationToken);
                var token = response.MaSoSanh == "1000"
                    ? response.Data?["token"]?.ToString()
                    : null;

                if (string.IsNullOrWhiteSpace(token))
                {
                    loi.Add($"tk_id_server {tk.TaiKhoanIdServer}: login trả {response.MaSoSanh}");
                    return;
                }

                tokenTheoTaiKhoan[tk.SoThuTu] = token;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                loi.Add($"tk_id_server {tk.TaiKhoanIdServer}: {RutGon(ex.Message, 160)}");
            }
            finally
            {
                gioiHan.Release();
            }
        });

        await Task.WhenAll(congViec);

        if (tokenTheoTaiKhoan.Count < taiKhoan.Count)
        {
            var mauLoi = string.Join(" | ", loi.Take(5));
            throw new LoiChuanBiKiemThuException(
                $"Không lấy đủ token cho {taiKhoan.Count} tài khoản seed. Lấy được {tokenTheoTaiKhoan.Count}/{taiKhoan.Count}. {mauLoi}");
        }

        return tokenTheoTaiKhoan.ToDictionary(x => x.Key, x => x.Value);
    }

    private static async Task<IReadOnlyList<KetQuaLanTai>> GuiSongSongAsync(
        NguCanhKiemThu ctx,
        IReadOnlyList<TaiKhoanSignupThanhCongSeed> taiKhoan,
        int soLuongSongSong,
        Func<TaiKhoanSignupThanhCongSeed, int, Task<YeuCauApi>> taoYeuCau,
        CancellationToken cancellationToken)
    {
        using var gioiHan = new SemaphoreSlim(soLuongSongSong);

        var congViec = taiKhoan.Select(async (tk, index) =>
        {
            await gioiHan.WaitAsync(cancellationToken);
            try
            {
                return await GuiMotYeuCauTaiAsync(ctx, tk, index, taoYeuCau, cancellationToken);
            }
            finally
            {
                gioiHan.Release();
            }
        });

        return await Task.WhenAll(congViec);
    }

    private static async Task<KetQuaLanTai> GuiMotYeuCauTaiAsync(
        NguCanhKiemThu ctx,
        TaiKhoanSignupThanhCongSeed taiKhoan,
        int index,
        Func<TaiKhoanSignupThanhCongSeed, int, Task<YeuCauApi>> taoYeuCau,
        CancellationToken cancellationToken)
    {
        var dongHo = Stopwatch.StartNew();
        try
        {
            var yeuCau = await taoYeuCau(taiKhoan, index);
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ctx.Api.GuiAsync(yeuCau, cancellationToken);
            dongHo.Stop();

            var datMa = Ok.Contains(response.MaSoSanh);
            var loi = datMa
                ? null
                : $"tk_id_server {taiKhoan.TaiKhoanIdServer}: mã thực tế {response.MaSoSanh}, HTTP {(int)response.HttpStatusCode}, message {response.Message ?? "(không có)"}";
            return new KetQuaLanTai(datMa, response.MaSoSanh, dongHo.Elapsed, loi, LoiMoiTruong: false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            dongHo.Stop();
            return new KetQuaLanTai(false, "TIMEOUT", dongHo.Elapsed, $"tk_id_server {taiKhoan.TaiKhoanIdServer}: quá timeout. {ex.Message}", LoiMoiTruong: true);
        }
        catch (HttpRequestException ex)
        {
            dongHo.Stop();
            return new KetQuaLanTai(false, "ENV_ERROR", dongHo.Elapsed, $"tk_id_server {taiKhoan.TaiKhoanIdServer}: không gửi được API. {ex.Message}", LoiMoiTruong: true);
        }
        catch (Exception ex)
        {
            dongHo.Stop();
            return new KetQuaLanTai(false, "ERROR", dongHo.Elapsed, $"tk_id_server {taiKhoan.TaiKhoanIdServer}: {RutGon(ex.Message, 180)}", LoiMoiTruong: false);
        }
    }

    private static string RutGon(string? raw, int max)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "";
        }

        var motDong = raw.Replace(Environment.NewLine, " ");
        return motDong.Length <= max ? motDong : motDong[..max] + "...";
    }

    private static Dictionary<string, object?> TaoBodyDangNhap(TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        return Obj(
            ("phone_number", taiKhoan.SoDienThoai),
            ("password", taiKhoan.MatKhauHienTai),
            ("devtoken", taiKhoan.UuidThietBi));
    }

    private static double TinhP95Ms(IReadOnlyList<KetQuaLanTai> ketQua)
    {
        if (ketQua.Count == 0)
        {
            return 0;
        }

        var thoiGian = ketQua
            .Select(x => x.ThoiGian.TotalMilliseconds)
            .OrderBy(x => x)
            .ToList();
        var viTri = Math.Clamp((int)Math.Ceiling(thoiGian.Count * 0.95) - 1, 0, thoiGian.Count - 1);
        return thoiGian[viTri];
    }

    private static string TaoThongDiepTai(
        IReadOnlyList<KetQuaLanTai> ketQua,
        double loiToiDaPhanTram,
        int tbToiDaMs,
        int p95ToiDaMs,
        TimeSpan thoiGianTong)
    {
        var tong = ketQua.Count;
        var dat = ketQua.Count(x => x.DatMa);
        var loi = tong - dat;
        var tiLeLoi = tong == 0 ? 100 : loi * 100d / tong;
        var tbMs = tong == 0 ? 0 : ketQua.Average(x => x.ThoiGian.TotalMilliseconds);
        var p95Ms = TinhP95Ms(ketQua);
        var maThucTe = string.Join(", ", ketQua
            .GroupBy(x => x.MaThucTe)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key)
            .Take(5)
            .Select(x => $"{x.Key}: {x.Count()}"));
        var mauLoi = string.Join(" | ", ketQua
            .Where(x => !x.DatMa && !string.IsNullOrWhiteSpace(x.Loi))
            .Select(x => x.Loi!)
            .Distinct()
            .Take(3));

        var thongDiep =
            $"Chạy {tong} request, song song {SoLuongChaySongSong}. Đạt {dat}/{tong}, lỗi {loi} ({tiLeLoi:0.##}%). " +
            $"TB {tbMs:0} ms, P95 {p95Ms:0} ms, tổng {(int)Math.Round(thoiGianTong.TotalMilliseconds)} ms. " +
            $"Ngưỡng: lỗi <= {loiToiDaPhanTram:0.##}%, TB <= {tbToiDaMs} ms, P95 <= {p95ToiDaMs} ms. Mã thực tế: {maThucTe}.";

        return string.IsNullOrWhiteSpace(mauLoi)
            ? thongDiep
            : $"{thongDiep} Mẫu lỗi: {mauLoi}";
    }

    private sealed record KetQuaLanTai(
        bool DatMa,
        string MaThucTe,
        TimeSpan ThoiGian,
        string? Loi,
        bool LoiMoiTruong);
}







