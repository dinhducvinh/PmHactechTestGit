using System.Text.Json;
using System.Text.Json.Serialization;
using HactechTest.ApiShopTesting.Core;
using HactechTest.Services.History;

namespace HactechTest.Services.ChayTest;

public static class BoChuyenDoiKetQuaChayTest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static KetQuaChay TaoKetQua(
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
            RequestBodyJson = TaoRequestBodyJson(yeuCau?.Body),
            ResponseRutGon = RutGon(response?.NoiDungRaw, 2000)
        };
    }

    public static ChiTietKetQuaTestCase TaoKetQuaLuuPhien(
        KetQuaChay ketQua,
        int soThuTu,
        string baseUrl)
    {
        var (method, path) = TachEndpoint(ketQua.Endpoint);
        return new ChiTietKetQuaTestCase
        {
            SequenceNo = soThuTu,
            DisplayName = $"{ketQua.Ma} - {ketQua.TenHienThi}",
            HttpMethod = method,
            Url = TaoUrlDayDu(baseUrl, path),
            ExpectedAppCode = ketQua.MaMongDoi ?? "",
            ActualAppCode = ketQua.MaThucTe,
            HttpStatus = ketQua.HttpStatus,
            Result = DinhDangKetQuaKiemThu.TrangThaiLuuDatabase(ketQua.TrangThai),
            DurationMs = (long)Math.Round(ketQua.ThoiGian.TotalMilliseconds),
            Reason = ketQua.ThongDiep,
            RequestBodyJson = ketQua.RequestBodyJson,
            ResponseBody = ketQua.ResponseRutGon
        };
    }

    public static TestCaseResultDetailData TaoChiTietKetQuaTest(KetQuaChay ketQua)
    {
        return new TestCaseResultDetailData
        {
            SequenceNo = "",
            TestCaseName = ketQua.TenHienThi,
            Result = DinhDangKetQuaKiemThu.TrangThaiHienThi(ketQua.TrangThai),
            ActualStatus = ketQua.MaThucTe ?? (ketQua.HttpStatus.HasValue ? "HTTP " + ketQua.HttpStatus.Value : "-"),
            ExpectedStatus = ketQua.MaMongDoi ?? "-",
            Duration = ((int)Math.Round(ketQua.ThoiGian.TotalMilliseconds)).ToString(),
            Reason = ketQua.ThongDiep,
            Method = ketQua.Endpoint?.Split(' ').FirstOrDefault() ?? "",
            Url = ketQua.Endpoint ?? "",
            ResponseBody = ketQua.ResponseRutGon ?? ""
        };
    }

    private static string? TaoRequestBodyJson(object? body)
    {
        if (body is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(body, JsonOptions);
        }
        catch
        {
            return body.ToString();
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

    private static (string Method, string Path) TachEndpoint(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return ("", "");
        }

        var parts = endpoint.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : ("", endpoint);
    }

    private static string TaoUrlDayDu(string baseUrl, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return baseUrl;
        }

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return (baseUrl ?? "").TrimEnd('/') + "/" + path.TrimStart('/');
    }
}
