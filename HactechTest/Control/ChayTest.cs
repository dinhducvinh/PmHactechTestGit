using System.Drawing;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;
using HactechTest.Services.App;
using HactechTest.Services.ChayTest;
using HactechTest.Services.Configuration;
using HactechTest.Services.History;
using HactechTest.Services.Reports;

namespace HactechTest.Control
{
    public partial class ChayTest : UserControl
    {
        private readonly DichVuDanhSachKichBan _dichVuDanhSachKichBan = new();
        private readonly DichVuChayTestApi _dichVuChayTest = new();
        private readonly List<KichBanApi> _tatCaKichBan = new();
        private readonly List<KichBanApi> _dsTestCaseHienThi = new();
        private readonly List<KetQuaChay> _ketQuaPhienHienTai = new();

        private CancellationTokenSource? _cts;
        private bool _dangChay;
        private bool _dangSuaBaseUrl;
        private bool _dangNapBoLoc;
        private bool _daNapBoLocLanDau;
        private string _cheDoChayPhienHienTai = "all";
        private string _cheDoLoiPhienHienTai = "continue_on_fail";
        private DateTimeOffset? _batDauLuc;
        private DateTimeOffset? _ketThucLuc;

        public ChayTest()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                KhoiTaoSuKienKetQua();
                NapCauHinhUngDungLenGiaoDien();

                btnTaiTestCase.Click += async (_, _) => await TaiTestCaseTheoBoLocAsync();
                cboBoSuuTap.SelectedIndexChanged += CboBoSuuTap_SelectedIndexChanged;
                cboModule.SelectedIndexChanged += async (_, _) => await TaiTestCaseTheoBoLocAsync();
                btnChonTatCa.Click += (_, _) => ChonTatCaTrongList(true);
                btnBoChonTatCa.Click += (_, _) => ChonTatCaTrongList(false);
                btnChayTatCa.Click += async (_, _) => await ChayAsync(false);
                btnChayDaChon.Click += async (_, _) => await ChayAsync(true);
                btnDungLai.Click += (_, _) => _cts?.Cancel();
                btnSuaUrl.Click += (_, _) => BatDauSuaBaseUrl();
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

        public async Task DatLaiBoLocAsync(bool batBuocNapLai = false)
        {
            if (!_daNapBoLocLanDau || batBuocNapLai)
            {
                await NapDanhSachBoSuuTapAsync();
                if (cboBoSuuTap.Items.Count > 0) cboBoSuuTap.SelectedIndex = 0;
                if (cboModule.Items.Count > 0) cboModule.SelectedIndex = 0;
                _daNapBoLocLanDau = true;
                await TaiTestCaseTheoBoLocAsync(napLaiNguon: false);
                return;
            }

            if (_dsTestCaseHienThi.Count == 0)
            {
                await TaiTestCaseTheoBoLocAsync();
            }
        }

        // ----------------------------------------------------------------
        // Cấu hình chạy bộ testcase API Shop
        // ----------------------------------------------------------------
        private void NapCauHinhUngDungLenGiaoDien()
        {
            var cauHinh = CauHinhUngDung.Instance;
            txtBaseUrl.Text = cauHinh.BaseUrl;
            numTimeout.Value = Math.Clamp(cauHinh.TimeoutGiay, 3, 300);
            _dangSuaBaseUrl = false;
            CapNhatTrangThaiBaseUrl(true);
        }

        private bool TaoCauHinhChay(out CauHinhChay cauHinh)
        {
            cauHinh = new CauHinhChay();
            if (_dangSuaBaseUrl)
            {
                MessageBox.Show("Vui lòng bấm Lưu URL trước khi chạy test hoặc kiểm tra seed.", "Thông báo");
                return false;
            }

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

            cauHinh = new CauHinhChay
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
            if (!_dangSuaBaseUrl)
            {
                return;
            }

            if (!LayBaseUrlHopLe(out var baseUrl))
            {
                return;
            }

            CauHinhUngDung.LuuBaseUrlApi(baseUrl);
            txtBaseUrl.Text = baseUrl;
            _dangSuaBaseUrl = false;
            CapNhatTrangThaiBaseUrl(!_dangChay);

            MessageBox.Show("Đã lưu Restful API URL. Lần sau mở phần mềm sẽ tự điền URL này.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BatDauSuaBaseUrl()
        {
            _dangSuaBaseUrl = true;
            CapNhatTrangThaiBaseUrl(!_dangChay);
            txtBaseUrl.Focus();
            txtBaseUrl.SelectAll();
        }

        private void CapNhatTrangThaiBaseUrl(bool choPhepThaoTac)
        {
            txtBaseUrl.Enabled = choPhepThaoTac;
            txtBaseUrl.ReadOnly = !_dangSuaBaseUrl;
            txtBaseUrl.BackColor = _dangSuaBaseUrl
                ? Color.White
                : Color.FromArgb(248, 249, 250);

            btnSuaUrl.Enabled = choPhepThaoTac && !_dangSuaBaseUrl;
            btnLuuUrl.Enabled = choPhepThaoTac && _dangSuaBaseUrl;
        }

        private async Task BtnKiemTraSeed_ClickAsync()
        {
            if (!TaoCauHinhChay(out var cauHinh))
            {
                return;
            }

            using var formThongBao = new FormThongBaoChuanBiSeed();

            KetQuaChuanBiDuLieuSeed ketQua;
            try
            {
                CapNhatTrangThaiKhiKiemTraSeed(true);
                var formCha = FindForm();
                if (formCha != null) formThongBao.Show(formCha);
                else formThongBao.Show();

                formThongBao.CapNhatTrangThai("Đang quét database seed theo cấu hình hiện tại...");
                ketQua = await ChuanBiSeed.KiemTraVaChuanBiAsync(cauHinh, formThongBao.CapNhatTrangThai);
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
                ChuanBiSeed.TaoNoiDungThongBao(ketQua),
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

        private async Task TaiTestCaseTheoBoLocAsync(bool napLaiNguon = true)
        {
            if (_dangNapBoLoc)
            {
                return;
            }

            if (napLaiNguon)
            {
                await NapTatCaKichBanAsync();
            }

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

            try
            {
                var danhSach = await _dichVuDanhSachKichBan.LayTatCaAsync(CauHinhUngDung.Instance.ConnectionString);
                _tatCaKichBan.AddRange(danhSach);
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
                var yeuCauChay = new YeuCauChayTestApi(
                    cauHinh,
                    dsCanChay,
                    rdoDungKhiLoi.Checked);
                var tienTrinh = new TienTrinhGiaoDien(this, CapNhatTienTrinhChay);
                var ketQuaPhien = await _dichVuChayTest.ChayAsync(yeuCauChay, tienTrinh, _cts.Token);

                _batDauLuc = ketQuaPhien.BatDauLuc;
                _ketThucLuc = ketQuaPhien.KetThucLuc;

                if (ketQuaPhien.BiHuy)
                {
                    lblTrangThaiChay.Text = "Đã hủy.";
                }
                else if (ketQuaPhien.DungDoLoi)
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
                _ketThucLuc ??= DateTimeOffset.Now;
                _dangChay = false;
                _cts?.Dispose();
                _cts = null;
                CapNhatTrangThaiNut(false);
                CapNhatTongKet();
            }
        }

        private void CapNhatTienTrinhChay(TienTrinhChayTestApi tienTrinh)
        {
            lblTrangThaiChay.Text = tienTrinh.TrangThai;
            if (tienTrinh.KetQua is null)
            {
                return;
            }

            _ketQuaPhienHienTai.Add(tienTrinh.KetQua);
            ThemDongKetQua(tienTrinh.KetQua);
            HienThiChiTietKetQua(tienTrinh.KetQua);
            prgTienTrinh.Value = Math.Min(prgTienTrinh.Maximum, tienTrinh.DangChayThu);
            CapNhatTongKet();
        }

        private sealed class TienTrinhGiaoDien : IProgress<TienTrinhChayTestApi>
        {
            private readonly System.Windows.Forms.Control _control;
            private readonly Action<TienTrinhChayTestApi> _capNhat;

            public TienTrinhGiaoDien(
                System.Windows.Forms.Control control,
                Action<TienTrinhChayTestApi> capNhat)
            {
                _control = control;
                _capNhat = capNhat;
            }

            public void Report(TienTrinhChayTestApi value)
            {
                if (_control.IsDisposed)
                {
                    return;
                }

                if (_control.InvokeRequired)
                {
                    _control.Invoke(() => _capNhat(value));
                    return;
                }

                _capNhat(value);
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
                var store = new PhienChayStore(cauHinh.ChuoiKetNoiSqlServer);
                var ketQuaLuu = _ketQuaPhienHienTai
                    .Select((item, index) => BoChuyenDoiKetQuaChayTest.TaoKetQuaLuuPhien(
                        item,
                        index + 1,
                        cauHinh.BaseUrl))
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
                Filter = "HTML report (*.html)|*.html|JSON report (*.json)|*.json|Excel workbook (*.xlsx)|*.xlsx",
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

            MessageBox.Show($"Đã lưu báo cáo:\n{ketQuaLuu.DuongDanHtml}\n{ketQuaLuu.DuongDanJson}\n{ketQuaLuu.DuongDanExcel}", "Thành công",
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

            using var formChiTiet = new HactechTest.FormChiTietKetQuaTestCase(
                BoChuyenDoiKetQuaChayTest.TaoChiTietKetQuaTest(r));
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
            CapNhatTrangThaiBaseUrl(!dangChay);
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
            CapNhatTrangThaiBaseUrl(!dangKiemTraSeed && !_dangChay);
            numTimeout.Enabled = !dangKiemTraSeed && !_dangChay;
            btnKiemTraSeed.Enabled = !dangKiemTraSeed && !_dangChay;
        }

    }
}



