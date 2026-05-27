using System.Diagnostics;
using System.Text;
using System.Text.Json;
using KiemThuApiShop.Core;
using KiemThuApiShop.KichBan;
using KiemThuApiShop.Seed;

namespace ApiTest.Control
{
    public partial class ChayTest : UserControl
    {
        private readonly IReadOnlyList<KichBanApi> _tatCaKichBan = BoKichBanApi.TaoTatCaKichBan();
        private readonly List<KichBanApi> _dsTestCaseHienThi = new();
        private readonly List<KetQuaChay> _ketQuaPhienHienTai = new();

        private CancellationTokenSource? _cts;
        private bool _dangChay;
        private string _cheDoChayPhienHienTai = "all";
        private string _cheDoLoiPhienHienTai = "continue_on_fail";
        private DateTimeOffset? _batDauLuc;
        private DateTimeOffset? _ketThucLuc;

        private CauHinhChay _cauHinhMacDinh = new();
        private TextBox? _txtBaseUrl;
        private TextBox? _txtConnectionString;
        private NumericUpDown? _numTimeout;
        private CheckBox? _chkTuDongChuanBi;
        private Button? _btnLuuBaoCao;

        public ChayTest()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                _cauHinhMacDinh = DocCauHinhShop();

                KhoiTaoSuKienKetQua();
                KhoiTaoCauHinhShop();
                GanPanelChiTietVaoKetQua();

                Load += ChayTest_Load;
                btnTaiTestCase.Click += BtnTaiTestCase_Click;
                cboBoSuuTap.SelectedIndexChanged += CboBoSuuTap_SelectedIndexChanged;
                cboModule.SelectedIndexChanged += (_, _) => BtnTaiTestCase_Click(null, EventArgs.Empty);
                btnChonTatCa.Click += (_, _) => ChonTatCaTrongList(true);
                btnBoChonTatCa.Click += (_, _) => ChonTatCaTrongList(false);
                btnChayTatCa.Click += async (_, _) => await ChayAsync(false);
                btnChayDaChon.Click += async (_, _) => await ChayAsync(true);
                btnDungLai.Click += (_, _) => _cts?.Cancel();
                btnLuuVaoCSDL.Click += async (_, _) => await BtnLuuVaoCSDL_ClickAsync();
                _btnLuuBaoCao!.Click += (_, _) => LuuBaoCao();

                lblTieuDeTrang.Text = "Chạy Test API Shop";
                btnLuuVaoCSDL.Text = "Lưu SQL seed";
                btnDungLai.Enabled = false;
                btnLuuVaoCSDL.Enabled = false;
                _btnLuuBaoCao.Enabled = false;
            }
        }

        private async void ChayTest_Load(object? sender, EventArgs e)
        {
            await NapDanhSachBoSuuTapAsync();
            BtnTaiTestCase_Click(null, EventArgs.Empty);
        }

        public async Task DatLaiBoLocAsync(int? idBoSuuTap, int? idMoDun)
        {
            await NapDanhSachBoSuuTapAsync();
            if (cboBoSuuTap.Items.Count > 0) cboBoSuuTap.SelectedIndex = 0;
            if (cboModule.Items.Count > 0) cboModule.SelectedIndex = 0;
            BtnTaiTestCase_Click(null, EventArgs.Empty);
        }

        // ----------------------------------------------------------------
        // Cấu hình chạy bộ testcase KiemThuApiShop
        // ----------------------------------------------------------------
        private void KhoiTaoCauHinhShop()
        {
            var panel = new Panel
            {
                Name = "pnlCauHinhShop",
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 104,
                Dock = DockStyle.Top
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2,
                Padding = new Padding(16, 12, 16, 12)
            };
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            _txtBaseUrl = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.75F),
                Margin = new Padding(0, 2, 16, 2),
                Text = _cauHinhMacDinh.BaseUrl
            };

            _numTimeout = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 3,
                Maximum = 300,
                Value = Math.Clamp(_cauHinhMacDinh.TimeoutGiay, 3, 300),
                Margin = new Padding(0, 2, 16, 2)
            };

            _chkTuDongChuanBi = new CheckBox
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Checked = _cauHinhMacDinh.TuDongChuanBiDuLieu,
                Text = "Tự động chuẩn bị dữ liệu seed trước khi chạy",
                Margin = new Padding(0)
            };

            _txtConnectionString = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 2, 0, 2),
                Text = _cauHinhMacDinh.ConnectionStrings.TryGetValue("ApiShopTestDb", out var cs)
                    ? cs
                    : "Server=.;Database=ApiShopTestDb;Trusted_Connection=True;TrustServerCertificate=True"
            };

            table.Controls.Add(TaoNhan("Restful API URL:"), 0, 0);
            table.Controls.Add(_txtBaseUrl, 1, 0);
            table.Controls.Add(TaoNhan("Timeout:"), 2, 0);
            table.Controls.Add(_numTimeout, 3, 0);
            table.Controls.Add(_chkTuDongChuanBi, 4, 0);
            table.SetColumnSpan(_chkTuDongChuanBi, 2);
            table.Controls.Add(TaoNhan("SQL Server seed:"), 0, 1);
            table.Controls.Add(_txtConnectionString, 1, 1);
            table.SetColumnSpan(_txtConnectionString, 5);

            panel.Controls.Add(table);
            pnlNoiDung.Controls.Add(panel);
            var headerIndex = pnlNoiDung.Controls.IndexOf(pnlDauTrang);
            if (headerIndex >= 0)
            {
                pnlNoiDung.Controls.SetChildIndex(panel, headerIndex);
            }

            _btnLuuBaoCao = new Button
            {
                BackColor = Color.FromArgb(108, 117, 125),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Margin = new Padding(8, 0, 0, 0),
                Size = new Size(150, 32),
                Text = "Lưu báo cáo"
            };
            _btnLuuBaoCao.FlatAppearance.BorderSize = 0;

            tblNutHanhDongLayout.Controls.Remove(btnLuuVaoCSDL);
            var rightActions = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Right,
                Margin = Padding.Empty,
                WrapContents = false
            };
            btnLuuVaoCSDL.Margin = Padding.Empty;
            rightActions.Controls.Add(btnLuuVaoCSDL);
            rightActions.Controls.Add(_btnLuuBaoCao);
            tblNutHanhDongLayout.Controls.Add(rightActions, 1, 0);
        }

        private static Label TaoNhan(string text)
        {
            return new Label
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(0, 4, 12, 0),
                Text = text
            };
        }

        private static CauHinhChay DocCauHinhShop()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "appsettings.shop.json"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "KiemThuApiShop", "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "KiemThuApiShop", "appsettings.json")
            };

            foreach (var path in candidates.Select(Path.GetFullPath))
            {
                if (File.Exists(path))
                {
                    try
                    {
                        return CauHinhChay.DocTuFile(path);
                    }
                    catch
                    {
                        // Nếu file cấu hình lỗi, dùng mặc định để app vẫn mở được.
                    }
                }
            }

            return new CauHinhChay();
        }

        private bool TaoCauHinhChay(out CauHinhChay cauHinh)
        {
            cauHinh = _cauHinhMacDinh;
            var baseUrl = (_txtBaseUrl?.Text ?? "").Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            {
                MessageBox.Show("Restful API URL phải là URL tuyệt đối, ví dụ http://localhost:8000.", "Thông báo");
                return false;
            }

            var connectionString = (_txtConnectionString?.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Connection string SQL Server seed không được rỗng.", "Thông báo");
                return false;
            }

            cauHinh = _cauHinhMacDinh with
            {
                BaseUrl = baseUrl,
                TimeoutGiay = (int)(_numTimeout?.Value ?? 60),
                TuDongChuanBiDuLieu = _chkTuDongChuanBi?.Checked ?? true,
                ConnectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["ApiShopTestDb"] = connectionString
                }
            };
            return true;
        }

        // ----------------------------------------------------------------
        // Nguồn test
        // ----------------------------------------------------------------
        private Task NapDanhSachBoSuuTapAsync()
        {
            cboBoSuuTap.Items.Clear();
            cboBoSuuTap.Items.Add("KiemThuApiShop");
            cboBoSuuTap.SelectedIndex = 0;

            cboModule.Items.Clear();
            cboModule.Items.Add("(Tất cả)");
            foreach (var nhom in _tatCaKichBan.Select(x => x.Nhom).Distinct().OrderBy(x => x))
            {
                cboModule.Items.Add(nhom);
            }
            cboModule.SelectedIndex = 0;

            return Task.CompletedTask;
        }

        private void CboBoSuuTap_SelectedIndexChanged(object? sender, EventArgs e)
        {
            BtnTaiTestCase_Click(null, EventArgs.Empty);
        }

        private void BtnTaiTestCase_Click(object? sender, EventArgs e)
        {
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
            _cheDoChayPhienHienTai = chiChayCaseDaChon ? "selected" : "all_sequential";
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
                var khoSeed = new KhoDuLieuSeedSqlServer(cauHinh.ChuoiKetNoiSqlServer, cauHinh.SoTaiKhoanSeed);
                await khoSeed.TaiAsync();

                using var mayKhach = new MayKhachApi(cauHinh.BaseUrl, TimeSpan.FromSeconds(cauHinh.TimeoutGiay));
                var nguCanh = new NguCanhKiemThu(cauHinh, mayKhach, khoSeed);

                if (cauHinh.TuDongChuanBiDuLieu)
                {
                    lblTrangThaiChay.Text = "Đang chuẩn bị dữ liệu test...";
                    await new ChuanBiDuLieuMoi(nguCanh).ChuanBiAsync();
                }

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

                    if (rdoDungKhiLoi.Checked && LaTrangThaiKhongDat(ketQua.TrangThai))
                    {
                        lblTrangThaiChay.Text = $"Đã dừng tại testcase failed: {ketQua.Ma}";
                        break;
                    }
                }

                if (_cts.IsCancellationRequested)
                {
                    lblTrangThaiChay.Text = "Đã dừng.";
                }
                else if (rdoDungKhiLoi.Checked && _ketQuaPhienHienTai.Any(x => LaTrangThaiKhongDat(x.TrangThai)))
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

                yeuCau = await kichBan.TaoYeuCauAsync(nguCanh);
                cancellationToken.ThrowIfCancellationRequested();
                var response = await nguCanh.Api.GuiAsync(yeuCau, cancellationToken);
                dongHo.Stop();

                if (!kichBan.MaChapNhan.Contains(response.MaSoSanh))
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
                ResponseRutGon = TienIchJson.RutGon(response?.NoiDungRaw, 2000)
            };
        }

        private void ThemDongKetQua(KetQuaChay r)
        {
            var index = dgvKetQuaChay.Rows.Add(
                _ketQuaPhienHienTai.Count,
                r.TenHienThi,
                HienThiTrangThai(r.TrangThai),
                r.MaThucTe ?? (r.HttpStatus.HasValue ? "HTTP " + r.HttpStatus.Value : "-"),
                r.MaMongDoi ?? "-",
                (int)Math.Round(r.ThoiGian.TotalMilliseconds),
                r.ThongDiep);

            var row = dgvKetQuaChay.Rows[index];
            row.Tag = r;
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
                var khoSeed = new KhoDuLieuSeedSqlServer(cauHinh.ChuoiKetNoiSqlServer, cauHinh.SoTaiKhoanSeed);
                var soDong = await khoSeed.GhiKetQuaTestCaseAsync(_ketQuaPhienHienTai);
                MessageBox.Show($"Đã lưu {soDong} dòng vào dbo.ketqua_testcase.", "Thành công",
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
                Filter = "HTML report (*.html)|*.html|JSON report (*.json)|*.json",
                FileName = $"bao-cao-api-shop-{DateTime.Now:yyyyMMdd-HHmmss}.html",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            var folder = Path.GetDirectoryName(dialog.FileName) ?? Environment.CurrentDirectory;
            var name = Path.GetFileNameWithoutExtension(dialog.FileName);
            var htmlPath = Path.Combine(folder, name + ".html");
            var jsonPath = Path.Combine(folder, name + ".json");
            var baoCao = TaoDuLieuBaoCao();

            File.WriteAllText(jsonPath, JsonSerializer.Serialize(baoCao, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }), Encoding.UTF8);
            File.WriteAllText(htmlPath, TaoHtmlBaoCao(), Encoding.UTF8);

            MessageBox.Show($"Đã lưu báo cáo:\n{htmlPath}\n{jsonPath}", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private object TaoDuLieuBaoCao()
        {
            var tong = _ketQuaPhienHienTai.Count;
            var dat = _ketQuaPhienHienTai.Count(x => x.TrangThai == TrangThaiKetQua.Dat);
            return new
            {
                thoiDiemChay = _batDauLuc?.ToString("yyyy-MM-dd HH:mm:ss"),
                thoiDiemKetThuc = _ketThucLuc?.ToString("yyyy-MM-dd HH:mm:ss"),
                nguoiThucHien = Environment.UserName,
                mayChay = Environment.MachineName,
                heDieuHanh = Environment.OSVersion.VersionString,
                baseUrl = _txtBaseUrl?.Text,
                cheDoChay = _cheDoChayPhienHienTai,
                cheDoLoi = _cheDoLoiPhienHienTai,
                tong,
                dat,
                khongDat = tong - dat,
                tyLeDat = tong == 0 ? 0 : Math.Round(100m * dat / tong, 2),
                ketQua = _ketQuaPhienHienTai.Select((x, i) => new
                {
                    stt = i + 1,
                    x.Ma,
                    x.Nhom,
                    x.TenHienThi,
                    trangThai = x.TrangThai.ToString(),
                    x.MaMongDoi,
                    x.MaThucTe,
                    x.HttpStatus,
                    thoiGianMs = (int)Math.Round(x.ThoiGian.TotalMilliseconds),
                    x.Endpoint,
                    x.ThongDiep,
                    x.ResponseRutGon
                }).ToList()
            };
        }

        private string TaoHtmlBaoCao()
        {
            static string E(string? s) => System.Net.WebUtility.HtmlEncode(s ?? "");
            var tong = _ketQuaPhienHienTai.Count;
            var dat = _ketQuaPhienHienTai.Count(x => x.TrangThai == TrangThaiKetQua.Dat);
            var tyLe = tong == 0 ? 0 : Math.Round(100m * dat / tong, 2);

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html><html lang=\"vi\"><head><meta charset=\"utf-8\"><title>Báo cáo API Shop</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:24px;background:#f4f5f7;color:#1f2933}section{background:#fff;border:1px solid #d9dee5;padding:16px;margin-bottom:16px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #d9dee5;padding:8px;text-align:left;vertical-align:top}th{background:#eef1f5}.pass{color:#198754;font-weight:700}.fail{color:#dc3545;font-weight:700}.meta{display:grid;grid-template-columns:180px 1fr;gap:6px 12px}</style>");
            sb.AppendLine("</head><body><section><h1>Báo cáo kiểm thử API Shop</h1><div class=\"meta\">");
            sb.AppendLine($"<strong>Thời điểm chạy</strong><span>{E(_batDauLuc?.ToString("yyyy-MM-dd HH:mm:ss"))}</span>");
            sb.AppendLine($"<strong>Người thực hiện</strong><span>{E(Environment.UserName)}</span>");
            sb.AppendLine($"<strong>Máy chạy</strong><span>{E(Environment.MachineName)}</span>");
            sb.AppendLine($"<strong>OS</strong><span>{E(Environment.OSVersion.VersionString)}</span>");
            sb.AppendLine($"<strong>Base URL</strong><span>{E(_txtBaseUrl?.Text)}</span>");
            sb.AppendLine($"<strong>Tổng kết</strong><span>Tổng: {tong}, Đạt: {dat}, Không đạt: {tong - dat}, Tỷ lệ: {tyLe}%</span>");
            sb.AppendLine("</div></section><section><h2>Chi tiết testcase</h2><table><thead><tr><th>STT</th><th>Mã</th><th>Nhóm</th><th>Tên</th><th>Kết quả</th><th>Mong đợi</th><th>Thực tế</th><th>HTTP</th><th>ms</th><th>Endpoint</th><th>Thông điệp</th></tr></thead><tbody>");

            for (var i = 0; i < _ketQuaPhienHienTai.Count; i++)
            {
                var item = _ketQuaPhienHienTai[i];
                var cls = item.TrangThai == TrangThaiKetQua.Dat ? "pass" : "fail";
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{i + 1}</td><td>{E(item.Ma)}</td><td>{E(item.Nhom)}</td><td>{E(item.TenHienThi)}</td>");
                sb.AppendLine($"<td class=\"{cls}\">{E(HienThiTrangThai(item.TrangThai))}</td><td>{E(item.MaMongDoi)}</td><td>{E(item.MaThucTe)}</td>");
                sb.AppendLine($"<td>{item.HttpStatus?.ToString() ?? ""}</td><td>{(int)Math.Round(item.ThoiGian.TotalMilliseconds)}</td><td>{E(item.Endpoint)}</td><td>{E(item.ThongDiep)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table></section></body></html>");
            return sb.ToString();
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

        private void GanPanelChiTietVaoKetQua()
        {
            if (!splitChayTest.Panel2.Controls.Contains(pnlChiTietResponse))
            {
                splitChayTest.Panel2.Controls.Add(pnlChiTietResponse);
                pnlChiTietResponse.BringToFront();
            }
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

            using var formChiTiet = new ApiTest.FormChiTietKetQuaTestCase(TaoChiTietKetQuaTest(r));
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
            rtbResponseBody.Text = "(Response body sẽ hiển thị sau khi chạy test)";
        }

        private void HienThiMoTaTestCase(KichBanApi tc)
        {
            lblChiTietCase.Text = $"{tc.Ma} - {tc.TenHienThi}";
            lblChiTietMeta.Text = $"Nhóm: {tc.Nhom} | Mã chấp nhận: {string.Join(", ", tc.MaChapNhan)}";
            rtbResponseBody.Text = tc.MoTa;
        }

        private void HienThiChiTietKetQua(KetQuaChay r)
        {
            lblChiTietCase.Text = $"{r.Ma} - {r.TenHienThi}";
            lblChiTietMeta.Text =
                $"{r.Endpoint ?? "-"} | expected: {r.MaMongDoi ?? "-"} | actual: {r.MaThucTe ?? "-"} | HTTP: {r.HttpStatus?.ToString() ?? "-"} | {(int)Math.Round(r.ThoiGian.TotalMilliseconds)} ms | {HienThiTrangThai(r.TrangThai)}";
            rtbResponseBody.Text =
                $"{r.ThongDiep}{Environment.NewLine}{Environment.NewLine}{r.ResponseRutGon ?? "(rỗng)"}";
        }

        private static ApiTest.TestCaseResultDetailData TaoChiTietKetQuaTest(KetQuaChay r)
        {
            return new ApiTest.TestCaseResultDetailData
            {
                SequenceNo = "",
                TestCaseName = r.TenHienThi,
                Result = HienThiTrangThai(r.TrangThai),
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
            if (_btnLuuBaoCao != null)
            {
                _btnLuuBaoCao.Enabled = !dangChay && _ketQuaPhienHienTai.Count > 0;
            }

            btnTaiTestCase.Enabled = !dangChay;
            cboBoSuuTap.Enabled = !dangChay;
            cboModule.Enabled = !dangChay;
            clbDanhSachTestCase.Enabled = !dangChay;
            _txtBaseUrl!.Enabled = !dangChay;
            _txtConnectionString!.Enabled = !dangChay;
            _numTimeout!.Enabled = !dangChay;
            _chkTuDongChuanBi!.Enabled = !dangChay;
        }

        private static bool LaTrangThaiKhongDat(TrangThaiKetQua trangThai)
        {
            return trangThai is TrangThaiKetQua.ThatBai or TrangThaiKetQua.LoiChuanBi or TrangThaiKetQua.LoiMoiTruong;
        }

        private static string HienThiTrangThai(TrangThaiKetQua trangThai)
        {
            return trangThai switch
            {
                TrangThaiKetQua.Dat => "PASS",
                TrangThaiKetQua.ThatBai => "FAIL",
                TrangThaiKetQua.BoQua => "SKIP",
                TrangThaiKetQua.LoiChuanBi => "SETUP ERROR",
                TrangThaiKetQua.LoiMoiTruong => "ENV ERROR",
                _ => trangThai.ToString()
            };
        }
    }
}
