using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using HactechTest.Models.History;
using HactechTest.Services.History;
using HactechTest.Services.Reports;

namespace HactechTest.Control
{
    public partial class LichSu : UserControl
    {
        private bool _dangNapDanhSach;
        private CancellationTokenSource? _timKiemCts;
        private CancellationTokenSource? _chiTietCts;
        private readonly Dictionary<int, List<ChiTietPhienChayDaLuu>> _chiTietTomTatCache = [];
        private readonly Dictionary<int, List<ChiTietPhienChayDaLuu>> _chiTietDayDuCache = [];
        private readonly LichSuService _lichSuService = new();

        public LichSu()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                BatDoubleBuffer(gridSessions);
                BatDoubleBuffer(gridSessionDetail);
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
            _chiTietTomTatCache.Clear();
            _chiTietDayDuCache.Clear();

            _dangNapDanhSach = true;
            try
            {
                gridSessions.SuspendLayout();
                gridSessionDetail.SuspendLayout();
                gridSessions.Rows.Clear();
                gridSessionDetail.Rows.Clear();
                if (filteredSessions.Count > 0)
                {
                    gridSessions.Rows.AddRange(filteredSessions.Select(TaoDongPhien).ToArray());
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
                gridSessionDetail.ResumeLayout();
                gridSessions.ResumeLayout();
                _dangNapDanhSach = false;
            }

            if (gridSessions.Rows.Count > 0)
            {
                TaiChiTietDangChonSauKhiVe(ct);
            }
        }

        private void TaiChiTietDangChonSauKhiVe(CancellationToken ct)
        {
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }

            BeginInvoke(new Action(async () =>
            {
                try
                {
                    await HienThiChiTietPhienDangChonAsync(ct);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    lblSelectedSession.Text = "KhÃ´ng táº£i Ä‘Æ°á»£c chi tiáº¿t lá»‹ch sá»­: " + ex.Message;
                }
            }));
        }

        private DataGridViewRow TaoDongPhien(PhienChayDaLuu session)
        {
            var row = new DataGridViewRow();
            row.CreateCells(
                gridSessions,
                session.RunAt.ToString("yyyy-MM-dd HH:mm:ss"),
                session.NguoiChay,
                session.Machine,
                session.OperatingSystem,
                session.TotalTests,
                session.PassedTests,
                session.FailedTests,
                session.PassRate + "%",
                session.AverageDurationMs);
            row.Tag = session;
            return row;
        }

        private static void BatDoubleBuffer(DataGridView grid)
        {
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(grid, true);
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
                chiTiet = await LayChiTietTomTatPhienAsync(session.Id, chiTietCt);
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
                if (chiTiet.Count > 0)
                {
                    gridSessionDetail.Rows.AddRange(chiTiet.Select(TaoDongChiTiet).ToArray());
                }
            }
            finally
            {
                gridSessionDetail.ResumeLayout();
            }
        }

        private DataGridViewRow TaoDongChiTiet(ChiTietPhienChayDaLuu detail)
        {
            var row = new DataGridViewRow();
            row.CreateCells(
                gridSessionDetail,
                detail.SequenceNo,
                detail.TestCaseName,
                detail.Result,
                detail.ActualStatus,
                detail.DurationMs,
                detail.Reason);
            ApDungMauDongChiTiet(row, detail.Result);
            return row;
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
                await _lichSuService.XoaPhienAsync(session.Id);
                _chiTietTomTatCache.Remove(session.Id);
                _chiTietDayDuCache.Remove(session.Id);
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
                var chiTiet = await LayChiTietDayDuPhienAsync(session.Id, CancellationToken.None);
                var duLieuBaoCao = _lichSuService.TaoDuLieuBaoCao(session, chiTiet);
                var ketQuaLuu = new BaoCaoPhienChayService().LuuBaoCao(dialog.FileName, duLieuBaoCao);

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

        private Task<List<PhienChayDaLuu>> LaySessionsAsync(string keyword, CancellationToken ct)
        {
            return _lichSuService.LayDanhSachAsync(keyword, ct);
        }

        private async Task<List<ChiTietPhienChayDaLuu>> LayChiTietTomTatPhienAsync(int phienChayId, CancellationToken ct)
        {
            if (_chiTietDayDuCache.TryGetValue(phienChayId, out var chiTietDayDu))
            {
                return chiTietDayDu;
            }

            if (_chiTietTomTatCache.TryGetValue(phienChayId, out var chiTietTomTat))
            {
                return chiTietTomTat;
            }

            var chiTiet = await _lichSuService.LayChiTietTomTatAsync(phienChayId, ct);
            _chiTietTomTatCache[phienChayId] = chiTiet;
            return chiTiet;
        }

        private async Task<List<ChiTietPhienChayDaLuu>> LayChiTietDayDuPhienAsync(int phienChayId, CancellationToken ct)
        {
            if (_chiTietDayDuCache.TryGetValue(phienChayId, out var chiTietDaCo))
            {
                return chiTietDaCo;
            }

            var chiTiet = await _lichSuService.LayChiTietAsync(phienChayId, ct);
            _chiTietDayDuCache[phienChayId] = chiTiet;
            _chiTietTomTatCache[phienChayId] = chiTiet;
            return chiTiet;
        }
    }
}

