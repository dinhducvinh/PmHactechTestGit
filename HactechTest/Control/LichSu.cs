using System.Windows.Forms;
using System.Drawing;
using HactechTest.Services.App;
using HactechTest.Services.History;

namespace HactechTest.Control
{
    public partial class LichSu : UserControl
    {
        private bool _dangNapDanhSach;

        public LichSu()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                Load += LichSu_Load;
                btnClearHistory.Click += BtnClearHistory_Click;
                txtSearch.TextChanged += async (_, _) => await NapDanhSachAsync();
                gridSessions.SelectionChanged += GridSessions_SelectionChanged;
                gridSessions.CellClick += GridSessions_CellClick;
            }
        }

        private async void LichSu_Load(object? sender, EventArgs e)
        {
            await NapDanhSachAsync();
        }

        public Task NapDanhSachAsync()
        {
            return NapDanhSachNoiBoAsync();
        }

        private async Task NapDanhSachNoiBoAsync()
        {
            var keyword = txtSearch.Text.Trim();
            var filteredSessions = await LaySessionsAsync(keyword);

            _dangNapDanhSach = true;
            gridSessions.Rows.Clear();
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
            _dangNapDanhSach = false;

            gridSessionDetail.Rows.Clear();
            lblSelectedSession.Text = filteredSessions.Count == 0
                ? "Không có dữ liệu lịch sử."
                : "Chọn phiên chạy để xem chi tiết.";

            if (gridSessions.Rows.Count > 0)
            {
                gridSessions.ClearSelection();
                gridSessions.Rows[0].Selected = true;
                gridSessions.CurrentCell = gridSessions.Rows[0].Cells[0];
                HienThiChiTietPhienDangChon();
            }
        }

        private void GridSessions_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dangNapDanhSach)
            {
                return;
            }

            HienThiChiTietPhienDangChon();
        }

        private void GridSessions_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            HienThiChiTietPhienDangChon();
        }

        private void HienThiChiTietPhienDangChon()
        {
            if (gridSessions.CurrentRow?.Tag is not PhienChayDaLuu session)
            {
                gridSessionDetail.Rows.Clear();
                return;
            }

            lblSelectedSession.Text =
                $"Phiên #{session.Id} - {session.RunAt:yyyy-MM-dd HH:mm:ss} - {session.NguoiChay} - {session.Details.Count} test case";

            gridSessionDetail.Rows.Clear();
            foreach (var detail in session.Details)
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

        private async void BtnClearHistory_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Xóa toàn bộ lịch sử chạy?",
                "Xac nhan",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                await XoaLichSuAsync();
                await NapDanhSachAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xóa được lịch sử: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<List<PhienChayDaLuu>> LaySessionsAsync(string keyword)
        {
            var store = TaoStore();
            return store is null ? [] : await store.LayDanhSachAsync(keyword);
        }

        private static async Task XoaLichSuAsync()
        {
            var store = TaoStore();
            if (store is not null)
            {
                await store.XoaTatCaAsync();
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

