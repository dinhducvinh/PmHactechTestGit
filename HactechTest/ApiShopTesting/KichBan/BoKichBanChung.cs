using HactechTest.ApiShopTesting.Core;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly Lazy<IReadOnlyList<KichBanApi>> TatCaKichBan = new(TaoTatCaKichBanNoiBo);

    private static readonly IReadOnlySet<string> Ok = Tap("1000");
    private static readonly IReadOnlySet<string> ThieuThamSo = Tap("1002");
    private static readonly IReadOnlySet<string> SaiKieu = Tap("1003");
    private static readonly IReadOnlySet<string> SaiGiaTri = Tap("1004");
    private static readonly IReadOnlySet<string> SaiToken = Tap("9998", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> KhongCoQuyen = Tap("1009", "9998", "HTTP_401", "HTTP_403");
    private static readonly IReadOnlySet<string> ThieuThamSoHoacSaiGiaTri = Tap("1002", "1004");
    private static readonly IReadOnlySet<string> SaiKieuHoacSaiGiaTri = Tap("1003", "1004");
    private static readonly IReadOnlySet<string> LoiThamSo = Tap("1002", "1003", "1004");
    private static readonly IReadOnlySet<string> SaiTokenHoacKhongCoNguoiDung = Tap("9998", "HTTP_401", "HTTP_403", "1013");
    private static readonly IReadOnlySet<string> OkHoacKhongCoNguoiDung = Tap("1000", "1013");
    private static readonly IReadOnlySet<string> KhongCoNguoiDung = Tap("1013");
    private static readonly IReadOnlySet<string> DaThucHienTruocDo = Tap("1010");

    public static IReadOnlyList<KichBanApi> TaoTatCaKichBan()
    {
        return TatCaKichBan.Value;
    }

    private static IReadOnlyList<KichBanApi> TaoTatCaKichBanNoiBo()
    {
        var ds = new List<KichBanApi>();
        ThemKichBanAuth(ds);
        ThemKichBanUser(ds);
        ThemKichBanProduct(ds);
        ThemKichBanSearch(ds);
        ThemKichBanOrder(ds);
        ThemKichBanWallet(ds);
        ThemKichBanRewards(ds);
        ThemKichBanFollowBlock(ds);
        ThemKichBanConversation(ds);
        ThemKichBanNotification(ds);
        ThemKichBanDevTokenPush(ds);
        ThemKichBanNews(ds);
        ThemKichBanTai(ds);
        return ds.AsReadOnly();
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
}


