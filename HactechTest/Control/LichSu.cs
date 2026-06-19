using System.Windows.Forms;
using System.Drawing;
using HactechTest.ApiShopTesting.Core;
using HactechTest.Services.App;
using HactechTest.Services.History;
using HactechTest.Services.Reports;

namespace HactechTest.Control
{
    public partial class LichSu : UserControl
    {
        private bool _dangNapDanhSach;
        private CancellationTokenSource? _timKiemCts;
        private CancellationTokenSource? _chiTietCts;
        private readonly Dictionary<int, List<ChiTietPhienChayDaLuu>> _chiTietCache = [];

        public LichSu()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                btnClearHistory.Click += BtnClearHistory_Click;
                btnExportReport.Click += BtnExportReport_Click;
                txtSearch.TextChanged += TxtSearch_TextChanged;
                gridSessions.SelectionChanged += GridSessions_SelectionChanged;
            }
        }

        public Task NapDanhSachAsync(CancellationToken ct = default)
        {
            return NapDanhSachNoiBoAsync(ct);
        }

        private async Task NapDanhSachNoiBoAsync(CancellationToken ct)
        {
            var keyword = txtSearch.Text.Trim();
            _chiTietCts?.Cancel();
            var filteredSessions = await LaySessionsAsync(keyword, ct);
            _chiTietCache.Clear();

            _dangNapDanhSach = true;
            try
            {
                gridSessions.Rows.Clear();
                gridSessionDetail.Rows.Clear();
                foreach (var session in filteredSessions)
                {
                    var index = gridSessions.Rows.Add(
                        session.RunAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        session.NguoiChay,
                        session.Machine,
                        session.OperatingSystem,
                        session.TotalTests,
                        session.PassedTests,
                        session.FailedTests,
                        session.PassRate + "%",
                        session.AverageDurationMs);

                    gridSessions.Rows[index].Tag = session;
                }

                lblSelectedSession.Text = filteredSessions.Count == 0
                    ? "Không có dữ liệu lịch sử."
                    : "Chọn phiên chạy để xem chi tiết.";

                if (gridSessions.Rows.Count > 0)
                {
                    gridSessions.ClearSelection();
                    gridSessions.Rows[0].Selected = true;
                    gridSessions.CurrentCell = gridSessions.Rows[0].Cells[0];
                }
            }
            finally
            {
                _dangNapDanhSach = false;
            }

            if (gridSessions.Rows.Count > 0)
            {
                await HienThiChiTietPhienDangChonAsync(ct);
            }
        }

        private async void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            _timKiemCts?.Cancel();

            var cts = new CancellationTokenSource();
            _timKiemCts = cts;

            try
            {
                await Task.Delay(300, cts.Token);
                await NapDanhSachAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async void GridSessions_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dangNapDanhSach)
            {
                return;
            }

            await HienThiChiTietPhienDangChonAsync();
        }

        private async Task HienThiChiTietPhienDangChonAsync(CancellationToken ct = default)
        {
            if (LayPhienDangChon() is not { } session)
            {
                gridSessionDetail.Rows.Clear();
                return;
            }

            _chiTietCts?.Cancel();
            _chiTietCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var chiTietCt = _chiTietCts.Token;

            lblSelectedSession.Text =
                $"Phiên #{session.Id} - {session.RunAt:yyyy-MM-dd HH:mm:ss} - {session.NguoiChay} - đang tải chi tiết...";

            gridSessionDetail.Rows.Clear();

            List<ChiTietPhienChayDaLuu> chiTiet;
            try
            {
                chiTiet = await LayChiTietPhienAsync(session.Id, chiTietCt);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                lblSelectedSession.Text = $"Không tải được chi tiết phiên #{session.Id}: {ex.Message}";
                return;
            }

            if (LayPhienDangChon()?.Id != session.Id || chiTietCt.IsCancellationRequested)
            {
                return;
            }

            lblSelectedSession.Text =
                $"Phiên #{session.Id} - {session.RunAt:yyyy-MM-dd HH:mm:ss} - {session.NguoiChay} - {chiTiet.Count} test case";

            gridSessionDetail.SuspendLayout();
            try
            {
                gridSessionDetail.Rows.Clear();
                foreach (var detail in chiTiet)
                {
                    var index = gridSessionDetail.Rows.Add(
                        detail.SequenceNo,
                        detail.TestCaseName,
                        detail.Result,
                        detail.ActualStatus,
                        detail.DurationMs,
                        detail.Reason);
                    ApDungMauDongChiTiet(gridSessionDetail.Rows[index], detail.Result);
                }
            }
            finally
            {
                gridSessionDetail.ResumeLayout();
            }
        }

        private static void ApDungMauDongChiTiet(DataGridViewRow row, string ketQua)
        {
            var mauNen = Color.White;
            var mauChu = Color.FromArgb(24, 30, 37);
            var mauChon = Color.FromArgb(210, 225, 245);

            switch (ketQua.ToUpperInvariant())
            {
                case "PASS":
                    mauNen = Color.FromArgb(226, 246, 231);
                    mauChon = Color.FromArgb(181, 226, 193);
                    break;
                case "FAIL":
                    mauNen = Color.FromArgb(255, 225, 225);
                    mauChu = Color.FromArgb(132, 28, 28);
                    mauChon = Color.FromArgb(242, 174, 174);
                    break;
                case "ERROR":
                    mauNen = Color.FromArgb(255, 239, 213);
                    mauChu = Color.FromArgb(126, 74, 0);
                    mauChon = Color.FromArgb(246, 210, 158);
                    break;
                case "SKIP":
                    mauNen = Color.FromArgb(238, 240, 242);
                    mauChu = Color.FromArgb(88, 96, 105);
                    mauChon = Color.FromArgb(214, 218, 222);
                    break;
            }

            row.DefaultCellStyle.BackColor = mauNen;
            row.DefaultCellStyle.ForeColor = mauChu;
            row.DefaultCellStyle.SelectionBackColor = mauChon;
            row.DefaultCellStyle.SelectionForeColor = mauChu;
            row.Cells[2].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private PhienChayDaLuu? LayPhienDangChon()
        {
            if (gridSessions.CurrentRow?.Tag is PhienChayDaLuu currentSession)
            {
                return currentSession;
            }

            return gridSessions.SelectedRows.Count > 0 &&
                   gridSessions.SelectedRows[0].Tag is PhienChayDaLuu selectedSession
                ? selectedSession
                : null;
        }

        private async void BtnClearHistory_Click(object? sender, EventArgs e)
        {
            var session = LayPhienDangChon();
            if (session is null)
            {
                MessageBox.Show(
                    "Vui lòng chọn một phiên chạy trong bảng lịch sử trước khi xóa.",
                    "Chưa chọn phiên",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Xóa phiên chạy #{session.Id} lúc {session.RunAt:yyyy-MM-dd HH:mm:ss}?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                btnClearHistory.Enabled = false;
                await XoaPhienAsync(session.Id);
                _chiTietCache.Remove(session.Id);
                await NapDanhSachAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xóa được phiên lịch sử: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnClearHistory.Enabled = true;
            }
        }

        private async void BtnExportReport_Click(object? sender, EventArgs e)
        {
            var session = LayPhienDangChon();
            if (session is null)
            {
                MessageBox.Show(
                    "Vui lòng chọn một phiên chạy trong bảng lịch sử trước khi xuất báo cáo.",
                    "Chưa chọn phiên",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Xuất báo cáo phiên lịch sử",
                Filter = "HTML report (*.html)|*.html|JSON report (*.json)|*.json|Excel workbook (*.xlsx)|*.xlsx",
                FileName = $"bao-cao-api-shop-phien-{session.Id}-{session.RunAt:yyyyMMdd-HHmmss}.html",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            try
            {
                btnExportReport.Enabled = false;
                var chiTiet = await LayChiTietPhienAsync(session.Id, CancellationToken.None);
                var baoCaoService = new BaoCaoPhienChayService();
                var duLieuBaoCao = baoCaoService.TaoDuLieuBaoCaoTuDongKetQua(
                    TaoThongTinBaoCao(session),
                    TaoDongBaoCao(chiTiet));
                var ketQuaLuu = baoCaoService.LuuBaoCao(dialog.FileName, duLieuBaoCao);

                MessageBox.Show(
                    $"Đã xuất báo cáo phiên #{session.Id}:\n{ketQuaLuu.DuongDanHtml}\n{ketQuaLuu.DuongDanJson}\n{ketQuaLuu.DuongDanExcel}",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xuất được báo cáo phiên lịch sử: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExportReport.Enabled = true;
            }
        }

        private static async Task<List<PhienChayDaLuu>> LaySessionsAsync(string keyword, CancellationToken ct)
        {
            var store = TaoStore();
            return store is null ? [] : await store.LayDanhSachAsync(keyword, ct);
        }

        private async Task<List<ChiTietPhienChayDaLuu>> LayChiTietPhienAsync(int phienChayId, CancellationToken ct)
        {
            if (_chiTietCache.TryGetValue(phienChayId, out var chiTietDaCo))
            {
                return chiTietDaCo;
            }

            var store = TaoStore();
            if (store is null)
            {
                return [];
            }

            var chiTiet = await store.LayChiTietAsync(phienChayId, ct);
            _chiTietCache[phienChayId] = chiTiet;
            return chiTiet;
        }

        private static ThongTinBaoCaoPhienChay TaoThongTinBaoCao(PhienChayDaLuu session)
        {
            return new ThongTinBaoCaoPhienChay(
                new DateTimeOffset(session.RunAt),
                null,
                session.NguoiChay,
                session.Machine,
                session.OperatingSystem,
                session.BaseUrl,
                session.CheDoChay,
                session.CheDoLoi);
        }

        private static List<DongBaoCaoTestCase> TaoDongBaoCao(IReadOnlyList<ChiTietPhienChayDaLuu> chiTiet)
        {
            return chiTiet.Select((item, index) =>
            {
                var (ma, tenHienThi) = TachMaVaTenTestCase(item.TestCaseName);
                var trangThai = ChuyenTrangThai(item.Result);
                return new DongBaoCaoTestCase
                {
                    Stt = item.SequenceNo > 0 ? item.SequenceNo : index + 1,
                    Ma = ma,
                    Nhom = TaoNhomTuMa(ma),
                    TenHienThi = tenHienThi,
                    TrangThai = string.IsNullOrWhiteSpace(item.Result)
                        ? DinhDangKetQuaKiemThu.TrangThaiHienThi(trangThai)
                        : item.Result,
                    MaMongDoi = item.ExpectedStatus,
                    MaThucTe = item.ActualStatus,
                    HttpStatus = LayHttpStatus(item.ActualStatus),
                    ThoiGianMs = item.DurationMs,
                    Endpoint = TaoEndpoint(item),
                    RequestBodyJson = item.RequestBody,
                    ThongDiep = item.Reason,
                    ResponseRutGon = item.ResponseBody,
                    TrangThaiGoc = trangThai
                };
            }).ToList();
        }

        private static (string Ma, string TenHienThi) TachMaVaTenTestCase(string tenTestCase)
        {
            var parts = tenTestCase.Split(" - ", 2, StringSplitOptions.TrimEntries);
            return parts.Length == 2
                ? (parts[0], parts[1])
                : ("", tenTestCase);
        }

        private static string TaoNhomTuMa(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
            {
                return "Lịch sử";
            }

            var nhom = ma.Split('-', 2, StringSplitOptions.TrimEntries)[0];
            return string.IsNullOrWhiteSpace(nhom)
                ? "Lịch sử"
                : char.ToUpperInvariant(nhom[0]) + nhom[1..].ToLowerInvariant();
        }

        private static TrangThaiKetQua ChuyenTrangThai(string ketQua)
        {
            return ketQua.Trim().ToUpperInvariant() switch
            {
                "PASS" => TrangThaiKetQua.Dat,
                "SKIP" => TrangThaiKetQua.BoQua,
                "FAIL" => TrangThaiKetQua.ThatBai,
                _ => TrangThaiKetQua.LoiMoiTruong
            };
        }

        private static int? LayHttpStatus(string actualStatus)
        {
            var raw = actualStatus.Trim();
            if (raw.StartsWith("HTTP_", StringComparison.OrdinalIgnoreCase))
            {
                raw = raw[5..];
            }
            else if (raw.StartsWith("HTTP ", StringComparison.OrdinalIgnoreCase))
            {
                raw = raw[5..];
            }
            else
            {
                return null;
            }

            return int.TryParse(raw, out var status) ? status : null;
        }

        private static string? TaoEndpoint(ChiTietPhienChayDaLuu chiTiet)
        {
            if (string.IsNullOrWhiteSpace(chiTiet.HttpMethod))
            {
                return string.IsNullOrWhiteSpace(chiTiet.Url) ? null : chiTiet.Url;
            }

            return string.IsNullOrWhiteSpace(chiTiet.Url)
                ? chiTiet.HttpMethod
                : $"{chiTiet.HttpMethod} {chiTiet.Url}";
        }

        private static async Task XoaPhienAsync(int phienChayId)
        {
            var store = TaoStore();
            if (store is not null)
            {
                await store.XoaPhienAsync(phienChayId);
            }
        }

        private static PhienChayStore? TaoStore()
        {
            return AppHost.IsInitialized && AppHost.Instance.DatabaseSanSang
                ? new PhienChayStore(AppHost.Instance.ConnectionString)
                : null;
        }
    }
}

