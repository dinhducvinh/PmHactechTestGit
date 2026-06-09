using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
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
        var ds = new List<KichBanApi>();
        ThemKichBanAuth(ds);
        ThemKichBanUser(ds);
        ThemKichBanProduct(ds);
        ThemKichBanSearch(ds);
        ThemKichBanOrder(ds);
        ThemKichBanFollowBlock(ds);
        ThemKichBanConversation(ds);
        ThemKichBanNotification(ds);
        ThemKichBanDevTokenPush(ds);
        ThemKichBanTai(ds);
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

    private static bool LaMaThanhCong(PhanHoiApi response)
    {
        return string.Equals(response.MaSoSanh, "1000", StringComparison.OrdinalIgnoreCase);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataCoTruong(params string[] truong)
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

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
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

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
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var giaTri = TienIchJson.DocBool(response.Data, tenTruong);
            if (giaTri != mongDoi)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.{tenTruong} phải bằng {mongDoi}, thực tế là {giaTri?.ToString() ?? "null"}."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private sealed record CapHaiTaiKhoan(
        TaiKhoanSignupThanhCongSeed TaiKhoanThuNhat,
        TaiKhoanSignupThanhCongSeed TaiKhoanThuHai);

    private sealed record CapTaiKhoanDangTheoDoi(
        TaiKhoanTheoDoiSeed QuanHeTheoDoi,
        TaiKhoanSignupThanhCongSeed TaiKhoanTheoDoi,
        TaiKhoanSignupThanhCongSeed TaiKhoanDuocTheoDoi);

    private sealed record CapTaiKhoanDangChan(
        TaiKhoanChanSeed QuanHeChan,
        TaiKhoanSignupThanhCongSeed TaiKhoanChan,
        TaiKhoanSignupThanhCongSeed TaiKhoanBiChan);

    private static IReadOnlyList<TaiKhoanSignupThanhCongSeed> LayDanhSachTaiKhoanSanSang(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(TaiKhoanSanSang)
            .OrderBy(x => x.SoThuTu)
            .ThenBy(x => x.TaiKhoanIdServer)
            .ToList();
    }

    private static TaiKhoanSignupThanhCongSeed? LayTaiKhoanTheoServerId(NguCanhKiemThu ctx, string? taiKhoanIdServer)
    {
        if (string.IsNullOrWhiteSpace(taiKhoanIdServer))
        {
            return null;
        }

        return ctx.KhoSeed.DuLieu.TaiKhoanSignupThanhCongSeed.FirstOrDefault(x =>
            TaiKhoanSanSang(x) &&
            string.Equals(x.TaiKhoanIdServer, taiKhoanIdServer, StringComparison.Ordinal));
    }

    private static bool TaiKhoanSanSang(TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        return !string.IsNullOrWhiteSpace(taiKhoan.TaiKhoanIdServer);
    }

    private static IReadOnlyList<TaiKhoanTheoDoiSeed> LayDanhSachQuanHeTheoDoiDangHoatDong(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed
            .Where(x =>
                x.TrangThai == "dang_theo_doi" &&
                !string.IsNullOrWhiteSpace(x.FollowerTaiKhoanIdServer) &&
                !string.IsNullOrWhiteSpace(x.FolloweeTaiKhoanIdServer))
            .ToList();
    }

    private static IReadOnlyList<TaiKhoanChanSeed> LayDanhSachQuanHeChanDangHoatDong(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.TaiKhoanChanSeed
            .Where(x =>
                x.TrangThai == "dang_chan" &&
                !string.IsNullOrWhiteSpace(x.BlockerTaiKhoanIdServer) &&
                !string.IsNullOrWhiteSpace(x.BlockedTaiKhoanIdServer))
            .GroupBy(x => x.BlockerTaiKhoanIdServer)
            .OrderByDescending(x => x.Count())
            .SelectMany(x => x.OrderBy(y => y.ChanSeedId))
            .ToList();
    }

    private static CapTaiKhoanDangTheoDoi LayCapTaiKhoanDangTheoDoi(NguCanhKiemThu ctx, string thongBaoKhiThieu)
    {
        foreach (var quanHe in LayDanhSachQuanHeTheoDoiDangHoatDong(ctx))
        {
            var taiKhoanTheoDoi = LayTaiKhoanTheoServerId(ctx, quanHe.FollowerTaiKhoanIdServer);
            var taiKhoanDuocTheoDoi = LayTaiKhoanTheoServerId(ctx, quanHe.FolloweeTaiKhoanIdServer);
            if (taiKhoanTheoDoi is not null && taiKhoanDuocTheoDoi is not null)
            {
                return new CapTaiKhoanDangTheoDoi(quanHe, taiKhoanTheoDoi, taiKhoanDuocTheoDoi);
            }
        }

        throw new BoQuaKiemThuException(thongBaoKhiThieu);
    }

    private static CapTaiKhoanDangChan LayCapTaiKhoanDangChan(NguCanhKiemThu ctx, string thongBaoKhiThieu)
    {
        foreach (var quanHe in LayDanhSachQuanHeChanDangHoatDong(ctx))
        {
            var taiKhoanChan = LayTaiKhoanTheoServerId(ctx, quanHe.BlockerTaiKhoanIdServer);
            var taiKhoanBiChan = LayTaiKhoanTheoServerId(ctx, quanHe.BlockedTaiKhoanIdServer);
            if (taiKhoanChan is not null && taiKhoanBiChan is not null)
            {
                return new CapTaiKhoanDangChan(quanHe, taiKhoanChan, taiKhoanBiChan);
            }
        }

        throw new BoQuaKiemThuException(thongBaoKhiThieu);
    }

    private static bool CoQuanHeTheoDoi(NguCanhKiemThu ctx, string? followerTaiKhoanIdServer, string? followeeTaiKhoanIdServer)
    {
        if (string.IsNullOrWhiteSpace(followerTaiKhoanIdServer) || string.IsNullOrWhiteSpace(followeeTaiKhoanIdServer))
        {
            return false;
        }

        return LayDanhSachQuanHeTheoDoiDangHoatDong(ctx).Any(x =>
            x.FollowerTaiKhoanIdServer == followerTaiKhoanIdServer &&
            x.FolloweeTaiKhoanIdServer == followeeTaiKhoanIdServer);
    }

    private static bool CoQuanHeChan(NguCanhKiemThu ctx, string? taiKhoanIdServerA, string? taiKhoanIdServerB)
    {
        if (string.IsNullOrWhiteSpace(taiKhoanIdServerA) || string.IsNullOrWhiteSpace(taiKhoanIdServerB))
        {
            return false;
        }

        return LayDanhSachQuanHeChanDangHoatDong(ctx).Any(x =>
            (x.BlockerTaiKhoanIdServer == taiKhoanIdServerA && x.BlockedTaiKhoanIdServer == taiKhoanIdServerB) ||
            (x.BlockerTaiKhoanIdServer == taiKhoanIdServerB && x.BlockedTaiKhoanIdServer == taiKhoanIdServerA));
    }

    private static CapHaiTaiKhoan ChonCapTaiKhoanChuaTheoDoi(NguCanhKiemThu ctx)
    {
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx);
        foreach (var taiKhoanTheoDoi in taiKhoan)
        {
            foreach (var taiKhoanDuocTheoDoi in taiKhoan)
            {
                if (taiKhoanTheoDoi.TaiKhoanIdServer == taiKhoanDuocTheoDoi.TaiKhoanIdServer ||
                    CoQuanHeTheoDoi(ctx, taiKhoanTheoDoi.TaiKhoanIdServer, taiKhoanDuocTheoDoi.TaiKhoanIdServer) ||
                    CoQuanHeChan(ctx, taiKhoanTheoDoi.TaiKhoanIdServer, taiKhoanDuocTheoDoi.TaiKhoanIdServer))
                {
                    continue;
                }

                return new CapHaiTaiKhoan(taiKhoanTheoDoi, taiKhoanDuocTheoDoi);
            }
        }

        throw new BoQuaKiemThuException("Không tìm được cặp tài khoản chưa follow nhau.");
    }

    private static CapHaiTaiKhoan ChonCapTaiKhoanChuaChan(NguCanhKiemThu ctx)
    {
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx);
        var taiKhoanChanChinhId = LayDanhSachQuanHeChanDangHoatDong(ctx)
            .GroupBy(x => x.BlockerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();
        var taiKhoanTheoDoiChinhId = LayDanhSachQuanHeTheoDoiDangHoatDong(ctx)
            .GroupBy(x => x.FollowerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();

        var taiKhoanChanChinh = !string.IsNullOrWhiteSpace(taiKhoanChanChinhId)
            ? taiKhoan.FirstOrDefault(x => x.TaiKhoanIdServer == taiKhoanChanChinhId)
            : null;
        IEnumerable<TaiKhoanSignupThanhCongSeed> danhSachTaiKhoanChan = taiKhoanChanChinh is not null
            ? new[] { taiKhoanChanChinh }
            : taiKhoan.Where(x => string.IsNullOrWhiteSpace(taiKhoanTheoDoiChinhId) || x.TaiKhoanIdServer != taiKhoanTheoDoiChinhId);

        foreach (var taiKhoanChan in danhSachTaiKhoanChan)
        {
            foreach (var taiKhoanBiChan in taiKhoan)
            {
                if (taiKhoanChan.TaiKhoanIdServer == taiKhoanBiChan.TaiKhoanIdServer ||
                    CoQuanHeChan(ctx, taiKhoanChan.TaiKhoanIdServer, taiKhoanBiChan.TaiKhoanIdServer) ||
                    CoQuanHeTheoDoi(ctx, taiKhoanChan.TaiKhoanIdServer, taiKhoanBiChan.TaiKhoanIdServer) ||
                    CoQuanHeTheoDoi(ctx, taiKhoanBiChan.TaiKhoanIdServer, taiKhoanChan.TaiKhoanIdServer))
                {
                    continue;
                }

                return new CapHaiTaiKhoan(taiKhoanChan, taiKhoanBiChan);
            }
        }

        throw new BoQuaKiemThuException("Không tìm được cặp tài khoản chưa block nhau.");
    }

    private static CapHaiTaiKhoan ChonCapTaiKhoanKhongCoQuanHeChan(NguCanhKiemThu ctx, string thongBaoKhiThieu)
    {
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx);
        foreach (var taiKhoanThuNhat in taiKhoan)
        {
            foreach (var taiKhoanThuHai in taiKhoan)
            {
                if (taiKhoanThuNhat.TaiKhoanIdServer == taiKhoanThuHai.TaiKhoanIdServer ||
                    CoQuanHeChan(ctx, taiKhoanThuNhat.TaiKhoanIdServer, taiKhoanThuHai.TaiKhoanIdServer))
                {
                    continue;
                }

                return new CapHaiTaiKhoan(taiKhoanThuNhat, taiKhoanThuHai);
            }
        }

        throw new BoQuaKiemThuException(thongBaoKhiThieu);
    }

    private static void XoaQuanHeTheoDoiDangHoatDong(NguCanhKiemThu ctx, TaiKhoanTheoDoiSeed quanHe)
    {
        ctx.KhoSeed.DuLieu.TaiKhoanTheoDoiSeed.RemoveAll(x =>
            x.TrangThai == "dang_theo_doi" &&
            x.FollowerTaiKhoanIdServer == quanHe.FollowerTaiKhoanIdServer &&
            x.FolloweeTaiKhoanIdServer == quanHe.FolloweeTaiKhoanIdServer);
    }

    private static void XoaQuanHeChanDangHoatDong(NguCanhKiemThu ctx, TaiKhoanChanSeed quanHe)
    {
        ctx.KhoSeed.DuLieu.TaiKhoanChanSeed.RemoveAll(x =>
            x.TrangThai == "dang_chan" &&
            x.BlockerTaiKhoanIdServer == quanHe.BlockerTaiKhoanIdServer &&
            x.BlockedTaiKhoanIdServer == quanHe.BlockedTaiKhoanIdServer);
    }
}


