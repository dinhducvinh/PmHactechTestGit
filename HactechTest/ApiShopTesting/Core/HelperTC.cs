using System.Text.Json.Nodes;
using System.Collections.Concurrent;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.Core;

internal static class HelperTC
{
    internal static Task<YeuCauApi> Req(HttpMethod method, string path, object? body = null, string? token = null)
    {
        return Task.FromResult(new YeuCauApi(method, path, body, token));
    }

    internal static Dictionary<string, object?> Obj(params (string key, object? value)[] pairs)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var (key, value) in pairs)
        {
            dict[key] = value;
        }

        return dict;
    }

    internal static string RutGon(string? raw, int max)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "";
        }

        var motDong = raw.Replace(Environment.NewLine, " ");
        return motDong.Length <= max ? motDong : motDong[..max] + "...";
    }

    internal static int IdBatBuoc(int? giaTri, string tenDuLieu)
    {
        if (giaTri is > 0)
        {
            return giaTri.Value;
        }

        throw new BoQuaKiemThuException($"Thiếu {tenDuLieu} trong seed.");
    }

    internal static bool LaMaThanhCong(PhanHoiApi response)
    {
        return string.Equals(response.MaSoSanh, "1000", StringComparison.OrdinalIgnoreCase);
    }

    internal static TaiKhoanChuaDangKySeed YeuCauTaiKhoanChuaDangKy(NguCanhKiemThu ctx)
    {
        return LayTaiKhoanChuaDangKy(ctx)
            ?? throw new BoQuaKiemThuException("Thiếu tài khoản chưa đăng ký trong taikhoan_seed.");
    }

    internal static async Task<TaiKhoanSignupThanhCongSeed> YeuCauTaiKhoanDaDangKyAsync(NguCanhKiemThu ctx)
    {
        var taiKhoan = LayTaiKhoanDaDangKy(ctx);
        if (taiKhoan is not null)
        {
            return taiKhoan;
        }

        return await DangKyTaiKhoanMoiAsync(ctx);
    }

    internal static async Task<string> YeuCauTokenHopLeAsync(NguCanhKiemThu ctx)
    {
        var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
        return await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan);
    }

    internal static async Task<string> LayTokenCuaTaiKhoanAsync(NguCanhKiemThu ctx, TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        var response = await ctx.Api.GuiAsync(new YeuCauApi(
            HttpMethod.Post,
            "/auth/login",
            TaoBodyDangNhap(taiKhoan)));

        var token = response.MaSoSanh == "1000"
            ? response.Data?["token"]?.ToString()
            : null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        throw new BoQuaKiemThuException(
            $"Không lấy được token hợp lệ cho tài khoản seed {taiKhoan.SoThuTu}. Đây là lỗi chuẩn bị test nếu API login chưa sẵn sàng.");
    }

    internal static async Task<Dictionary<int, string>> LayTokenChoDanhSachTaiKhoanAsync(
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

    internal static async Task<TaiKhoanSignupThanhCongSeed> DangKyTaiKhoanMoiAsync(NguCanhKiemThu ctx)
    {
        var taiKhoan = LayTaiKhoanChuaDangKy(ctx);
        if (taiKhoan is null)
        {
            throw new BoQuaKiemThuException("Thiếu tài khoản chưa đăng ký trong taikhoan_seed.");
        }

        var matKhauDungDeDangKy = taiKhoan.MatKhauHienTai;
        var response = await ctx.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/auth/signup", TaoBodyDangKy(taiKhoan)));
        if (response.MaSoSanh != "1000")
        {
            throw new BoQuaKiemThuException("Không tạo được tài khoản đã đăng ký qua API /auth/signup. Kiểm tra base URL hoặc logic signup.");
        }

        var taiKhoanDaDangKy = await ctx.CapNhatDB.LuuTaiKhoanDangKyThanhCongAsync(
            response,
            taiKhoan,
            matKhauDungDeDangKy);
        return taiKhoanDaDangKy;
    }

    internal static async Task<string> LayOtpResetMatKhauAsync(NguCanhKiemThu ctx, TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        var response = await ctx.Api.GuiAsync(new YeuCauApi(
            HttpMethod.Post,
            "/auth/create_code_reset_password",
            Obj(("phone_number", taiKhoan.SoDienThoai))));

        var otp = response.MaSoSanh == "1000"
            ? response.Data?["otp"]?.ToString()
            : null;

        if (!string.IsNullOrWhiteSpace(otp))
        {
            return otp;
        }

        throw new BoQuaKiemThuException(
            "Không lấy được OTP từ /auth/create_code_reset_password. Nếu server không trả OTP ra response thì cần chuẩn bị OTP thủ công trong môi trường test.");
    }

    internal static async Task XacThucOtpResetMatKhauAsync(NguCanhKiemThu ctx, TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        var otp = await LayOtpResetMatKhauAsync(ctx, taiKhoan);
        var response = await ctx.Api.GuiAsync(new YeuCauApi(
            HttpMethod.Post,
            "/auth/check_code_reset_password",
            Obj(("phone_number", taiKhoan.SoDienThoai), ("reset_code", otp))));

        if (response.MaSoSanh != "1000")
        {
            throw new BoQuaKiemThuException("Không xác thực được OTP trước khi chạy reset password.");
        }
    }

    internal static Dictionary<string, object?> TaoBodyDangKy(TaiKhoanChuaDangKySeed taiKhoan)
    {
        return Obj(
            ("phone_number", taiKhoan.SoDienThoai),
            ("password", taiKhoan.MatKhauHienTai),
            ("uuid", taiKhoan.UuidThietBi));
    }

    internal static Dictionary<string, object?> TaoBodyDangNhap(TaiKhoanSignupThanhCongSeed taiKhoan)
    {
        return Obj(
            ("phone_number", taiKhoan.SoDienThoai),
            ("password", taiKhoan.MatKhauHienTai),
            ("devtoken", taiKhoan.UuidThietBi));
    }

    internal static TaiKhoanSignupThanhCongSeed? LayTaiKhoanDaDangKy(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x => x.TaiKhoanIdServer > 0)
            .OrderBy(x => x.SoThuTu)
            .ThenBy(x => x.TaiKhoanIdServer)
            .FirstOrDefault();
    }

    internal static TaiKhoanChuaDangKySeed? LayTaiKhoanChuaDangKy(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TaiKhoanChuaDangKySeed
            .Where(x => x.TrangThai == "san_sang")
            .OrderBy(x => x.SoThuTu)
            .FirstOrDefault();
    }

    internal static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataCoTruong(params string[] truong)
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu data object."));
            }

            foreach (var t in truong)
            {
                if (!data.ContainsKey(t))
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data thiếu trường `{t}`."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    internal static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataLaMang(string? truongItemBatBuoc = null)
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

            if (!string.IsNullOrWhiteSpace(truongItemBatBuoc) &&
                mang.Count > 0 &&
                (mang[0] is not JsonObject item || !item.ContainsKey(truongItemBatBuoc)))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"item đầu tiên thiếu trường `{truongItemBatBuoc}`."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    internal static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> DataBoolBang(string tenTruong, bool mongDoi)
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var giaTri = response.Data?[tenTruong]?.GetValue<bool>();
            if (giaTri != mongDoi)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.{tenTruong} phải bằng {mongDoi}, thực tế là {giaTri?.ToString() ?? "null"}."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    internal sealed record CapHaiTaiKhoan(
        TaiKhoanSignupThanhCongSeed TaiKhoanThuNhat,
        TaiKhoanSignupThanhCongSeed TaiKhoanThuHai);

    internal sealed record CapTaiKhoanDangTheoDoi(
        TaiKhoanTheoDoiSeed QuanHeTheoDoi,
        TaiKhoanSignupThanhCongSeed TaiKhoanTheoDoi,
        TaiKhoanSignupThanhCongSeed TaiKhoanDuocTheoDoi);

    internal sealed record CapTaiKhoanDangChan(
        TaiKhoanChanSeed QuanHeChan,
        TaiKhoanSignupThanhCongSeed TaiKhoanChan,
        TaiKhoanSignupThanhCongSeed TaiKhoanBiChan);

    internal sealed record CapSanPhamVaSeller(
        SanPhamSeed SanPham,
        TaiKhoanSignupThanhCongSeed Seller);

    internal sealed record CapTaiKhoanVaSanPham(
        TaiKhoanSignupThanhCongSeed TaiKhoan,
        SanPhamSeed SanPham);

    internal sealed record CapLikeSanPham(
        TaiKhoanThichSanPhamSeed Like,
        TaiKhoanSignupThanhCongSeed TaiKhoan,
        SanPhamSeed SanPham);

    internal sealed record DuLieuBodySanPham(
        Dictionary<string, object?> Body,
        DanhMucSeed DanhMuc,
        ThuongHieuSeed? ThuongHieu,
        DiaChiTaiKhoanSeed DiaChi,
        string Ten,
        decimal Gia);

    internal sealed record DuLieuThemDiaChi(
        PhuongXaSeed PhuongXa,
        TinhThanhSeed TinhThanh,
        Dictionary<string, object?> Body,
        string DiaChi,
        string DiaChiChiTiet,
        decimal ViDo,
        decimal KinhDo,
        string TenNguoiNhan,
        string SoDienThoaiNguoiNhan);

    internal static IReadOnlyList<TaiKhoanSignupThanhCongSeed> LayDanhSachTaiKhoanSanSang(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x => x.TaiKhoanIdServer > 0)
            .OrderBy(x => x.SoThuTu)
            .ThenBy(x => x.TaiKhoanIdServer)
            .ToList();
    }

    internal static IReadOnlyList<TaiKhoanSignupThanhCongSeed> LayDanhSachTaiKhoanDaDangKyBatBuoc(
        NguCanhKiemThu ctx,
        int soLuong,
        string thongBaoKhiThieu)
    {
        var taiKhoan = ctx.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x =>
                x.TaiKhoanIdServer is > 0 &&
                !string.IsNullOrWhiteSpace(x.SoDienThoai) &&
                !string.IsNullOrWhiteSpace(x.MatKhauHienTai))
            .OrderBy(x => x.SoThuTu)
            .Take(soLuong)
            .ToList();

        return taiKhoan.Count >= soLuong
            ? taiKhoan
            : throw new LoiChuanBiKiemThuException(thongBaoKhiThieu);
    }

    internal static TaiKhoanSignupThanhCongSeed? LayTaiKhoanTheoServerId(NguCanhKiemThu ctx, int? taiKhoanIdServer)
    {
        if (taiKhoanIdServer is null or <= 0)
        {
            return null;
        }

        return ctx.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed.FirstOrDefault(x =>
            x.TaiKhoanIdServer > 0 &&
            x.TaiKhoanIdServer == taiKhoanIdServer.Value);
    }

    internal static TaiKhoanTimKiemSeed LayTimKiemDangLuu(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed
            .Where(x => x.TrangThai == "dang_luu")
            .OrderBy(x => x.TaiKhoanTimKiemSeedId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu tk_timkiem_seed trạng thái dang_luu.");
    }

    internal static TaiKhoanSignupThanhCongSeed? LayTaiKhoanUuTienCoThongBao(NguCanhKiemThu ctx)
    {
        var thongBao = ctx.CapNhatDB.DuLieu.ThongBaoSeed
            .Where(x => x.NotificationIdServer is > 0)
            .OrderBy(x => x.DaDoc == true ? 1 : 0)
            .ThenBy(x => x.ThongBaoSeedId)
            .FirstOrDefault();

        return thongBao is null
            ? null
            : LayTaiKhoanTheoServerId(ctx, thongBao.TaiKhoanIdServer);
    }

    internal static ThongBaoSeed LayThongBaoChuaDoc(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.ThongBaoSeed
            .Where(x => x.NotificationIdServer is > 0)
            .Where(x => x.DaDoc != true || string.Equals(x.TrangThai, "dang_luu", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ThongBaoSeedId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu thông báo chưa đọc trong thongbao_seed. Hãy bấm Kiểm tra seed hoặc tạo nghiệp vụ phát sinh notification trước.");
    }

    internal static long LayNotificationIdDangCoHoacMacDinh(NguCanhKiemThu ctx)
    {
        var thongBao = ctx.CapNhatDB.DuLieu.ThongBaoSeed
            .Where(x => x.NotificationIdServer is > 0)
            .OrderBy(x => x.ThongBaoSeedId)
            .FirstOrDefault();

        return thongBao is null
            ? 999999999
            : IdBatBuoc(thongBao.NotificationIdServer, "thongbao_seed.notification_id_server");
    }

    internal static DanhMucSeed LayDanhMucSanSang(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && x.DanhMucIdServer is > 0)
            ?? throw new BoQuaKiemThuException("Thiếu danhmuc_seed trạng thái san_sang.");
    }

    internal static DanhMucSeed LayDanhMucCoCon(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                x.DanhMucIdServer is > 0 &&
                (x.CoDanhMucCon == true ||
                 ctx.CapNhatDB.DuLieu.DanhMucSeed.Any(con =>
                     con.TrangThai == "san_sang" &&
                     con.DanhMucChaIdServer == x.DanhMucIdServer)))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed có danh mục con.");
    }

    internal static DanhMucSeed LayDanhMucKhongCoCon(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                x.DanhMucIdServer is > 0 &&
                x.CoDanhMucCon != true &&
                !ctx.CapNhatDB.DuLieu.DanhMucSeed.Any(con =>
                    con.TrangThai == "san_sang" &&
                    con.DanhMucChaIdServer == x.DanhMucIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed tồn tại nhưng không có danh mục con.");
    }

    internal static DanhMucSeed LayDanhMucCoThuongHieu(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                x.DanhMucIdServer is > 0 &&
                (x.CoThuongHieu == true ||
                 ctx.CapNhatDB.DuLieu.ThuongHieuSeed.Any(th =>
                     th.TrangThai == "san_sang" &&
                     th.DanhMucIdServer == x.DanhMucIdServer)))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed có thương hiệu.");
    }

    internal static DanhMucSeed LayDanhMucKhongCoThuongHieu(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.DanhMucSeed
            .FirstOrDefault(x =>
                x.TrangThai == "san_sang" &&
                x.DanhMucIdServer is > 0 &&
                x.CoThuongHieu != true &&
                !ctx.CapNhatDB.DuLieu.ThuongHieuSeed.Any(th =>
                    th.TrangThai == "san_sang" &&
                    th.DanhMucIdServer == x.DanhMucIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu danh mục seed tồn tại nhưng không có thương hiệu.");
    }

    internal static ThuongHieuSeed? LayThuongHieuSanSang(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.ThuongHieuSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && x.ThuongHieuIdServer is > 0);
    }

    internal static bool QuanHeTheoDoiDangHoatDong(TaiKhoanTheoDoiSeed quanHe)
    {
        return quanHe.TrangThai == "dang_theo_doi" &&
               quanHe.FollowerTaiKhoanIdServer is > 0 &&
               quanHe.FolloweeTaiKhoanIdServer is > 0;
    }

    internal static bool QuanHeChanDangHoatDong(TaiKhoanChanSeed quanHe)
    {
        return quanHe.TrangThai == "dang_chan" &&
               quanHe.BlockerTaiKhoanIdServer is > 0 &&
               quanHe.BlockedTaiKhoanIdServer is > 0;
    }

    internal static IReadOnlyList<TaiKhoanTheoDoiSeed> LayDanhSachQuanHeTheoDoiDangHoatDong(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TaiKhoanTheoDoiSeed
            .Where(QuanHeTheoDoiDangHoatDong)
            .ToList();
    }

    internal static IReadOnlyList<TaiKhoanChanSeed> LayDanhSachQuanHeChanDangHoatDong(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TaiKhoanChanSeed
            .Where(QuanHeChanDangHoatDong)
            .GroupBy(x => x.BlockerTaiKhoanIdServer)
            .OrderByDescending(x => x.Count())
            .SelectMany(x => x.OrderBy(y => y.ChanSeedId))
            .ToList();
    }

    internal static CapTaiKhoanDangTheoDoi LayCapTaiKhoanDangTheoDoi(NguCanhKiemThu ctx, string thongBaoKhiThieu)
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

    internal static CapTaiKhoanDangChan LayCapTaiKhoanDangChan(NguCanhKiemThu ctx, string thongBaoKhiThieu)
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

    internal static bool CoQuanHeTheoDoi(NguCanhKiemThu ctx, int? followerTaiKhoanIdServer, int? followeeTaiKhoanIdServer)
    {
        if (followerTaiKhoanIdServer is null or <= 0 || followeeTaiKhoanIdServer is null or <= 0)
        {
            return false;
        }

        return LayDanhSachQuanHeTheoDoiDangHoatDong(ctx).Any(x =>
            x.FollowerTaiKhoanIdServer == followerTaiKhoanIdServer.Value &&
            x.FolloweeTaiKhoanIdServer == followeeTaiKhoanIdServer.Value);
    }

    internal static bool CoQuanHeChan(NguCanhKiemThu ctx, int? taiKhoanIdServerA, int? taiKhoanIdServerB)
    {
        if (taiKhoanIdServerA is null or <= 0 || taiKhoanIdServerB is null or <= 0)
        {
            return false;
        }

        return LayDanhSachQuanHeChanDangHoatDong(ctx).Any(x =>
            (x.BlockerTaiKhoanIdServer == taiKhoanIdServerA.Value && x.BlockedTaiKhoanIdServer == taiKhoanIdServerB.Value) ||
            (x.BlockerTaiKhoanIdServer == taiKhoanIdServerB.Value && x.BlockedTaiKhoanIdServer == taiKhoanIdServerA.Value));
    }

    internal static CapHaiTaiKhoan ChonCapTaiKhoan(
        IEnumerable<TaiKhoanSignupThanhCongSeed> danhSachTaiKhoanThuNhat,
        IEnumerable<TaiKhoanSignupThanhCongSeed> danhSachTaiKhoanThuHai,
        Func<TaiKhoanSignupThanhCongSeed, TaiKhoanSignupThanhCongSeed, bool> phuHop,
        string thongBaoKhiThieu)
    {
        foreach (var taiKhoanThuNhat in danhSachTaiKhoanThuNhat)
        {
            foreach (var taiKhoanThuHai in danhSachTaiKhoanThuHai)
            {
                if (taiKhoanThuNhat.TaiKhoanIdServer != taiKhoanThuHai.TaiKhoanIdServer &&
                    phuHop(taiKhoanThuNhat, taiKhoanThuHai))
                {
                    return new CapHaiTaiKhoan(taiKhoanThuNhat, taiKhoanThuHai);
                }
            }
        }

        throw new BoQuaKiemThuException(thongBaoKhiThieu);
    }

    internal static CapHaiTaiKhoan ChonCapTaiKhoanChuaTheoDoi(NguCanhKiemThu ctx)
    {
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx);
        return ChonCapTaiKhoan(
            taiKhoan,
            taiKhoan,
            (taiKhoanTheoDoi, taiKhoanDuocTheoDoi) =>
                !CoQuanHeTheoDoi(ctx, taiKhoanTheoDoi.TaiKhoanIdServer, taiKhoanDuocTheoDoi.TaiKhoanIdServer) &&
                !CoQuanHeChan(ctx, taiKhoanTheoDoi.TaiKhoanIdServer, taiKhoanDuocTheoDoi.TaiKhoanIdServer),
            "Không tìm được cặp tài khoản chưa follow nhau.");
    }

    internal static CapHaiTaiKhoan ChonCapTaiKhoanChuaChan(NguCanhKiemThu ctx)
    {
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx);
        var danhSachTaiKhoanChan = LayDanhSachTaiKhoanUuTienDeChan(ctx, taiKhoan);

        return ChonCapTaiKhoan(
            danhSachTaiKhoanChan,
            taiKhoan,
            (taiKhoanChan, taiKhoanBiChan) =>
                !CoQuanHeChan(ctx, taiKhoanChan.TaiKhoanIdServer, taiKhoanBiChan.TaiKhoanIdServer) &&
                !CoQuanHeTheoDoi(ctx, taiKhoanChan.TaiKhoanIdServer, taiKhoanBiChan.TaiKhoanIdServer) &&
                !CoQuanHeTheoDoi(ctx, taiKhoanBiChan.TaiKhoanIdServer, taiKhoanChan.TaiKhoanIdServer),
            "Không tìm được cặp tài khoản chưa block nhau.");
    }

    internal static IEnumerable<TaiKhoanSignupThanhCongSeed> LayDanhSachTaiKhoanUuTienDeChan(
        NguCanhKiemThu ctx,
        IReadOnlyList<TaiKhoanSignupThanhCongSeed> taiKhoanSanSang)
    {
        var taiKhoanChanChinhId = LayDanhSachQuanHeChanDangHoatDong(ctx)
            .GroupBy(x => x.BlockerTaiKhoanIdServer!.Value)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();

        var taiKhoanChanChinh = taiKhoanSanSang.FirstOrDefault(x => x.TaiKhoanIdServer == taiKhoanChanChinhId);
        if (taiKhoanChanChinh is not null)
        {
            return new[] { taiKhoanChanChinh };
        }

        var taiKhoanTheoDoiChinhId = LayDanhSachQuanHeTheoDoiDangHoatDong(ctx)
            .GroupBy(x => x.FollowerTaiKhoanIdServer!.Value)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .FirstOrDefault();

        return taiKhoanSanSang.Where(x => taiKhoanTheoDoiChinhId <= 0 || x.TaiKhoanIdServer != taiKhoanTheoDoiChinhId);
    }

    internal static CapHaiTaiKhoan ChonCapTaiKhoanKhongCoQuanHeChan(NguCanhKiemThu ctx, string thongBaoKhiThieu)
    {
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx);
        return ChonCapTaiKhoan(
            taiKhoan,
            taiKhoan,
            (taiKhoanThuNhat, taiKhoanThuHai) =>
                !CoQuanHeChan(ctx, taiKhoanThuNhat.TaiKhoanIdServer, taiKhoanThuHai.TaiKhoanIdServer),
            thongBaoKhiThieu);
    }

    internal static TinNhanSeed LayTinNhanDaGuiBatBuoc(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TinNhanSeed
            .Where(x => x.TrangThai == "da_gui")
            .Where(x => x.ConversationIdServer is > 0)
            .Where(x => x.SenderTaiKhoanIdServer is > 0)
            .Where(x => x.ReceiverTaiKhoanIdServer is > 0)
            .OrderByDescending(x => x.GuiLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.TinNhanSeedId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu dữ liệu tinnhan_seed trạng thái da_gui. Hãy bấm Kiểm tra seed để tạo tin nhắn mồi.");
    }

    internal static TinNhanSeed LayTinNhanCoNoiDungDaGuiBatBuoc(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TinNhanSeed
            .Where(x => x.TrangThai == "da_gui")
            .Where(x => x.ConversationIdServer is > 0)
            .Where(x => x.MessageIdServer is > 0)
            .Where(x => x.SenderTaiKhoanIdServer is > 0)
            .Where(x => x.ReceiverTaiKhoanIdServer is > 0)
            .Where(x => !string.IsNullOrWhiteSpace(x.NoiDung))
            .OrderByDescending(x => x.GuiLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.TinNhanSeedId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu tinnhan_seed dạng text có nội dung để đối chiếu get_conversation.");
    }

    internal static TinNhanSeed LayTinNhanCoQuanHeChanBatBuoc(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.TinNhanSeed
            .Where(x => x.TrangThai == "da_gui")
            .Where(x => x.ConversationIdServer is > 0)
            .Where(x => x.SenderTaiKhoanIdServer is > 0)
            .Where(x => x.ReceiverTaiKhoanIdServer is > 0)
            .Where(x => CoQuanHeChan(ctx, x.SenderTaiKhoanIdServer, x.ReceiverTaiKhoanIdServer))
            .OrderByDescending(x => x.GuiLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.TinNhanSeedId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu tinnhan_seed giữa hai tài khoản có quan hệ block. Có thể chạy CONVERSATION-SEND-07 trước để tạo dữ liệu này.");
    }

    internal static CapHaiTaiKhoan ChonCapTaiKhoanChanChuaCoConversation(NguCanhKiemThu ctx)
    {
        foreach (var quanHe in LayDanhSachQuanHeChanDangHoatDong(ctx))
        {
            var blocker = LayTaiKhoanTheoServerId(ctx, quanHe.BlockerTaiKhoanIdServer);
            var blocked = LayTaiKhoanTheoServerId(ctx, quanHe.BlockedTaiKhoanIdServer);
            if (blocker is not null &&
                blocked is not null &&
                !CoConversationTrongSeed(ctx, blocker.TaiKhoanIdServer, blocked.TaiKhoanIdServer))
            {
                return new CapHaiTaiKhoan(blocker, blocked);
            }
        }

        throw new BoQuaKiemThuException("Không tìm được cặp tài khoản có quan hệ block nhưng chưa có conversation trong tinnhan_seed.");
    }

    internal static CapHaiTaiKhoan ChonCapTaiKhoanChuaCoConversation(NguCanhKiemThu ctx)
    {
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx);
        return ChonCapTaiKhoan(
            taiKhoan,
            taiKhoan,
            (taiKhoanThuNhat, taiKhoanThuHai) =>
                !CoQuanHeChan(ctx, taiKhoanThuNhat.TaiKhoanIdServer, taiKhoanThuHai.TaiKhoanIdServer) &&
                !CoConversationTrongSeed(ctx, taiKhoanThuNhat.TaiKhoanIdServer, taiKhoanThuHai.TaiKhoanIdServer),
            "Không tìm được cặp tài khoản chưa có conversation trong tinnhan_seed.");
    }

    internal static bool CoConversationTrongSeed(NguCanhKiemThu ctx, int? taiKhoanIdServerA, int? taiKhoanIdServerB)
    {
        if (taiKhoanIdServerA is null or <= 0 || taiKhoanIdServerB is null or <= 0)
        {
            return false;
        }

        return ctx.CapNhatDB.DuLieu.TinNhanSeed.Any(x =>
            x.TrangThai == "da_gui" &&
            ((x.SenderTaiKhoanIdServer == taiKhoanIdServerA.Value && x.ReceiverTaiKhoanIdServer == taiKhoanIdServerB.Value) ||
             (x.SenderTaiKhoanIdServer == taiKhoanIdServerB.Value && x.ReceiverTaiKhoanIdServer == taiKhoanIdServerA.Value)));
    }

    internal static SanPhamSeed LaySanPhamSanSang(NguCanhKiemThu ctx)
    {
        return LayDanhSachSanPhamSanSang(ctx).FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu sanpham_seed trạng thái san_sang.");
    }

    internal static IReadOnlyList<SanPhamSeed> LayDanhSachSanPhamSanSang(NguCanhKiemThu ctx)
    {
        return ctx.CapNhatDB.DuLieu.SanPhamSeed
            .Where(x => x.TrangThai == "san_sang" && x.SanPhamIdServer is > 0)
            .OrderBy(x => x.ThuTuNoiBo)
            .ToList();
    }

    internal static SanPhamSeed? LaySanPhamTheoServerId(NguCanhKiemThu ctx, int? sanPhamIdServer)
    {
        if (sanPhamIdServer is null or <= 0)
        {
            return null;
        }

        return LayDanhSachSanPhamSanSang(ctx).FirstOrDefault(x => x.SanPhamIdServer == sanPhamIdServer.Value);
    }

    internal static SanPhamSeed LaySanPhamCoTheXoa(NguCanhKiemThu ctx)
    {
        var sanPhams = LayDanhSachSanPhamSanSang(ctx)
            .Where(x => x.TaiKhoanIdServer is > 0)
            .ToList();

        var sanPhamDaTaoBoiTest = sanPhams
            .Where(x => x.TaoBoiTest)
            .OrderByDescending(x => x.TaoLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.ThuTuNoiBo)
            .FirstOrDefault();

        if (sanPhamDaTaoBoiTest is not null)
        {
            return sanPhamDaTaoBoiTest;
        }

        return sanPhams
            .OrderByDescending(x => x.TaoLuc ?? DateTimeOffset.MinValue)
            .ThenByDescending(x => x.ThuTuNoiBo)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu sanpham_seed có thể xóa.");
    }

    internal static TaiKhoanSignupThanhCongSeed LaySellerCuaSanPham(NguCanhKiemThu ctx, SanPhamSeed sanPham)
    {
        return LayTaiKhoanTheoServerId(ctx, sanPham.TaiKhoanIdServer)
            ?? throw new BoQuaKiemThuException($"Thiếu tài khoản seller của sản phẩm seed {sanPham.ThuTuNoiBo}.");
    }

    internal static CapSanPhamVaSeller LaySanPhamKemSeller(NguCanhKiemThu ctx)
    {
        var sanPham = LaySanPhamSanSang(ctx);
        return new CapSanPhamVaSeller(sanPham, LaySellerCuaSanPham(ctx, sanPham));
    }

    internal static CapSanPhamVaSeller LaySanPhamCoTheXoaKemSeller(NguCanhKiemThu ctx)
    {
        var sanPham = LaySanPhamCoTheXoa(ctx);
        return new CapSanPhamVaSeller(sanPham, LaySellerCuaSanPham(ctx, sanPham));
    }

    internal static TaiKhoanSignupThanhCongSeed LaySellerCoDiaChi(NguCanhKiemThu ctx)
    {
        return LayDanhSachTaiKhoanSanSang(ctx)
            .FirstOrDefault(taiKhoan => ctx.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed.Any(diaChi =>
                diaChi.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer &&
                diaChi.TrangThai == "san_sang" &&
                diaChi.DiaChiIdServer is > 0))
            ?? throw new BoQuaKiemThuException("Thiếu seller đã đăng ký có diachi_tk_seed san_sang.");
    }

    internal static DuLieuBodySanPham TaoDuLieuBodySanPhamMoi(NguCanhKiemThu ctx, TaiKhoanSignupThanhCongSeed seller)
    {
        var danhMuc = LayDanhMucSanSang(ctx);
        var thuongHieu = LayThuongHieuSanSang(ctx);
        var diaChi = ctx.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(x =>
            x.TaiKhoanIdServer == seller.TaiKhoanIdServer &&
            x.TrangThai == "san_sang" &&
            x.DiaChiIdServer is > 0)
            ?? throw new BoQuaKiemThuException("Thiếu địa chỉ seed của seller để thêm sản phẩm.");

        var soThuTu = ctx.CapNhatDB.DuLieu.SanPhamSeed.Count + 1;
        var ten = $"San pham testcase {soThuTu} {DateTimeOffset.Now:HHmmss}";
        var gia = 150000m + soThuTu * 1000m;
        var body = Obj(
            ("title", ten),
            ("price", gia),
            ("description", "San pham tao tu testcase Product."),
            ("category_id", IdBatBuoc(danhMuc.DanhMucIdServer, "danhmuc_seed.dm_id_server")),
            ("ship_from_id", IdBatBuoc(diaChi.DiaChiIdServer, "diachi_tk_seed.diachi_id_server")),
            ("variants", new[] { Obj(("size", "M"), ("stock", 20), ("color", "Do"), ("weight", 500)) }));

        if (thuongHieu?.ThuongHieuIdServer is > 0)
        {
            body["brand_id"] = IdBatBuoc(thuongHieu.ThuongHieuIdServer, "thuonghieu_seed.thuonghieu_id_server");
        }

        return new DuLieuBodySanPham(body, danhMuc, thuongHieu, diaChi, ten, gia);
    }

    internal static DuLieuThemDiaChi TaoDuLieuThemDiaChi(
        NguCanhKiemThu ctx,
        TaiKhoanSignupThanhCongSeed? taiKhoan,
        string maTestCase)
    {
        var ward = ctx.CapNhatDB.DuLieu.PhuongXaSeed
            .OrderBy(x => x.PhuongXaId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu Wards_seed để tạo body /order/add_order_address.");
        var province = ctx.CapNhatDB.DuLieu.TinhThanhSeed
            .FirstOrDefault(x => x.TinhThanhId == ward.TinhThanhId)
            ?? throw new BoQuaKiemThuException($"Thiếu Provinces_seed id {ward.TinhThanhId} tương ứng với ward {ward.PhuongXaId}.");

        var seedId = taiKhoan?.SoThuTu ?? 0;
        var addressDetail = $"So {Math.Max(seedId, 1)}, duong Test";
        var address = $"{addressDetail}, {ward.TenPhuongXa}, {province.TenTinhThanh}";
        var phone = taiKhoan?.SoDienThoai ?? "0909000001";
        var receiverName = $"Receiver {maTestCase}";
        var lat = 21.0m + Math.Max(seedId, 1) / 10000m;
        var lng = 105.8m + Math.Max(seedId, 1) / 10000m;
        var body = Obj(
            ("address", address),
            ("is_default", true),
            ("address_id", new[] { ward.PhuongXaId, province.TinhThanhId }),
            ("lat", lat),
            ("lng", lng),
            ("receiver_name", receiverName),
            ("phone", phone),
            ("full_address", address),
            ("address_detail", addressDetail));

        return new DuLieuThemDiaChi(ward, province, body, address, addressDetail, lat, lng, receiverName, phone);
    }

    internal static int? DocIdSanPham(JsonNode? data)
    {
        return data?["id"]?.GetValue<int>();
    }

    internal static CapTaiKhoanVaSanPham ChonCapTaiKhoanXemSanPhamKhongBiChan(NguCanhKiemThu ctx)
    {
        var sanPham = LaySanPhamSanSang(ctx);
        var taiKhoan = LayDanhSachTaiKhoanSanSang(ctx)
            .FirstOrDefault(taiKhoan =>
                taiKhoan.TaiKhoanIdServer != sanPham.TaiKhoanIdServer &&
                !CoQuanHeChan(ctx, taiKhoan.TaiKhoanIdServer, sanPham.TaiKhoanIdServer))
            ?? throw new BoQuaKiemThuException("Thiếu tài khoản khác seller và không có quan hệ block với seller.");

        return new CapTaiKhoanVaSanPham(taiKhoan, sanPham);
    }

    internal static CapTaiKhoanVaSanPham ChonCapTaiKhoanBiChanVoiSanPham(NguCanhKiemThu ctx)
    {
        var sanPhams = LayDanhSachSanPhamSanSang(ctx)
            .Where(x => x.TaiKhoanIdServer is > 0)
            .ToList();

        foreach (var quanHeChan in LayDanhSachQuanHeChanDangHoatDong(ctx))
        {
            var sanPhamCuaTaiKhoanBiChan = sanPhams.FirstOrDefault(x => x.TaiKhoanIdServer == quanHeChan.BlockedTaiKhoanIdServer);
            var taiKhoanChan = LayTaiKhoanTheoServerId(ctx, quanHeChan.BlockerTaiKhoanIdServer);
            if (sanPhamCuaTaiKhoanBiChan is not null && taiKhoanChan is not null)
            {
                return new CapTaiKhoanVaSanPham(taiKhoanChan, sanPhamCuaTaiKhoanBiChan);
            }

            var sanPhamCuaTaiKhoanChan = sanPhams.FirstOrDefault(x => x.TaiKhoanIdServer == quanHeChan.BlockerTaiKhoanIdServer);
            var taiKhoanBiChan = LayTaiKhoanTheoServerId(ctx, quanHeChan.BlockedTaiKhoanIdServer);
            if (sanPhamCuaTaiKhoanChan is not null && taiKhoanBiChan is not null)
            {
                return new CapTaiKhoanVaSanPham(taiKhoanBiChan, sanPhamCuaTaiKhoanChan);
            }
        }

        throw new BoQuaKiemThuException("Thiếu cặp user/sản phẩm có quan hệ block trong tk_chan_seed.");
    }

    internal static bool LikeSanPhamDangLuu(TaiKhoanThichSanPhamSeed like)
    {
        return like.TaiKhoanIdServer is > 0 &&
               like.SanPhamIdServer is > 0;
    }

    internal static CapTaiKhoanVaSanPham ChonCapTaiKhoanSanPhamChuaLike(NguCanhKiemThu ctx)
    {
        var taiKhoanSanSang = LayDanhSachTaiKhoanSanSang(ctx);
        var likeDangLuu = ctx.CapNhatDB.DuLieu.TaiKhoanThichSanPhamSeed
            .Where(LikeSanPhamDangLuu)
            .ToList();

        foreach (var sanPham in LayDanhSachSanPhamSanSang(ctx))
        {
            var sanPhamIdServer = IdBatBuoc(sanPham.SanPhamIdServer, "sanpham_seed.sp_id_server");
            var taiKhoan = taiKhoanSanSang.FirstOrDefault(user =>
                user.TaiKhoanIdServer != sanPham.TaiKhoanIdServer &&
                !CoQuanHeChan(ctx, user.TaiKhoanIdServer, sanPham.TaiKhoanIdServer) &&
                !likeDangLuu.Any(like =>
                    like.TaiKhoanIdServer == user.TaiKhoanIdServer &&
                    like.SanPhamIdServer == sanPhamIdServer));

            if (taiKhoan is not null)
            {
                return new CapTaiKhoanVaSanPham(taiKhoan, sanPham);
            }
        }

        throw new BoQuaKiemThuException("Thiếu cặp tài khoản/sản phẩm chưa like để chạy PRODUCT-LIKE-01.");
    }

    internal static CapLikeSanPham LayLikeSanPhamDangLuu(NguCanhKiemThu ctx)
    {
        foreach (var like in ctx.CapNhatDB.DuLieu.TaiKhoanThichSanPhamSeed
                     .Where(LikeSanPhamDangLuu)
                     .OrderBy(x => x.ThichSanPhamSeedId))
        {
            var taiKhoan = LayTaiKhoanTheoServerId(ctx, like.TaiKhoanIdServer);
            var sanPham = LaySanPhamTheoServerId(ctx, like.SanPhamIdServer);
            if (taiKhoan is not null && sanPham is not null)
            {
                return new CapLikeSanPham(like, taiKhoan, sanPham);
            }
        }

        throw new BoQuaKiemThuException("Thiếu tk_thich_sanpham_seed đang lưu để chạy PRODUCT-LIKE-02.");
    }
}
