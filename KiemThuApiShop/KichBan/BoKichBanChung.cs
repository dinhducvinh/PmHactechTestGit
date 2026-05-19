using System.Text.Json.Nodes;
using KiemThuApiShop.Core;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> Ok = Tap("1000");
    private static readonly IReadOnlySet<string> ThieuThamSo = Tap("1002");
    private static readonly IReadOnlySet<string> SaiKieu = Tap("1003");
    private static readonly IReadOnlySet<string> SaiGiaTri = Tap("1004");
    private static readonly IReadOnlySet<string> SaiToken = Tap("9998", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> KhongCoDuLieu = Tap("1000", "9994");
    private static readonly IReadOnlySet<string> KhongCoSanPham = Tap("9992", "404", "HTTP_404");
    private static readonly IReadOnlySet<string> KhongCoQuyen = Tap("1009", "9998", "HTTP_401", "HTTP_403");

    public static IReadOnlyList<KichBanApi> TaoTatCaKichBan()
    {
        var ds = new List<KichBanApi>();
        ThemKichBanAuth(ds);
        ThemKichBanUser(ds);
        ThemKichBanProduct(ds);
        return ds;
    }

    private static IReadOnlySet<string> Tap(params string[] ma)
    {
        return new HashSet<string>(ma, StringComparer.OrdinalIgnoreCase);
    }

    private static void Them(
        List<KichBanApi> ds,
        string ma,
        string nhom,
        string ten,
        string moTa,
        Func<NguCanhKiemThu, Task<YeuCauApi>> taoYeuCau,
        IReadOnlySet<string> maChapNhan,
        Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>>? kiemTraThem = null,
        Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task>? sauKhiDat = null,
        string? lyDoBoQua = null)
    {
        ds.Add(new KichBanApi
        {
            Ma = ma,
            Nhom = nhom,
            TenHienThi = ten,
            MoTa = moTa,
            TaoYeuCauAsync = taoYeuCau,
            MaChapNhan = maChapNhan,
            KiemTraThemAsync = kiemTraThem,
            SauKhiDatAsync = sauKhiDat,
            LyDoBoQuaCoDinh = lyDoBoQua
        });
    }

    private static Task<YeuCauApi> Req(HttpMethod method, string path, object? body = null, string? token = null)
    {
        return Task.FromResult(new YeuCauApi(method, path, body, token));
    }

    private static Dictionary<string, object?> Obj(params (string key, object? value)[] pairs)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var (key, value) in pairs)
        {
            dict[key] = value;
        }

        return dict;
    }

    private static long SoId(string giaTri, string tenDuLieu)
    {
        if (long.TryParse(giaTri, out var id))
        {
            return id;
        }

        throw new BoQuaKiemThuException($"{tenDuLieu} trong seed phải là số để gọi API này. Giá trị hiện tại: {giaTri}");
    }

    private static long SoIdBatBuoc(string? giaTri, string tenDuLieu)
    {
        if (string.IsNullOrWhiteSpace(giaTri))
        {
            throw new BoQuaKiemThuException($"Thiếu {tenDuLieu} trong seed.");
        }

        return SoId(giaTri, tenDuLieu);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataCoTruong(params string[] truong)
    {
        return (response, _, _) =>
        {
            if (response.Data is null)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu data."));
            }

            foreach (var t in truong)
            {
                if (!TienIchJson.CoTruong(response.Data, t))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data thiếu trường `{t}`."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataLaMang(string? truongItemBatBuoc = null)
    {
        return (response, _, _) =>
        {
            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data không phải mảng."));
            }

            if (!string.IsNullOrWhiteSpace(truongItemBatBuoc) && mang.Count > 0 && !TienIchJson.CoTruong(mang[0], truongItemBatBuoc))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"item đầu tiên thiếu trường `{truongItemBatBuoc}`."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataBoolBang(string tenTruong, bool mongDoi)
    {
        return (response, _, _) =>
        {
            var giaTri = TienIchJson.DocBool(response.Data, tenTruong);
            if (giaTri != mongDoi)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.{tenTruong} phải bằng {mongDoi}, thực tế là {giaTri?.ToString() ?? "null"}."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraMangToiDa(int toiDa)
    {
        return (response, _, _) =>
        {
            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data không phải mảng."));
            }

            return Task.FromResult(mang.Count <= toiDa
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"data có {mang.Count} item, vượt count mong đợi {toiDa}."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraMangSapXepTheoSort()
    {
        return (response, _, _) =>
        {
            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data không phải mảng."));
            }

            int? truoc = null;
            foreach (var item in mang)
            {
                var sort = TienIchJson.DocSoNguyen(item, "sort");
                if (sort is null)
                {
                    continue;
                }

                if (truoc is not null && sort < truoc)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, "Trường sort không được sắp xếp tăng dần."));
                }

                truoc = (int)sort.Value;
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }
}
