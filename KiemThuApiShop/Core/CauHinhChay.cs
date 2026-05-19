using System.Text.Json;
using System.Text.Json.Serialization;

namespace KiemThuApiShop.Core;

public sealed record CauHinhChay
{
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; init; } = "http://localhost:8000";

    [JsonPropertyName("timeoutGiay")]
    public int TimeoutGiay { get; init; } = 60;

    [JsonPropertyName("tuDongChuanBiDuLieu")]
    public bool TuDongChuanBiDuLieu { get; init; } = true;

    [JsonPropertyName("connectionStrings")]
    public Dictionary<string, string> ConnectionStrings { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ApiShopTestDb"] = "Server=.;Database=ApiShopTestDb;Trusted_Connection=True;TrustServerCertificate=True"
    };

    [JsonPropertyName("thuMucKetQua")]
    public string ThuMucKetQua { get; init; } = "ket-qua";

    [JsonPropertyName("soTaiKhoanSeed")]
    public int SoTaiKhoanSeed { get; init; } = 100;

    [JsonPropertyName("soTaiKhoanDangKyTruoc")]
    public int SoTaiKhoanDangKyTruoc { get; init; } = 10;

    [JsonIgnore]
    public string ChuoiKetNoiSqlServer =>
        ConnectionStrings.TryGetValue("ApiShopTestDb", out var chuoiKetNoi) && !string.IsNullOrWhiteSpace(chuoiKetNoi)
            ? chuoiKetNoi
            : throw new InvalidOperationException("Thiếu connectionStrings.ApiShopTestDb trong appsettings.json.");

    public static CauHinhChay DocTuFile(string duongDan)
    {
        if (!File.Exists(duongDan))
        {
            return new CauHinhChay();
        }

        var json = File.ReadAllText(duongDan);
        return JsonSerializer.Deserialize<CauHinhChay>(json, TuyChonJson.MacDinh) ?? new CauHinhChay();
    }

    public static CauHinhChay GhiDeBangThamSoDongLenh(CauHinhChay cauHinh, string[] args)
    {
        var baseUrl = ThamSoDongLenh.LayGiaTri(args, "--base-url") ?? cauHinh.BaseUrl;
        var connectionString = ThamSoDongLenh.LayGiaTri(args, "--connection-string");
        var timeout = ThamSoDongLenh.LayGiaTriInt(args, "--timeout", cauHinh.TimeoutGiay);
        var khongChuanBi = ThamSoDongLenh.CoCo(args, "--khong-chuan-bi-du-lieu");
        var connectionStrings = new Dictionary<string, string>(cauHinh.ConnectionStrings, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            connectionStrings["ApiShopTestDb"] = connectionString;
        }

        return cauHinh with
        {
            BaseUrl = baseUrl.TrimEnd('/'),
            ConnectionStrings = connectionStrings,
            TimeoutGiay = timeout,
            TuDongChuanBiDuLieu = khongChuanBi ? false : cauHinh.TuDongChuanBiDuLieu
        };
    }
}

public static class ThamSoDongLenh
{
    public static bool CoCo(string[] args, string tenCo)
    {
        return args.Any(x => string.Equals(x, tenCo, StringComparison.OrdinalIgnoreCase));
    }

    public static string? LayGiaTri(string[] args, string tenCo)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], tenCo, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    public static int LayGiaTriInt(string[] args, string tenCo, int macDinh)
    {
        var giaTri = LayGiaTri(args, tenCo);
        return int.TryParse(giaTri, out var so) ? so : macDinh;
    }

    public static IReadOnlyList<string> LayNhieuGiaTri(string[] args, string tenCo)
    {
        var ketQua = new List<string>();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (!string.Equals(args[i], tenCo, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ketQua.AddRange(args[i + 1]
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        }

        return ketQua;
    }
}

public static class TuyChonJson
{
    public static readonly JsonSerializerOptions MacDinh = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
