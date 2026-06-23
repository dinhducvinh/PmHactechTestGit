using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private readonly NguCanhKiemThu _nguCanh;
    private readonly KeHoachChuanBiDuLieuSeed _keHoach;

    public ChuanBiSeed(NguCanhKiemThu nguCanh, KeHoachChuanBiDuLieuSeed keHoach)
    {
        _nguCanh = nguCanh;
        _keHoach = keHoach;
    }

    public static async Task<KetQuaChuanBiDuLieuSeed> KiemTraVaChuanBiAsync(
        CauHinhChay cauHinh,
        Action<string>? baoTrangThai = null,
        CancellationToken ct = default)
    {
        var seedCheck = new SeedLoadCheck();
        if (baoTrangThai is not null)
        {
            seedCheck.TrangThaiThayDoi += baoTrangThai;
        }

        KetQuaKiemTraDuLieuSeed kiemTraTruoc;
        try
        {
            kiemTraTruoc = await seedCheck.KiemTraAsync(cauHinh.ChuoiKetNoiSqlServer, ct);
        }
        catch (Exception ex)
        {
            return KetQuaChuanBiDuLieuSeed.ThatBai(
                cauHinh.BaseUrl,
                "Không thể đọc database seed.",
                ex.Message,
                []);
        }

        ct.ThrowIfCancellationRequested();
        var daDongBoTinhThanhPhuongXa = false;
        if (kiemTraTruoc.ThongKe.SoTaiKhoanDaDangKy > 0)
        {
            baoTrangThai?.Invoke("Đang đồng bộ tỉnh/thành phố và phường/xã từ API...");
            try
            {
                kiemTraTruoc = await DongBoTinhThanhPhuongXaVaKiemTraLaiAsync(
                    cauHinh,
                    seedCheck,
                    kiemTraTruoc,
                    ct);
                daDongBoTinhThanhPhuongXa = true;
            }
            catch (Exception ex)
            {
                return KetQuaChuanBiDuLieuSeed.ThatBai(
                    cauHinh.BaseUrl,
                    "Không thể đồng bộ tỉnh/thành phố và phường/xã từ API.",
                    ex.Message,
                    kiemTraTruoc.DuLieuConThieu,
                    kiemTraTruoc.ThongKe);
            }
        }

        ct.ThrowIfCancellationRequested();
        if (kiemTraTruoc.DaDuDuLieu)
        {
            return new KetQuaChuanBiDuLieuSeed
            {
                BaseUrl = cauHinh.BaseUrl,
                DaDuTruocKhiChuanBi = true,
                DaThuBoSung = daDongBoTinhThanhPhuongXa,
                DaDuSauKhiChuanBi = true,
                ThongKe = kiemTraTruoc.ThongKe,
                ThongDiep = daDongBoTinhThanhPhuongXa
                    ? "Dữ liệu mồi trong database seed đã đủ sau khi đồng bộ tỉnh/thành phố và phường/xã từ API."
                    : "Dữ liệu mồi trong database seed đã đủ."
            };
        }

        if (!kiemTraTruoc.KeHoach.CoHangMucCanChuanBi)
        {
            return new KetQuaChuanBiDuLieuSeed
            {
                BaseUrl = cauHinh.BaseUrl,
                DaDuTruocKhiChuanBi = false,
                DaThuBoSung = false,
                DaDuSauKhiChuanBi = false,
                DuLieuConThieuTruoc = kiemTraTruoc.DuLieuConThieu,
                DuLieuConThieuSau = kiemTraTruoc.DuLieuConThieu,
                ThongKe = kiemTraTruoc.ThongKe,
                ThongDiep = "Dữ liệu seed còn thiếu nhưng không có hạng mục nào có thể tự bổ sung bằng API."
            };
        }

        baoTrangThai?.Invoke("Dữ liệu seed chưa đủ. Đang gọi API để bổ sung dữ liệu mồi...");
        try
        {
            using var api = new MayKhachApi(cauHinh);
            var capNhatDB = new CapNhatDB(cauHinh.ChuoiKetNoiSqlServer, kiemTraTruoc.DuLieu);
            var nguCanh = new NguCanhKiemThu(cauHinh, api, capNhatDB);
            await new ChuanBiSeed(nguCanh, kiemTraTruoc.KeHoach).ChuanBiAsync();
        }
        catch (Exception ex)
        {
            var thongKeLoi = kiemTraTruoc.ThongKe;
            var thieuSauLoi = kiemTraTruoc.DuLieuConThieu;
            try
            {
                var kiemTraLoi = await seedCheck.KiemTraAsync(cauHinh.ChuoiKetNoiSqlServer, ct);
                thongKeLoi = kiemTraLoi.ThongKe;
                thieuSauLoi = kiemTraLoi.DuLieuConThieu;
            }
            catch
            {
                // Giữ thống kê trước khi chạy nếu không đọc lại được database seed.
            }

            return KetQuaChuanBiDuLieuSeed.ThatBai(
                cauHinh.BaseUrl,
                "Không thể gọi API để chuẩn bị dữ liệu mồi.",
                ex.Message,
                thieuSauLoi,
                thongKeLoi);
        }

        ct.ThrowIfCancellationRequested();
        baoTrangThai?.Invoke("Đang kiểm tra lại dữ liệu seed sau khi bổ sung...");

        KetQuaKiemTraDuLieuSeed kiemTraSau;
        try
        {
            kiemTraSau = await seedCheck.KiemTraAsync(cauHinh.ChuoiKetNoiSqlServer, ct);
        }
        catch (Exception ex)
        {
            return KetQuaChuanBiDuLieuSeed.ThatBai(
                cauHinh.BaseUrl,
                "Đã thử bổ sung dữ liệu mồi nhưng không thể đọc lại database seed.",
                ex.Message,
                kiemTraTruoc.DuLieuConThieu,
                kiemTraTruoc.ThongKe);
        }

        if (kiemTraSau.DaDuDuLieu)
        {
            return new KetQuaChuanBiDuLieuSeed
            {
                BaseUrl = cauHinh.BaseUrl,
                DaDuTruocKhiChuanBi = false,
                DaThuBoSung = true,
                DaDuSauKhiChuanBi = true,
                DuLieuConThieuTruoc = kiemTraTruoc.DuLieuConThieu,
                ThongKe = kiemTraSau.ThongKe,
                ThongDiep = "Đã bổ sung đầy đủ dữ liệu mồi cần thiết."
            };
        }

        return new KetQuaChuanBiDuLieuSeed
        {
            BaseUrl = cauHinh.BaseUrl,
            DaDuTruocKhiChuanBi = false,
            DaThuBoSung = true,
            DaDuSauKhiChuanBi = false,
            DuLieuConThieuTruoc = kiemTraTruoc.DuLieuConThieu,
            DuLieuConThieuSau = kiemTraSau.DuLieuConThieu,
            ThongKe = kiemTraSau.ThongKe,
            ThongDiep = "Không thể bổ sung đủ dữ liệu mồi. Có thể một số API chuẩn bị seed chưa gọi thành công."
        };
    }

    public static string TaoNoiDungThongBao(KetQuaChuanBiDuLieuSeed ketQua)
    {
        var dong = new List<string>
        {
            ketQua.ThongDiep
        };

        if (!string.IsNullOrWhiteSpace(ketQua.BaseUrl))
        {
            dong.Add("");
            dong.Add("Base URL: " + ketQua.BaseUrl);
        }

        if (ketQua.ThongKe is not null && ketQua.DaDuSauKhiChuanBi)
        {
            ThemThongKeVaoThongBao(dong, ketQua.ThongKe);
        }

        if (ketQua.DaDuTruocKhiChuanBi)
        {
            dong.Add("");
            dong.Add(ketQua.DaThuBoSung
                ? "Đã đồng bộ dữ liệu tỉnh/thành phố và phường/xã từ API; không cần gọi thêm API để bổ sung dữ liệu mồi."
                : "Không cần gọi API để bổ sung dữ liệu mồi.");
            return string.Join(Environment.NewLine, dong);
        }

        if (ketQua.DaThuBoSung && ketQua.DaDuSauKhiChuanBi)
        {
            dong.Add("");
            dong.Add("App đã gọi API để tạo/đồng bộ dữ liệu thật trên server và cập nhật trạng thái seed trong SQL Server.");
            return string.Join(Environment.NewLine, dong);
        }

        if (ketQua.DuLieuConThieuSau.Count > 0)
        {
            dong.Add("");
            dong.Add("Dữ liệu còn thiếu:");
            foreach (var item in ketQua.DuLieuConThieuSau.Take(8))
            {
                dong.Add("- " + item);
            }
        }

        if (!string.IsNullOrWhiteSpace(ketQua.LoiChiTiet))
        {
            dong.Add("");
            dong.Add("Chi tiết lỗi: " + ketQua.LoiChiTiet);
        }

        dong.Add("");
        dong.Add("Kiểm tra lại SQL Server, Base URL và các API signup/login, wallet, địa chỉ, danh mục/thương hiệu, sản phẩm, giỏ hàng, đơn hàng, saved search, follow/block, tin nhắn.");
        return string.Join(Environment.NewLine, dong);
    }

    private static void ThemThongKeVaoThongBao(List<string> dong, ThongKeDuLieuSeed thongKe)
    {
        dong.Add("");
        dong.Add("Số lượng seed hiện tại:");
        dong.Add($"- Tài khoản đã đăng ký qua API signup: {thongKe.SoTaiKhoanDaDangKy}/{thongKe.SoTaiKhoanDangKyCanCo}.");
        dong.Add($"- Tỉnh/thành phố seed đã đồng bộ từ API: {thongKe.SoTinhThanh}.");
        dong.Add($"- Phường/xã seed đã đồng bộ từ API: {thongKe.SoPhuongXa}.");
        dong.Add($"- Saved search dang_luu: {thongKe.SoSavedSearchDangLuu}/{thongKe.SoSavedSearchCanCo}.");
        dong.Add($"- Quan hệ follow dang_theo_doi: {thongKe.SoQuanHeFollowDangTheoDoi} quan hệ; tài khoản chính có {thongKe.SoQuanHeFollowToiDaCuaMotTaiKhoan} quan hệ, yêu cầu tối thiểu {thongKe.SoQuanHeFollowCanCoCuaMotTaiKhoan}.");
        dong.Add($"- Quan hệ block dang_chan: {thongKe.SoQuanHeBlockDangChan} quan hệ; tài khoản chính có {thongKe.SoQuanHeBlockToiDaCuaMotTaiKhoan} quan hệ, yêu cầu tối thiểu {thongKe.SoQuanHeBlockCanCoCuaMotTaiKhoan}.");
        dong.Add($"- Địa chỉ tài khoản san_sang: {thongKe.SoDiaChiTaiKhoanSanSang}/{thongKe.SoDiaChiTaiKhoanCanCo}.");
        dong.Add($"- Danh mục seed san_sang: {thongKe.SoDanhMucSanSang}/{thongKe.SoDanhMucCanCo}.");
        dong.Add($"- Thương hiệu seed san_sang: {thongKe.SoThuongHieuSanSang}.");
        dong.Add($"- Sản phẩm seed san_sang: {thongKe.SoSanPhamSanSang}/{thongKe.SoSanPhamCanCo}.");
        dong.Add($"- Giỏ hàng seed dang_trong_gio: {thongKe.SoGioHangDangTrongGio}/{thongKe.SoGioHangCanCo}.");
        dong.Add($"- Đơn hàng seed: {thongKe.SoDonHangDangLuu}/{thongKe.SoDonHangCanCo}.");
        dong.Add($"- Like sản phẩm seed đang lưu: {thongKe.SoLikeSanPhamDangLuu}/{thongKe.SoLikeSanPhamCanCo}.");
        dong.Add($"- Tin nhắn seed da_gui: {thongKe.SoTinNhanDaGui}/{thongKe.SoTinNhanCanCo}.");
        dong.Add($"- Thông báo seed dang_luu: {thongKe.SoThongBaoDangLuu}/{thongKe.SoThongBaoCanCo}.");
    }

    public async Task ChuanBiAsync()
    {
        if (!_keHoach.CoHangMucCanChuanBi)
        {
            return;
        }

        try
        {
            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.TaiKhoanDaDangKy))
            {
                await DangKyTaiKhoanChuaDangKyAsync();
            }

            await DongBoTinhThanhPhuongXaAsync();

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.DanhMucVaThuongHieu))
            {
                await DongBoDanhMucVaThuongHieuAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.DiaChiTaiKhoan))
            {
                await TaoDiaChiTaiKhoanSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.Chan))
            {
                await TaoChanSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.TheoDoi))
            {
                await TaoTheoDoiSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.TimKiem))
            {
                await TaoTimKiemSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.SanPham))
            {
                await TaoSanPhamSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.GioHang))
            {
                await TaoGioHangSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.DonHang))
            {
                await TaoDonHangSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.LikeSanPham))
            {
                await TaoLikeSanPhamSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.TinNhan))
            {
                await TaoTinNhanSeedAsync();
            }

            if (_keHoach.CanChuanBi(HangMucChuanBiDuLieuSeed.ThongBao))
            {
                await DongBoThongBaoSeedAsync();
            }
        }
        catch (LoiChuanBiKiemThuException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"Không kết nối được API seed tại Base URL {_nguCanh.CauHinh.BaseUrl}. Chi tiết: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"API seed bị timeout sau {_nguCanh.CauHinh.TimeoutGiay} giây. Chi tiết: {ex.Message}");
        }
    }

    private static async Task<KetQuaKiemTraDuLieuSeed> DongBoTinhThanhPhuongXaVaKiemTraLaiAsync(
        CauHinhChay cauHinh,
        SeedLoadCheck seedCheck,
        KetQuaKiemTraDuLieuSeed kiemTraHienTai,
        CancellationToken ct)
    {
        using var api = new MayKhachApi(cauHinh);
        var capNhatDB = new CapNhatDB(cauHinh.ChuoiKetNoiSqlServer, kiemTraHienTai.DuLieu);
        var nguCanh = new NguCanhKiemThu(cauHinh, api, capNhatDB);
        await new ChuanBiSeed(nguCanh, new KeHoachChuanBiDuLieuSeed([]))
            .DongBoTinhThanhPhuongXaAsync();

        ct.ThrowIfCancellationRequested();
        return await seedCheck.KiemTraAsync(cauHinh.ChuoiKetNoiSqlServer, ct);
    }

    private async Task<string> LayTokenSeedAsync(TaiKhoanSignupThanhCongSeed taiKhoan, string mucDich)
    {
        try
        {
            return await HelperTC.LayTokenCuaTaiKhoanAsync(_nguCanh, taiKhoan);
        }
        catch (BoQuaKiemThuException ex)
        {
            throw new LoiChuanBiKiemThuException($"{ex.Message} Mục đích: {mucDich}.");
        }
        catch (HttpRequestException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"Không kết nối được API /auth/login khi {mucDich}. Base URL: {_nguCanh.CauHinh.BaseUrl}. Chi tiết: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"API /auth/login bị timeout sau {_nguCanh.CauHinh.TimeoutGiay} giây khi {mucDich}. Chi tiết: {ex.Message}");
        }
    }

    private async Task<PhanHoiApi> GuiApiSeedAsync(
        HttpMethod phuongThuc,
        string duongDan,
        object? body = null,
        string? token = null,
        string mucDich = "chuẩn bị dữ liệu seed")
    {
        try
        {
            var response = await _nguCanh.Api.GuiAsync(new YeuCauApi(phuongThuc, duongDan, body, token));
            if (response.MaSoSanh != "1000" &&
                !(duongDan == "/api/get_list_brands" && response.MaSoSanh == "9994"))
            {
                throw TaoLoiApiSeed(duongDan, response, mucDich);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"Không kết nối được API {duongDan} khi {mucDich}. Base URL: {_nguCanh.CauHinh.BaseUrl}. Chi tiết: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            throw new LoiChuanBiKiemThuException(
                $"API {duongDan} bị timeout sau {_nguCanh.CauHinh.TimeoutGiay} giây khi {mucDich}. Chi tiết: {ex.Message}");
        }
    }

    private static LoiChuanBiKiemThuException TaoLoiApiSeed(string duongDan, PhanHoiApi response, string mucDich)
    {
        var message = string.IsNullOrWhiteSpace(response.Message) ? "(không có message)" : response.Message;
        var raw = RutGon(response.NoiDungRaw);
        return new LoiChuanBiKiemThuException(
            $"API {duongDan} không thành công khi {mucDich}. HTTP {(int)response.HttpStatusCode}, code {response.MaSoSanh}, message: {message}. Response: {raw}");
    }

    private List<TaiKhoanSignupThanhCongSeed> LayTaiKhoanDaDangKySanSang()
    {
        return _nguCanh.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x =>
                x.TaiKhoanIdServer is > 0)
            .OrderBy(x => x.SoThuTu)
            .ToList();
    }

    private static IEnumerable<JsonObject> LayObjectTuNode(JsonNode? node)
    {
        if (node is JsonArray array)
        {
            return array.OfType<JsonObject>();
        }

        if (node is JsonObject obj)
        {
            foreach (var tenMang in new[] { "items", "rows", "list", "data", "results", "notifications", "brands", "categories" })
            {
                if (obj[tenMang] is JsonArray mang)
                {
                    return mang.OfType<JsonObject>();
                }
            }

            return [obj];
        }

        return [];
    }

    private static IEnumerable<JsonObject> LayObjectDeQuy(JsonNode? node)
    {
        foreach (var obj in LayObjectTuNode(node))
        {
            yield return obj;

            foreach (var tenMangCon in new[] { "children", "childs", "child", "sub_categories", "subCategories" })
            {
                if (obj[tenMangCon] is null)
                {
                    continue;
                }

                foreach (var con in LayObjectDeQuy(obj[tenMangCon]))
                {
                    yield return con;
                }
            }
        }
    }





    private static int? IdChoBody(int? id)
    {
        return id is > 0 ? id.Value : null;
    }

    private static int? IdChoBody(int id)
    {
        return id > 0 ? id : null;
    }

    private static int IdTiepTheo<T>(IEnumerable<T> danhSach, Func<T, int> layId)
    {
        return danhSach.Select(layId).DefaultIfEmpty(0).Max() + 1;
    }
}

public sealed class KetQuaChuanBiDuLieuSeed
{
    public required string BaseUrl { get; init; }
    public bool DaDuTruocKhiChuanBi { get; init; }
    public bool DaThuBoSung { get; init; }
    public bool DaDuSauKhiChuanBi { get; init; }
    public string ThongDiep { get; init; } = "";
    public string? LoiChiTiet { get; init; }
    public IReadOnlyList<string> DuLieuConThieuTruoc { get; init; } = [];
    public IReadOnlyList<string> DuLieuConThieuSau { get; init; } = [];
    public ThongKeDuLieuSeed? ThongKe { get; init; }

    public static KetQuaChuanBiDuLieuSeed ThatBai(
        string baseUrl,
        string thongDiep,
        string loiChiTiet,
        IReadOnlyList<string> duLieuConThieu,
        ThongKeDuLieuSeed? thongKe = null)
    {
        return new KetQuaChuanBiDuLieuSeed
        {
            BaseUrl = baseUrl,
            DaDuTruocKhiChuanBi = false,
            DaThuBoSung = duLieuConThieu.Count > 0,
            DaDuSauKhiChuanBi = false,
            ThongDiep = thongDiep,
            LoiChiTiet = loiChiTiet,
            DuLieuConThieuSau = duLieuConThieu,
            ThongKe = thongKe
        };
    }
}








