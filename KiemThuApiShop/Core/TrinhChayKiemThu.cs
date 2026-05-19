using System.Diagnostics;

namespace KiemThuApiShop.Core;

public sealed class TrinhChayKiemThu
{
    private readonly NguCanhKiemThu _nguCanh;

    public TrinhChayKiemThu(NguCanhKiemThu nguCanh)
    {
        _nguCanh = nguCanh;
    }

    public async Task<IReadOnlyList<KetQuaChay>> ChayAsync(IReadOnlyList<KichBanApi> danhSach)
    {
        var ketQua = new List<KetQuaChay>();
        var tong = danhSach.Count;

        for (var i = 0; i < tong; i++)
        {
            var kichBan = danhSach[i];
            Console.WriteLine($"[{i + 1}/{tong}] {kichBan.Ma} - {kichBan.TenHienThi}");
            var dongHo = Stopwatch.StartNew();
            YeuCauApi? yeuCau = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(kichBan.LyDoBoQuaCoDinh))
                {
                    ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.BoQua, kichBan.LyDoBoQuaCoDinh!, dongHo.Elapsed));
                    InDongKetQua(ketQua[^1]);
                    continue;
                }

                yeuCau = await kichBan.TaoYeuCauAsync(_nguCanh);
                var response = await _nguCanh.Api.GuiAsync(yeuCau);
                dongHo.Stop();

                if (!kichBan.MaChapNhan.Contains(response.MaSoSanh))
                {
                    var thongDiep =
                        $"Server trả mã nghiệp vụ không đúng. Mong đợi: {string.Join(", ", kichBan.MaChapNhan)}; thực tế: {response.MaSoSanh}. " +
                        $"Message server: {response.Message ?? "(không có)"}. Cần kiểm tra logic server hoặc tài liệu expected.";

                    ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.ThatBai, thongDiep, dongHo.Elapsed, yeuCau, response));
                    InDongKetQua(ketQua[^1]);
                    continue;
                }

                if (kichBan.KiemTraThemAsync is not null)
                {
                    var kiemTraThem = await kichBan.KiemTraThemAsync(response, yeuCau, _nguCanh);
                    if (!kiemTraThem.Dat)
                    {
                        var thongDiep = $"Mã nghiệp vụ đúng nhưng dữ liệu trả về sai kỳ vọng: {kiemTraThem.Loi}";
                        ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.ThatBai, thongDiep, dongHo.Elapsed, yeuCau, response));
                        InDongKetQua(ketQua[^1]);
                        continue;
                    }
                }

                if (kichBan.SauKhiDatAsync is not null)
                {
                    await kichBan.SauKhiDatAsync(response, yeuCau, _nguCanh);
                }

                ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.Dat, "Đạt expected code và kiểm tra dữ liệu bổ sung.", dongHo.Elapsed, yeuCau, response));
                InDongKetQua(ketQua[^1]);
            }
            catch (BoQuaKiemThuException ex)
            {
                dongHo.Stop();
                ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.BoQua, ex.Message, dongHo.Elapsed, yeuCau));
                InDongKetQua(ketQua[^1]);
            }
            catch (LoiChuanBiKiemThuException ex)
            {
                dongHo.Stop();
                ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.LoiChuanBi, ex.Message, dongHo.Elapsed, yeuCau));
                InDongKetQua(ketQua[^1]);
            }
            catch (HttpRequestException ex)
            {
                dongHo.Stop();
                ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.LoiMoiTruong,
                    $"Không gọi được API. Kiểm tra base URL/server/network. Chi tiết: {ex.Message}", dongHo.Elapsed, yeuCau));
                InDongKetQua(ketQua[^1]);
            }
            catch (TaskCanceledException)
            {
                dongHo.Stop();
                ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.LoiMoiTruong,
                    "Gọi API quá thời gian chờ. Kiểm tra server hoặc tăng --timeout.", dongHo.Elapsed, yeuCau));
                InDongKetQua(ketQua[^1]);
            }
            catch (Exception ex)
            {
                dongHo.Stop();
                ketQua.Add(TaoKetQua(kichBan, TrangThaiKetQua.LoiChuanBi,
                    $"Lỗi trong mã test, chưa kết luận được server đúng/sai: {ex.Message}", dongHo.Elapsed, yeuCau));
                InDongKetQua(ketQua[^1]);
            }
        }

        return ketQua;
    }

    private static KetQuaChay TaoKetQua(
        KichBanApi kichBan,
        TrangThaiKetQua trangThai,
        string thongDiep,
        TimeSpan thoiGian,
        YeuCauApi? yeuCau = null,
        PhanHoiApi? response = null)
    {
        return new KetQuaChay
        {
            Ma = kichBan.Ma,
            Nhom = kichBan.Nhom,
            TenHienThi = kichBan.TenHienThi,
            TrangThai = trangThai,
            ThongDiep = thongDiep,
            MaMongDoi = string.Join(", ", kichBan.MaChapNhan),
            MaThucTe = response?.MaSoSanh,
            HttpStatus = response is null ? null : (int)response.HttpStatusCode,
            ThoiGian = response?.ThoiGianPhanHoi ?? thoiGian,
            Endpoint = yeuCau is null ? null : $"{yeuCau.PhuongThuc} {yeuCau.DuongDan}",
            ResponseRutGon = TienIchJson.RutGon(response?.NoiDungRaw)
        };
    }

    private static void InDongKetQua(KetQuaChay ketQua)
    {
        var mauCu = Console.ForegroundColor;
        Console.ForegroundColor = ketQua.TrangThai switch
        {
            TrangThaiKetQua.Dat => ConsoleColor.Green,
            TrangThaiKetQua.BoQua => ConsoleColor.Yellow,
            TrangThaiKetQua.LoiChuanBi => ConsoleColor.Yellow,
            _ => ConsoleColor.Red
        };

        Console.WriteLine($"    {ketQua.TrangThai}: {ketQua.ThongDiep}");
        Console.ForegroundColor = mauCu;
    }
}
