using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;

namespace HactechTest.ApiShopTesting.Core;

public enum TrangThaiKetQua
{
    Dat,
    ThatBai,
    BoQua,
    LoiChuanBi,
    LoiMoiTruong
}

public sealed class YeuCauApi
{
    public YeuCauApi(HttpMethod phuongThuc, string duongDan, object? body = null, string? token = null)
    {
        PhuongThuc = phuongThuc;
        DuongDan = duongDan;
        Body = body;
        Token = token;
    }

    public HttpMethod PhuongThuc { get; }
    public string DuongDan { get; }
    public object? Body { get; }
    public string? Token { get; }
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, object?> Tam { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class PhanHoiApi
{
    public HttpStatusCode HttpStatusCode { get; init; }
    public string NoiDungRaw { get; init; } = "";
    public JsonNode? Json { get; init; }
    public string? Code { get; init; }
    public string? Message { get; init; }
    public JsonNode? Data { get; init; }
    public TimeSpan ThoiGianPhanHoi { get; init; }

    public string MaSoSanh => string.IsNullOrWhiteSpace(Code)
        ? $"HTTP_{(int)HttpStatusCode}"
        : Code!;
}

public sealed class KichBanApi
{
    public required string Ma { get; init; }
    public required string Nhom { get; init; }
    public required string TenHienThi { get; init; }
    public required string MoTa { get; init; }
    public required Func<NguCanhKiemThu, Task<YeuCauApi>> TaoYeuCauAsync { get; init; }
    public Func<NguCanhKiemThu, CancellationToken, Task<KetQuaChay>>? ChayRiengAsync { get; init; }
    public required IReadOnlySet<string> MaChapNhan { get; init; }
    public Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>>? KiemTraThemAsync { get; init; }
    public Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task>? SauKhiDatAsync { get; init; }
    public string? LyDoBoQuaCoDinh { get; init; }
}

public sealed record KetQuaKiemTraThem(bool Dat, string? Loi)
{
    public static readonly KetQuaKiemTraThem ThanhCong = new(true, null);
}

public sealed class KetQuaChay
{
    public required string Ma { get; init; }
    public required string Nhom { get; init; }
    public required string TenHienThi { get; init; }
    public required TrangThaiKetQua TrangThai { get; init; }
    public required string ThongDiep { get; init; }
    public string? MaMongDoi { get; init; }
    public string? MaThucTe { get; init; }
    public int? HttpStatus { get; init; }
    public TimeSpan ThoiGian { get; init; }
    public string? Endpoint { get; init; }
    public string? RequestBodyJson { get; init; }
    public string? ResponseRutGon { get; init; }
}

public sealed class BoQuaKiemThuException : Exception
{
    public BoQuaKiemThuException(string message) : base(message)
    {
    }
}

public sealed class LoiChuanBiKiemThuException : Exception
{
    public LoiChuanBiKiemThuException(string message) : base(message)
    {
    }
}


