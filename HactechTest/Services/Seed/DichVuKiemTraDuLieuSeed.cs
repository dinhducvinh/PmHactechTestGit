using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.Services.Seed
{
    public sealed class DichVuKiemTraDuLieuSeed
    {
        private const int SoTaiKhoanDaDangKyToiThieu = 100;
        private const int SoSavedSearchToiThieu = 2;
        private const int SoQuanHeTheoDoiToiThieu = 2;
        private const int SoQuanHeChanToiThieu = 2;
        private const int SoTinhThanhToiThieu = 5;
        private const int SoPhuongXaToiThieu = 28;
        private const int SoDiaChiTaiKhoanToiThieu = 50;
        private const int SoDanhMucToiThieu = 1;
        private const int SoSanPhamToiThieu = 60;
        private const int SoLikeSanPhamToiThieu = 5;
        private const int SoTinNhanToiThieu = 5;

        public event Action<string>? TrangThaiThayDoi;

        public async Task<KetQuaChuanBiDuLieuSeed> KiemTraVaChuanBiAsync(CauHinhChay cauHinh, CancellationToken ct = default)
        {
            BaoTrangThai("Đang kết nối database seed...");

            var khoSeed = new KhoDuLieuSeedSqlServer(cauHinh.ChuoiKetNoiSqlServer);
            try
            {
                await khoSeed.TaiAsync();
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

            var thongKeTruoc = TaoThongKeSeed(khoSeed.DuLieu);
            var thieuTruoc = LietKeDuLieuConThieu(thongKeTruoc);
            if (thieuTruoc.Count == 0)
            {
                return new KetQuaChuanBiDuLieuSeed
                {
                    BaseUrl = cauHinh.BaseUrl,
                    DaDuTruocKhiChuanBi = true,
                    DaThuBoSung = false,
                    DaDuSauKhiChuanBi = true,
                    ThongKe = thongKeTruoc,
                    ThongDiep = "Dữ liệu mồi trong database seed đã đủ."
                };
            }

            BaoTrangThai("Dữ liệu seed chưa đủ. Đang gọi API để bổ sung dữ liệu mồi...");

            try
            {
                using var api = new MayKhachApi(cauHinh.BaseUrl, TimeSpan.FromSeconds(cauHinh.TimeoutGiay));
                var nguCanh = new NguCanhKiemThu(cauHinh, api, khoSeed);
                await new ChuanBiDuLieuMoi(nguCanh, SoTaiKhoanDaDangKyToiThieu).ChuanBiAsync();
            }
            catch (Exception ex)
            {
                var thongKeLoi = thongKeTruoc;
                var thieuSauLoi = thieuTruoc;
                try
                {
                    await khoSeed.TaiAsync();
                    thongKeLoi = TaoThongKeSeed(khoSeed.DuLieu);
                    thieuSauLoi = LietKeDuLieuConThieu(thongKeLoi);
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
            BaoTrangThai("Đang kiểm tra lại dữ liệu seed sau khi bổ sung...");

            try
            {
                await khoSeed.TaiAsync();
            }
            catch (Exception ex)
            {
                return KetQuaChuanBiDuLieuSeed.ThatBai(
                    cauHinh.BaseUrl,
                    "Đã thử bổ sung dữ liệu mồi nhưng không thể đọc lại database seed.",
                    ex.Message,
                    thieuTruoc,
                    thongKeTruoc);
            }

            var thongKeSau = TaoThongKeSeed(khoSeed.DuLieu);
            var thieuSau = LietKeDuLieuConThieu(thongKeSau);
            if (thieuSau.Count == 0)
            {
                return new KetQuaChuanBiDuLieuSeed
                {
                    BaseUrl = cauHinh.BaseUrl,
                    DaDuTruocKhiChuanBi = false,
                    DaThuBoSung = true,
                    DaDuSauKhiChuanBi = true,
                    DuLieuConThieuTruoc = thieuTruoc,
                    ThongKe = thongKeSau,
                    ThongDiep = "Đã bổ sung đầy đủ dữ liệu mồi cần thiết."
                };
            }

            return new KetQuaChuanBiDuLieuSeed
            {
                BaseUrl = cauHinh.BaseUrl,
                DaDuTruocKhiChuanBi = false,
                DaThuBoSung = true,
                DaDuSauKhiChuanBi = false,
                DuLieuConThieuTruoc = thieuTruoc,
                DuLieuConThieuSau = thieuSau,
                ThongKe = thongKeSau,
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
                dong.Add("Không cần gọi API để bổ sung dữ liệu mồi.");
                return string.Join(Environment.NewLine, dong);
            }

            if (ketQua.DaThuBoSung && ketQua.DaDuSauKhiChuanBi)
            {
                dong.Add("");
                dong.Add("App đã gọi API để tạo dữ liệu thật trên server và cập nhật trạng thái seed trong SQL Server.");
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
            dong.Add("Kiểm tra lại SQL Server, các file SQL trong HactechTest\\Database, Base URL và các API signup/login, địa chỉ, danh mục/thương hiệu, sản phẩm, saved search, follow/block, tin nhắn.");
            return string.Join(Environment.NewLine, dong);
        }

        private static void ThemThongKeVaoThongBao(List<string> dong, ThongKeDuLieuSeed thongKe)
        {
            dong.Add("");
            dong.Add("Số lượng seed hiện tại:");
            dong.Add($"- Tài khoản đã đăng ký qua API signup: {thongKe.SoTaiKhoanDaDangKy}/{thongKe.SoTaiKhoanDangKyCanCo}.");
            dong.Add($"- Tỉnh/thành phố seed: {thongKe.SoTinhThanh}/{thongKe.SoTinhThanhCanCo}.");
            dong.Add($"- Phường/xã seed: {thongKe.SoPhuongXa}/{thongKe.SoPhuongXaCanCo}.");
            dong.Add($"- Saved search dang_luu: {thongKe.SoSavedSearchDangLuu}/{thongKe.SoSavedSearchCanCo}.");
            dong.Add($"- Quan hệ follow dang_theo_doi: {thongKe.SoQuanHeFollowDangTheoDoi} quan hệ; tài khoản chính có {thongKe.SoQuanHeFollowToiDaCuaMotTaiKhoan} quan hệ, yêu cầu tối thiểu {thongKe.SoQuanHeFollowCanCoCuaMotTaiKhoan}.");
            dong.Add($"- Quan hệ block dang_chan: {thongKe.SoQuanHeBlockDangChan} quan hệ; tài khoản chính có {thongKe.SoQuanHeBlockToiDaCuaMotTaiKhoan} quan hệ, yêu cầu tối thiểu {thongKe.SoQuanHeBlockCanCoCuaMotTaiKhoan}.");
            dong.Add($"- Địa chỉ tài khoản san_sang: {thongKe.SoDiaChiTaiKhoanSanSang}/{thongKe.SoDiaChiTaiKhoanCanCo}.");
            dong.Add($"- Danh mục seed san_sang: {thongKe.SoDanhMucSanSang}/{thongKe.SoDanhMucCanCo}.");
            dong.Add($"- Thương hiệu seed san_sang: {thongKe.SoThuongHieuSanSang}.");
            dong.Add($"- Sản phẩm seed san_sang: {thongKe.SoSanPhamSanSang}/{thongKe.SoSanPhamCanCo}.");
            dong.Add($"- Like sản phẩm seed san_sang: {thongKe.SoLikeSanPhamSanSang}/{thongKe.SoLikeSanPhamCanCo}.");
            dong.Add($"- Tin nhắn seed da_gui: {thongKe.SoTinNhanDaGui}/{thongKe.SoTinNhanCanCo}.");
            dong.Add($"- Thông báo seed dang_luu: {thongKe.SoThongBaoDangLuu}.");
        }

        private void BaoTrangThai(string noiDung)
        {
            TrangThaiThayDoi?.Invoke(noiDung);
        }

        private static ThongKeDuLieuSeed TaoThongKeSeed(DuLieuSeed duLieu)
        {
            return new ThongKeDuLieuSeed
            {
                SoTaiKhoanDaDangKy = DemTaiKhoanDaDangKy(duLieu),
                SoTaiKhoanDangKyCanCo = SoTaiKhoanDaDangKyToiThieu,
                SoTinhThanh = duLieu.TinhThanhSeed.Count,
                SoTinhThanhCanCo = SoTinhThanhToiThieu,
                SoPhuongXa = duLieu.PhuongXaSeed.Count,
                SoPhuongXaCanCo = SoPhuongXaToiThieu,
                SoSavedSearchDangLuu = duLieu.TaiKhoanTimKiemSeed.Count(x => x.TrangThai == "dang_luu"),
                SoSavedSearchCanCo = SoSavedSearchToiThieu,
                SoQuanHeFollowDangTheoDoi = duLieu.TaiKhoanTheoDoiSeed.Count(x => x.TrangThai == "dang_theo_doi"),
                SoQuanHeFollowToiDaCuaMotTaiKhoan = DemQuanHeNhieuNhat(duLieu.TaiKhoanTheoDoiSeed, x => x.FollowerTaiKhoanIdServer, x => x.TrangThai == "dang_theo_doi"),
                SoQuanHeFollowCanCoCuaMotTaiKhoan = SoQuanHeTheoDoiToiThieu,
                SoQuanHeBlockDangChan = duLieu.TaiKhoanChanSeed.Count(x => x.TrangThai == "dang_chan"),
                SoQuanHeBlockToiDaCuaMotTaiKhoan = DemQuanHeNhieuNhat(duLieu.TaiKhoanChanSeed, x => x.BlockerTaiKhoanIdServer, x => x.TrangThai == "dang_chan"),
                SoQuanHeBlockCanCoCuaMotTaiKhoan = SoQuanHeChanToiThieu,
                SoDiaChiTaiKhoanSanSang = duLieu.DiaChiTaiKhoanSeed.Count(x => x.TrangThai == "san_sang"),
                SoDiaChiTaiKhoanCanCo = SoDiaChiTaiKhoanToiThieu,
                SoDanhMucSanSang = duLieu.DanhMucSeed.Count(x => x.TrangThai == "san_sang"),
                SoDanhMucCanCo = SoDanhMucToiThieu,
                SoThuongHieuSanSang = duLieu.ThuongHieuSeed.Count(x => x.TrangThai == "san_sang"),
                SoSanPhamSanSang = duLieu.SanPhamSeed.Count(x => x.TrangThai == "san_sang"),
                SoSanPhamCanCo = SoSanPhamToiThieu,
                SoLikeSanPhamSanSang = duLieu.TaiKhoanThichSanPhamSeed.Count(x => x.TrangThai == "san_sang"),
                SoLikeSanPhamCanCo = SoLikeSanPhamToiThieu,
                SoTinNhanDaGui = duLieu.TinNhanSeed.Count(x => x.TrangThai == "da_gui"),
                SoTinNhanCanCo = SoTinNhanToiThieu,
                SoThongBaoDangLuu = duLieu.ThongBaoSeed.Count(x => x.TrangThai == "dang_luu")
            };
        }

        private static List<string> LietKeDuLieuConThieu(ThongKeDuLieuSeed thongKe)
        {
            var thieu = new List<string>();

            if (thongKe.SoTaiKhoanDaDangKy < thongKe.SoTaiKhoanDangKyCanCo)
            {
                thieu.Add($"Thiếu tài khoản đã đăng ký qua API signup: hiện có {thongKe.SoTaiKhoanDaDangKy}/{thongKe.SoTaiKhoanDangKyCanCo}.");
            }

            if (thongKe.SoTinhThanh < thongKe.SoTinhThanhCanCo)
            {
                thieu.Add($"Thiếu tỉnh/thành phố seed: hiện có {thongKe.SoTinhThanh}/{thongKe.SoTinhThanhCanCo}. Hãy chạy InsertProvinceWardSeed.sql.");
            }

            if (thongKe.SoPhuongXa < thongKe.SoPhuongXaCanCo)
            {
                thieu.Add($"Thiếu phường/xã seed: hiện có {thongKe.SoPhuongXa}/{thongKe.SoPhuongXaCanCo}. Hãy chạy InsertProvinceWardSeed.sql.");
            }

            if (thongKe.SoSavedSearchDangLuu < thongKe.SoSavedSearchCanCo)
            {
                thieu.Add($"Thiếu saved search dang_luu: hiện có {thongKe.SoSavedSearchDangLuu}/{thongKe.SoSavedSearchCanCo}.");
            }

            if (thongKe.SoQuanHeFollowToiDaCuaMotTaiKhoan < thongKe.SoQuanHeFollowCanCoCuaMotTaiKhoan)
            {
                thieu.Add($"Thiếu quan hệ follow dang_theo_doi: hiện có tối đa {thongKe.SoQuanHeFollowToiDaCuaMotTaiKhoan}/{thongKe.SoQuanHeFollowCanCoCuaMotTaiKhoan} cho một tài khoản.");
            }

            if (thongKe.SoQuanHeBlockToiDaCuaMotTaiKhoan < thongKe.SoQuanHeBlockCanCoCuaMotTaiKhoan)
            {
                thieu.Add($"Thiếu quan hệ block dang_chan: hiện có tối đa {thongKe.SoQuanHeBlockToiDaCuaMotTaiKhoan}/{thongKe.SoQuanHeBlockCanCoCuaMotTaiKhoan} cho một tài khoản.");
            }

            if (thongKe.SoDiaChiTaiKhoanSanSang < thongKe.SoDiaChiTaiKhoanCanCo)
            {
                thieu.Add($"Thiếu địa chỉ tài khoản san_sang: hiện có {thongKe.SoDiaChiTaiKhoanSanSang}/{thongKe.SoDiaChiTaiKhoanCanCo}.");
            }

            if (thongKe.SoDanhMucSanSang < thongKe.SoDanhMucCanCo)
            {
                thieu.Add($"Thiếu danh mục seed san_sang: hiện có {thongKe.SoDanhMucSanSang}/{thongKe.SoDanhMucCanCo}.");
            }

            if (thongKe.SoSanPhamSanSang < thongKe.SoSanPhamCanCo)
            {
                thieu.Add($"Thiếu sản phẩm seed san_sang: hiện có {thongKe.SoSanPhamSanSang}/{thongKe.SoSanPhamCanCo}.");
            }

            if (thongKe.SoLikeSanPhamSanSang < thongKe.SoLikeSanPhamCanCo)
            {
                thieu.Add($"Thiếu like sản phẩm seed san_sang: hiện có {thongKe.SoLikeSanPhamSanSang}/{thongKe.SoLikeSanPhamCanCo}.");
            }

            if (thongKe.SoTinNhanDaGui < thongKe.SoTinNhanCanCo)
            {
                thieu.Add($"Thiếu tin nhắn seed da_gui: hiện có {thongKe.SoTinNhanDaGui}/{thongKe.SoTinNhanCanCo}.");
            }

            return thieu;
        }

        private static int DemTaiKhoanDaDangKy(DuLieuSeed duLieu)
        {
            return duLieu.TaiKhoanSignupThanhCongSeed.Count(x =>
                !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer));
        }

        private static int DemQuanHeNhieuNhat<T>(
            IEnumerable<T> danhSach,
            Func<T, string?> layKhoa,
            Func<T, bool> dieuKien)
        {
            return danhSach
                .Where(dieuKien)
                .Select(layKhoa)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x)
                .Select(x => x.Count())
                .DefaultIfEmpty(0)
                .Max();
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

    public sealed class ThongKeDuLieuSeed
    {
        public int SoTaiKhoanDaDangKy { get; init; }
        public int SoTaiKhoanDangKyCanCo { get; init; }
        public int SoTinhThanh { get; init; }
        public int SoTinhThanhCanCo { get; init; }
        public int SoPhuongXa { get; init; }
        public int SoPhuongXaCanCo { get; init; }
        public int SoSavedSearchDangLuu { get; init; }
        public int SoSavedSearchCanCo { get; init; }
        public int SoQuanHeFollowDangTheoDoi { get; init; }
        public int SoQuanHeFollowToiDaCuaMotTaiKhoan { get; init; }
        public int SoQuanHeFollowCanCoCuaMotTaiKhoan { get; init; }
        public int SoQuanHeBlockDangChan { get; init; }
        public int SoQuanHeBlockToiDaCuaMotTaiKhoan { get; init; }
        public int SoQuanHeBlockCanCoCuaMotTaiKhoan { get; init; }
        public int SoDiaChiTaiKhoanSanSang { get; init; }
        public int SoDiaChiTaiKhoanCanCo { get; init; }
        public int SoDanhMucSanSang { get; init; }
        public int SoDanhMucCanCo { get; init; }
        public int SoThuongHieuSanSang { get; init; }
        public int SoSanPhamSanSang { get; init; }
        public int SoSanPhamCanCo { get; init; }
        public int SoLikeSanPhamSanSang { get; init; }
        public int SoLikeSanPhamCanCo { get; init; }
        public int SoTinNhanDaGui { get; init; }
        public int SoTinNhanCanCo { get; init; }
        public int SoThongBaoDangLuu { get; init; }
    }
}

