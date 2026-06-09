using System.Diagnostics;
using System.Text.Json;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.KichBan;
using HactechTest.ApiShopTesting.Seed;
using HactechTest.Services.App;
using HactechTest.Services.Configuration;
using HactechTest.Services.Data;
using HactechTest.Services.DynamicTests;
using HactechTest.Services.History;
using HactechTest.Services.Reports;
using HactechTest.Services.Seed;

namespace HactechTest.Control
{
    public partial class ChayTest : UserControl
    {
        private readonly IReadOnlyList<KichBanApi> _kichBanCode = BoKichBanApi.TaoTatCaKichBan();
        private readonly List<KichBanApi> _tatCaKichBan = new();
        private readonly List<KichBanApi> _dsTestCaseHienThi = new();
        private readonly List<KetQuaChay> _ketQuaPhienHienTai = new();

        private CancellationTokenSource? _cts;
        private bool _dangChay;
        private bool _dangNapBoLoc;
        private string _cheDoChayPhienHienTai = "all";
        private string _cheDoLoiPhienHienTai = "continue_on_fail";
        private DateTimeOffset? _batDauLuc;
        private DateTimeOffset? _ketThucLuc;

        private CauHinhChay _cauHinhMacDinh = new();

        public ChayTest()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                _cauHinhMacDinh = CauHinhUngDung.Instance.TaoCauHinhChay();

                KhoiTaoSuKienKetQua();
                GanCauHinhMacDinhLenGiaoDien();

                Load += ChayTest_Load;
                btnTaiTestCase.Click += async (_, _) => await TaiTestCaseTheoBoLocAsync();
                cboBoSuuTap.SelectedIndexChanged += CboBoSuuTap_SelectedIndexChanged;
                cboModule.SelectedIndexChanged += async (_, _) => await TaiTestCaseTheoBoLocAsync();
                btnChonTatCa.Click += (_, _) => ChonTatCaTrongList(true);
                btnBoChonTatCa.Click += (_, _) => ChonTatCaTrongList(false);
                btnChayTatCa.Click += async (_, _) => await ChayAsync(false);
                btnChayDaChon.Click += async (_, _) => await ChayAsync(true);
                btnDungLai.Click += (_, _) => _cts?.Cancel();
                btnLuuUrl.Click += (_, _) => LuuBaseUrl();
                btnLuuVaoCSDL.Click += async (_, _) => await BtnLuuVaoCSDL_ClickAsync();
                btnLuuBaoCao.Click += (_, _) => LuuBaoCao();
                btnKiemTraSeed.Click += async (_, _) => await BtnKiemTraSeed_ClickAsync();

            }
        }

        public bool CoKetQuaPhienChay => _ketQuaPhienHienTai.Count > 0;

        public bool DangChayTest => _dangChay;

        public int SoKetQuaPhienHienTai => _ketQuaPhienHienTai.Count;

        public void HuyNeuDangChay()
        {
            _cts?.Cancel();
        }

        private async void ChayTest_Load(object? sender, EventArgs e)
        {
            await NapDanhSachBoSuuTapAsync();
            await TaiTestCaseTheoBoLocAsync();
        }

        public async Task DatLaiBoLocAsync()
        {
            await NapDanhSachBoSuuTapAsync();
            if (cboBoSuuTap.Items.Count > 0) cboBoSuuTap.SelectedIndex = 0;
            if (cboModule.Items.Count > 0) cboModule.SelectedIndex = 0;
            await TaiTestCaseTheoBoLocAsync();
        }

        // ----------------------------------------------------------------
        // Cấu hình chạy bộ testcase API Shop
        // ----------------------------------------------------------------
        private void GanCauHinhMacDinhLenGiaoDien()
        {
            txtBaseUrl.Text = _cauHinhMacDinh.BaseUrl;
            numTimeout.Value = Math.Clamp(_cauHinhMacDinh.TimeoutGiay, 3, 300);
        }

        private bool TaoCauHinhChay(out CauHinhChay cauHinh)
        {
            cauHinh = _cauHinhMacDinh;
            if (!LayBaseUrlHopLe(out var baseUrl))
            {
                return false;
            }

            var connectionString = CauHinhUngDung.Instance.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Chưa cấu hình database. Hãy bấm Cấu hình DB ở màn hình đăng nhập.", "Thông báo");
                return false;
            }

            cauHinh = _cauHinhMacDinh with
            {
                BaseUrl = baseUrl,
                TimeoutGiay = (int)numTimeout.Value,
                ConnectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["HactechTestDb"] = connectionString
                }
            };
            return true;
        }

        private bool LayBaseUrlHopLe(out string baseUrl)
        {
            baseUrl = txtBaseUrl.Text.Trim().TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            {
                return true;
            }

            MessageBox.Show("Restful API URL phải là URL tuyệt đối. Ví dụ: http://localhost:8000.", "Thông báo");
            return false;
        }

        private void LuuBaseUrl()
        {
            if (!LayBaseUrlHopLe(out var baseUrl))
            {
                return;
            }

            CauHinhUngDung.LuuBaseUrlApi(baseUrl);
            txtBaseUrl.Text = baseUrl;
            _cauHinhMacDinh = _cauHinhMacDinh with { BaseUrl = baseUrl };

            MessageBox.Show("Đã lưu Restful API URL. Lần sau mở phần mềm sẽ tự điền URL này.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task BtnKiemTraSeed_ClickAsync()
        {
            if (!TaoCauHinhChay(out var cauHinh))
            {
                return;
            }

            var dichVu = new DichVuKiemTraDuLieuSeed();
            using var formThongBao = new FormThongBaoChuanBiSeed();
            dichVu.TrangThaiThayDoi += formThongBao.CapNhatTrangThai;

            KetQuaChuanBiDuLieuSeed ketQua;
            try
            {
                CapNhatTrangThaiKhiKiemTraSeed(true);
                var formCha = FindForm();
                if (formCha != null) formThongBao.Show(formCha);
                else formThongBao.Show();

                formThongBao.CapNhatTrangThai("Đang quét database seed theo cấu hình hiện tại...");
                ketQua = await dichVu.KiemTraVaChuanBiAsync(cauHinh);
            }
            catch (Exception ex)
            {
                ketQua = KetQuaChuanBiDuLieuSeed.ThatBai(
                    cauHinh.BaseUrl,
                    "Không thể kiểm tra hoặc chuẩn bị dữ liệu seed theo cấu hình hiện tại.",
                    ex.Message,
                    []);
            }
            finally
            {
                if (!formThongBao.IsDisposed)
                {
                    formThongBao.Close();
                }

                CapNhatTrangThaiKhiKiemTraSeed(false);
            }

            MessageBox.Show(
                DichVuKiemTraDuLieuSeed.TaoNoiDungThongBao(ketQua),
                ketQua.DaDuSauKhiChuanBi ? "Dữ liệu seed" : "Cảnh báo dữ liệu seed",
                MessageBoxButtons.OK,
                ketQua.DaDuSauKhiChuanBi ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        // ----------------------------------------------------------------
        // Nguồn test
        // ----------------------------------------------------------------
        private async Task NapDanhSachBoSuuTapAsync()
        {
            _dangNapBoLoc = true;
            try
            {
                await NapTatCaKichBanAsync();

                cboBoSuuTap.Items.Clear();
                cboBoSuuTap.Items.Add("API Shop");
                cboBoSuuTap.SelectedIndex = 0;

                cboModule.Items.Clear();
                cboModule.Items.Add("(Tất cả)");
                foreach (var nhom in _tatCaKichBan.Select(x => x.Nhom).Distinct().OrderBy(x => x))
                {
                    cboModule.Items.Add(nhom);
                }
                cboModule.SelectedIndex = 0;
            }
            finally
            {
                _dangNapBoLoc = false;
            }
        }

        private async void CboBoSuuTap_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await TaiTestCaseTheoBoLocAsync();
        }

        private async Task TaiTestCaseTheoBoLocAsync()
        {
            if (_dangNapBoLoc)
            {
                return;
            }

            await NapTatCaKichBanAsync();

            if (cboModule.Items.Count == 0)
            {
                return;
            }

            var nhom = cboModule.SelectedItem?.ToString();
            var danhSach = string.IsNullOrWhiteSpace(nhom) || nhom == "(Tất cả)"
                ? _tatCaKichBan
                : _tatCaKichBan.Where(x => string.Equals(x.Nhom, nhom, StringComparison.OrdinalIgnoreCase)).ToList();

            clbDanhSachTestCase.Items.Clear();
            _dsTestCaseHienThi.Clear();
            _dsTestCaseHienThi.AddRange(danhSach);

            foreach (var tc in _dsTestCaseHienThi)
            {
                clbDanhSachTestCase.Items.Add($"[{tc.Nhom}] {tc.Ma} - {tc.TenHienThi}", true);
            }

            lblTrangThaiChay.Text = $"Sẵn sàng - {_dsTestCaseHienThi.Count} testcase.";
            HienThiChiTietMacDinh();
        }

        private async Task NapTatCaKichBanAsync()
        {
            _tatCaKichBan.Clear();
            _tatCaKichBan.AddRange(_kichBanCode);

            var connectionString = CauHinhUngDung.Instance.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            try
            {
                var database = new Database(connectionString);
                var store = new TestCaseDongStore(database);
                var testCaseDong = await store.LayDanhSachAsync();
                _tatCaKichBan.AddRange(testCaseDong.Select(BoKichBanDong.TaoKichBan));
            }
            catch (Exception ex)
            {
                lblTrangThaiChay.Text = "Không tải được test case cơ bản từ database: " + ex.Message;
            }
        }

        private void ChonTatCaTrongList(bool isChecked)
        {
            for (var i = 0; i < clbDanhSachTestCase.Items.Count; i++)
            {
                clbDanhSachTestCase.SetItemChecked(i, isChecked);
            }
        }

        // ----------------------------------------------------------------
        // Chạy test
        // ----------------------------------------------------------------
        private async Task ChayAsync(bool chiChayCaseDaChon)
        {
            if (_dangChay) return;

            if (!TaoCauHinhChay(out var cauHinh))
            {
                return;
            }

            var dsCanChay = chiChayCaseDaChon
                ? _dsTestCaseHienThi.Where((_, index) => clbDanhSachTestCase.GetItemChecked(index)).ToList()
                : _dsTestCaseHienThi.ToList();

            if (dsCanChay.Count == 0)
            {
                MessageBox.Show("Không có testcase nào được chọn.", "Thông báo");
                return;
            }

            _dangChay = true;
            _cts = new CancellationTokenSource();
            _batDauLuc = DateTimeOffset.Now;
            _ketThucLuc = null;
            _cheDoChayPhienHienTai = chiChayCaseDaChon ? "selected" : "all";
            _cheDoLoiPhienHienTai = rdoDungKhiLoi.Checked ? "stop_on_fail" : "continue_on_fail";
            _ketQuaPhienHienTai.Clear();
            dgvKetQuaChay.Rows.Clear();
            prgTienTrinh.Maximum = dsCanChay.Count;
            prgTienTrinh.Value = 0;
            lblTrangThaiChay.Text = "Đang chạy...";
            lblTongKet.Text = "Đang thực hiện...";
            HienThiChiTietMacDinh();
            CapNhatTrangThaiNut(true);

            try
            {
                lblTrangThaiChay.Text = "Đang nạp dữ liệu seed...";
                var khoSeed = new KhoDuLieuSeedSqlServer(cauHinh.ChuoiKetNoiSqlServer);
                await khoSeed.TaiAsync();

                using var mayKhach = new MayKhachApi(cauHinh.BaseUrl, TimeSpan.FromSeconds(cauHinh.TimeoutGiay));
                var nguCanh = new NguCanhKiemThu(cauHinh, mayKhach, khoSeed);

                for (var i = 0; i < dsCanChay.Count; i++)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    var kichBan = dsCanChay[i];
                    lblTrangThaiChay.Text = $"Đang chạy {i + 1}/{dsCanChay.Count}: {kichBan.Ma}";

                    var ketQua = await ChayMotKichBanAsync(kichBan, nguCanh, _cts.Token);
                    _ketQuaPhienHienTai.Add(ketQua);
                    ThemDongKetQua(ketQua);
                    HienThiChiTietKetQua(ketQua);
                    prgTienTrinh.Value = Math.Min(prgTienTrinh.Maximum, i + 1);
                    CapNhatTongKet();

                    if (rdoDungKhiLoi.Checked && DinhDangKetQuaKiemThu.LaKhongDat(ketQua.TrangThai))
                    {
                        lblTrangThaiChay.Text = $"Đã dừng tại testcase failed: {ketQua.Ma}";
                        break;
                    }
                }

                if (_cts.IsCancellationRequested)
                {
                    lblTrangThaiChay.Text = "Đã dừng.";
                }
                else if (rdoDungKhiLoi.Checked && _ketQuaPhienHienTai.Any(x => DinhDangKetQuaKiemThu.LaKhongDat(x.TrangThai)))
                {
                    lblTrangThaiChay.Text = "Đã dừng do testcase failed.";
                }
                else
                {
                    lblTrangThaiChay.Text = "Hoàn tất.";
                }
            }
            catch (OperationCanceledException)
            {
                lblTrangThaiChay.Text = "Đã hủy.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chạy test:\n" + ex.Message, "Lỗi");
                lblTrangThaiChay.Text = "Lỗi khi chạy.";
            }
            finally
            {
                _ketThucLuc = DateTimeOffset.Now;
                _dangChay = false;
                _cts?.Dispose();
                _cts = null;
                CapNhatTrangThaiNut(false);
                CapNhatTongKet();
            }
        }

        private async Task<KetQuaChay> ChayMotKichBanAsync(
            KichBanApi kichBan,
            NguCanhKiemThu nguCanh,
            CancellationToken cancellationToken)
        {
            var dongHo = Stopwatch.StartNew();
            YeuCauApi? yeuCau = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(kichBan.LyDoBoQuaCoDinh))
                {
                    return TaoKetQua(kichBan, TrangThaiKetQua.BoQua, kichBan.LyDoBoQuaCoDinh!, dongHo.Elapsed);
                }

                if (kichBan.ChayRiengAsync is not null)
                {
                    return await kichBan.ChayRiengAsync(nguCanh, cancellationToken);
                }

                yeuCau = await kichBan.TaoYeuCauAsync(nguCanh);
                cancellationToken.ThrowIfCancellationRequested();
                var response = await nguCanh.Api.GuiAsync(yeuCau, cancellationToken);
                dongHo.Stop();

                if (!BoKichBanDong.LaMaChapNhanBatKy(kichBan.MaChapNhan) &&
                    !kichBan.MaChapNhan.Contains(response.MaSoSanh))
                {
                    var thongDiep =
                        $"Server trả mã nghiệp vụ không đúng. Mong đợi: {string.Join(", ", kichBan.MaChapNhan)}; " +
                        $"thực tế: {response.MaSoSanh}. Message server: {response.Message ?? "(không có)"}.";
                    return TaoKetQua(kichBan, TrangThaiKetQua.ThatBai, thongDiep, dongHo.Elapsed, yeuCau, response);
                }

                if (kichBan.KiemTraThemAsync is not null)
                {
                    var kiemTraThem = await kichBan.KiemTraThemAsync(response, yeuCau, nguCanh);
                    if (!kiemTraThem.Dat)
                    {
                        return TaoKetQua(kichBan, TrangThaiKetQua.ThatBai,
                            $"Mã nghiệp vụ đúng nhưng dữ liệu trả về sai kỳ vọng: {kiemTraThem.Loi}",
                            dongHo.Elapsed, yeuCau, response);
                    }
                }

                if (kichBan.SauKhiDatAsync is not null)
                {
                    await kichBan.SauKhiDatAsync(response, yeuCau, nguCanh);
                }

                return TaoKetQua(kichBan, TrangThaiKetQua.Dat,
                    "Đạt expected code và kiểm tra dữ liệu bổ sung.", dongHo.Elapsed, yeuCau, response);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (BoQuaKiemThuException ex)
            {
                dongHo.Stop();
                return TaoKetQua(kichBan, TrangThaiKetQua.BoQua, ex.Message, dongHo.Elapsed, yeuCau);
            }
            catch (LoiChuanBiKiemThuException ex)
            {
                dongHo.Stop();
                return TaoKetQua(kichBan, TrangThaiKetQua.LoiChuanBi, ex.Message, dongHo.Elapsed, yeuCau);
            }
            catch (HttpRequestException ex)
            {
                dongHo.Stop();
                return TaoKetQua(kichBan, TrangThaiKetQua.LoiMoiTruong,
                    $"Không gọi được API. Kiểm tra base URL/server/network. Chi tiết: {ex.Message}",
                    dongHo.Elapsed, yeuCau);
            }
            catch (TaskCanceledException)
            {
                dongHo.Stop();
                return TaoKetQua(kichBan, TrangThaiKetQua.LoiMoiTruong,
                    "Gọi API quá thời gian chờ. Kiểm tra server hoặc tăng timeout.",
                    dongHo.Elapsed, yeuCau);
            }
            catch (Exception ex)
            {
                dongHo.Stop();
                return TaoKetQua(kichBan, TrangThaiKetQua.LoiChuanBi,
                    $"Lỗi trong mã test, chưa kết luận được server đúng/sai: {ex.Message}",
                    dongHo.Elapsed, yeuCau);
            }
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
                RequestBodyJson = TaoRequestBodyJson(yeuCau?.Body),
                ResponseRutGon = TienIchJson.RutGon(response?.NoiDungRaw, 2000)
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
                return JsonSerializer.Serialize(body, TuyChonJson.MacDinh);
            }
            catch
            {
                return body.ToString();
            }
        }

        private void ThemDongKetQua(KetQuaChay r)
        {
            var index = dgvKetQuaChay.Rows.Add(
                _ketQuaPhienHienTai.Count,
                r.TenHienThi,
                DinhDangKetQuaKiemThu.TrangThaiHienThi(r.TrangThai),
                r.MaThucTe ?? (r.HttpStatus.HasValue ? "HTTP " + r.HttpStatus.Value : "-"),
                r.MaMongDoi ?? "-",
                (int)Math.Round(r.ThoiGian.TotalMilliseconds),
                r.ThongDiep);

            var row = dgvKetQuaChay.Rows[index];
            row.Tag = r;
            row.Cells[colLyDo.Index].ToolTipText = r.ThongDiep;
            row.Cells[colKetQua.Index].ToolTipText = DinhDangKetQuaKiemThu.TrangThaiHienThi(r.TrangThai);
            row.Cells[colTenTestCase.Index].ToolTipText = $"{r.Ma} - {r.TenHienThi}";
            row.DefaultCellStyle.BackColor = r.TrangThai switch
            {
                TrangThaiKetQua.Dat => Color.FromArgb(220, 245, 220),
                TrangThaiKetQua.ThatBai => Color.FromArgb(252, 220, 220),
                TrangThaiKetQua.LoiChuanBi => Color.FromArgb(255, 235, 200),
                TrangThaiKetQua.LoiMoiTruong => Color.FromArgb(255, 226, 210),
                TrangThaiKetQua.BoQua => Color.FromArgb(235, 235, 235),
                _ => Color.White
            };
        }

        private void CapNhatTongKet()
        {
            var tong = _ketQuaPhienHienTai.Count;
            var dat = _ketQuaPhienHienTai.Count(item => item.TrangThai == TrangThaiKetQua.Dat);
            var khongDat = tong - dat;
            var tyLe = tong == 0 ? 0 : Math.Round(100m * dat / tong, 2);
            var tb = tong == 0 ? 0 : (int)_ketQuaPhienHienTai.Average(item => item.ThoiGian.TotalMilliseconds);

            lblTongKet.Text =
                $"Tổng: {tong}   Đạt: {dat}   Không đạt: {khongDat}   Tỷ lệ: {tyLe}%   TB: {tb} ms";
        }

        private async Task BtnLuuVaoCSDL_ClickAsync()
        {
            if (_ketQuaPhienHienTai.Count == 0)
            {
                MessageBox.Show("Chưa có kết quả để lưu.", "Thông báo");
                return;
            }

            if (!TaoCauHinhChay(out var cauHinh))
            {
                return;
            }

            try
            {
                btnLuuVaoCSDL.Enabled = false;
                var database = new Database(cauHinh.ChuoiKetNoiSqlServer);

                var store = new PhienChayStore(database);
                var ketQuaLuu = _ketQuaPhienHienTai
                    .Select((item, index) => TaoKetQuaLuuPhien(item, index + 1, cauHinh.BaseUrl))
                    .ToList();
                var phienId = await store.LuuPhienChayAsync(
                    _cheDoChayPhienHienTai,
                    _cheDoLoiPhienHienTai,
                    cauHinh.BaseUrl,
                    ketQuaLuu);

                MessageBox.Show($"Đã lưu phiên chạy #{phienId} vào dbo.phien_chay và dbo.chi_tiet_phien_chay.", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lưu được kết quả SQL: " + ex.Message, "Lỗi");
            }
            finally
            {
                btnLuuVaoCSDL.Enabled = _ketQuaPhienHienTai.Count > 0;
            }
        }

        private static ChiTietKetQuaTestCase TaoKetQuaLuuPhien(KetQuaChay r, int sequenceNo, string baseUrl)
        {
            var (method, path) = TachEndpoint(r.Endpoint);
            return new ChiTietKetQuaTestCase
            {
                SequenceNo = sequenceNo,
                DisplayName = $"{r.Ma} - {r.TenHienThi}",
                HttpMethod = method,
                Url = TaoUrlDayDu(baseUrl, path),
                ExpectedAppCode = r.MaMongDoi ?? "",
                ActualAppCode = r.MaThucTe,
                HttpStatus = r.HttpStatus,
                Result = DinhDangKetQuaKiemThu.TrangThaiLuuDatabase(r.TrangThai),
                DurationMs = (long)Math.Round(r.ThoiGian.TotalMilliseconds),
                Reason = r.ThongDiep,
                RequestBodyJson = r.RequestBodyJson,
                ResponseBody = r.ResponseRutGon
            };
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

        private void LuuBaoCao()
        {
            if (_ketQuaPhienHienTai.Count == 0)
            {
                MessageBox.Show("Chưa có kết quả để lưu báo cáo.", "Thông báo");
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Lưu báo cáo phiên chạy",
                Filter = "HTML report (*.html)|*.html|JSON report (*.json)|*.json",
                FileName = $"bao-cao-api-shop-{DateTime.Now:yyyyMMdd-HHmmss}.html",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            var thongTinBaoCao = TaoThongTinBaoCao();
            var ketQuaLuu = new BaoCaoPhienChayService()
                .LuuBaoCao(dialog.FileName, thongTinBaoCao, _ketQuaPhienHienTai);

            MessageBox.Show($"Đã lưu báo cáo:\n{ketQuaLuu.DuongDanHtml}\n{ketQuaLuu.DuongDanJson}", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private ThongTinBaoCaoPhienChay TaoThongTinBaoCao()
        {
            return new ThongTinBaoCaoPhienChay(
                _batDauLuc,
                _ketThucLuc,
                LayTenDangNhapNguoiThucHien(),
                Environment.MachineName,
                Environment.OSVersion.VersionString,
                txtBaseUrl.Text,
                _cheDoChayPhienHienTai,
                _cheDoLoiPhienHienTai);
        }

        private static string LayTenDangNhapNguoiThucHien()
        {
            if (AppHost.IsInitialized && AppHost.Instance.TaiKhoanDangNhap is { } taiKhoan)
            {
                return taiKhoan.TenDangNhap;
            }

            return Environment.UserName;
        }

        // ----------------------------------------------------------------
        // Chi tiết kết quả
        // ----------------------------------------------------------------
        private void KhoiTaoSuKienKetQua()
        {
            dgvKetQuaChay.CellClick += DgvKetQuaChay_CellClick;
            dgvKetQuaChay.CellDoubleClick += DgvKetQuaChay_CellDoubleClick;
            clbDanhSachTestCase.SelectedIndexChanged += (_, _) =>
            {
                if (clbDanhSachTestCase.SelectedIndex >= 0 &&
                    clbDanhSachTestCase.SelectedIndex < _dsTestCaseHienThi.Count)
                {
                    HienThiMoTaTestCase(_dsTestCaseHienThi[clbDanhSachTestCase.SelectedIndex]);
                }
            };
        }

        private void DgvKetQuaChay_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (LayKetQuaTuDong(e.RowIndex) is KetQuaChay r)
            {
                HienThiChiTietKetQua(r);
            }
        }

        private void DgvKetQuaChay_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (LayKetQuaTuDong(e.RowIndex) is not KetQuaChay r) return;

            using var formChiTiet = new HactechTest.FormChiTietKetQuaTestCase(TaoChiTietKetQuaTest(r));
            var formCha = FindForm();
            if (formCha != null) formChiTiet.ShowDialog(formCha);
            else formChiTiet.ShowDialog();
        }

        private KetQuaChay? LayKetQuaTuDong(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvKetQuaChay.Rows.Count) return null;
            var row = dgvKetQuaChay.Rows[rowIndex];
            return row.IsNewRow ? null : row.Tag as KetQuaChay;
        }

        private void HienThiChiTietMacDinh()
        {
            lblChiTietCase.Text = "Chọn 1 testcase để xem mô tả hoặc chọn kết quả để xem response.";
            lblChiTietMeta.Text = "Kết quả, HTTP status và lý do sẽ hiển thị tại đây.";
            txtLyDoNhanh.Text = "Chọn một dòng kết quả để xem nhanh lý do lỗi.";
        }

        private void HienThiMoTaTestCase(KichBanApi tc)
        {
            lblChiTietCase.Text = $"{tc.Ma} - {tc.TenHienThi}";
            lblChiTietMeta.Text = $"Nhóm: {tc.Nhom} | Mã chấp nhận: {string.Join(", ", tc.MaChapNhan)}";
            txtLyDoNhanh.Text = tc.MoTa;
        }

        private void HienThiChiTietKetQua(KetQuaChay r)
        {
            lblChiTietCase.Text = $"{r.Ma} - {r.TenHienThi}";
            lblChiTietMeta.Text =
                $"{r.Endpoint ?? "-"} | expected: {r.MaMongDoi ?? "-"} | actual: {r.MaThucTe ?? "-"} | HTTP: {r.HttpStatus?.ToString() ?? "-"} | {(int)Math.Round(r.ThoiGian.TotalMilliseconds)} ms | {DinhDangKetQuaKiemThu.TrangThaiHienThi(r.TrangThai)}";
            txtLyDoNhanh.Text = string.IsNullOrWhiteSpace(r.ThongDiep)
                ? "(Không có lý do chi tiết)"
                : r.ThongDiep;
        }

        private static HactechTest.TestCaseResultDetailData TaoChiTietKetQuaTest(KetQuaChay r)
        {
            return new HactechTest.TestCaseResultDetailData
            {
                SequenceNo = "",
                TestCaseName = r.TenHienThi,
                Result = DinhDangKetQuaKiemThu.TrangThaiHienThi(r.TrangThai),
                ActualStatus = r.MaThucTe ?? (r.HttpStatus.HasValue ? "HTTP " + r.HttpStatus.Value : "-"),
                ExpectedStatus = r.MaMongDoi ?? "-",
                Duration = ((int)Math.Round(r.ThoiGian.TotalMilliseconds)).ToString(),
                Reason = r.ThongDiep,
                Method = r.Endpoint?.Split(' ').FirstOrDefault() ?? "",
                Url = r.Endpoint ?? "",
                ResponseBody = r.ResponseRutGon ?? ""
            };
        }

        private void CapNhatTrangThaiNut(bool dangChay)
        {
            btnChayTatCa.Enabled = !dangChay;
            btnChayDaChon.Enabled = !dangChay;
            btnDungLai.Enabled = dangChay;
            btnLuuVaoCSDL.Enabled = !dangChay && _ketQuaPhienHienTai.Count > 0;
            btnLuuBaoCao.Enabled = !dangChay && _ketQuaPhienHienTai.Count > 0;

            btnTaiTestCase.Enabled = !dangChay;
            cboBoSuuTap.Enabled = !dangChay;
            cboModule.Enabled = !dangChay;
            clbDanhSachTestCase.Enabled = !dangChay;
            txtBaseUrl.Enabled = !dangChay;
            btnLuuUrl.Enabled = !dangChay;
            numTimeout.Enabled = !dangChay;
            btnKiemTraSeed.Enabled = !dangChay;
        }

        private void CapNhatTrangThaiKhiKiemTraSeed(bool dangKiemTraSeed)
        {
            btnChayTatCa.Enabled = !dangKiemTraSeed && !_dangChay;
            btnChayDaChon.Enabled = !dangKiemTraSeed && !_dangChay;
            btnDungLai.Enabled = _dangChay;
            btnLuuVaoCSDL.Enabled = !dangKiemTraSeed && !_dangChay && _ketQuaPhienHienTai.Count > 0;
            btnLuuBaoCao.Enabled = !dangKiemTraSeed && !_dangChay && _ketQuaPhienHienTai.Count > 0;

            btnTaiTestCase.Enabled = !dangKiemTraSeed && !_dangChay;
            cboBoSuuTap.Enabled = !dangKiemTraSeed && !_dangChay;
            cboModule.Enabled = !dangKiemTraSeed && !_dangChay;
            clbDanhSachTestCase.Enabled = !dangKiemTraSeed && !_dangChay;
            txtBaseUrl.Enabled = !dangKiemTraSeed && !_dangChay;
            btnLuuUrl.Enabled = !dangKiemTraSeed && !_dangChay;
            numTimeout.Enabled = !dangKiemTraSeed && !_dangChay;
            btnKiemTraSeed.Enabled = !dangKiemTraSeed && !_dangChay;
        }

    }
}


