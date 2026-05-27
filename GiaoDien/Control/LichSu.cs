using System.Linq;
using System.Windows.Forms;

namespace ApiTest.Control
{
    public partial class LichSu : UserControl
    {
        private readonly List<DemoSession> _sessions = TaoDuLieuMau();

        public LichSu()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                Load += LichSu_Load;
                btnClearHistory.Click += BtnClearHistory_Click;
                txtSearch.TextChanged += async (_, _) => await NapDanhSachAsync();
                gridSessions.SelectionChanged += GridSessions_SelectionChanged;
                gridSessions.AutoGenerateColumns = false;
                gridSessionDetail.AutoGenerateColumns = false;
            }
        }

        private async void LichSu_Load(object? sender, EventArgs e)
        {
            await NapDanhSachAsync();
        }

        public Task NapDanhSachAsync()
        {
            var keyword = txtSearch.Text.Trim();
            var filteredSessions = string.IsNullOrWhiteSpace(keyword)
                ? _sessions
                : _sessions.Where(session =>
                    session.Runner.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    session.Machine.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    session.Note.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            gridSessions.Rows.Clear();
            foreach (var session in filteredSessions)
            {
                var index = gridSessions.Rows.Add(
                    session.RunAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    session.Runner,
                    session.Machine,
                    session.OperatingSystem,
                    session.TotalTests,
                    session.PassedTests,
                    session.FailedTests,
                    session.PassRate + "%",
                    session.AverageDurationMs);

                gridSessions.Rows[index].Tag = session;
            }

            gridSessionDetail.Rows.Clear();
            lblSelectedSession.Text = filteredSessions.Count == 0
                ? "Khong co du lieu lich su."
                : "Chon phien chay de xem chi tiet.";

            return Task.CompletedTask;
        }

        private void GridSessions_SelectionChanged(object? sender, EventArgs e)
        {
            if (gridSessions.CurrentRow?.Tag is not DemoSession session)
            {
                return;
            }

            lblSelectedSession.Text =
                $"Phien #{session.Id} - {session.RunAt:yyyy-MM-dd HH:mm:ss} - {session.Runner}";

            gridSessionDetail.Rows.Clear();
            foreach (var detail in session.Details)
            {
                gridSessionDetail.Rows.Add(
                    detail.SequenceNo,
                    detail.TestCaseName,
                    detail.Result,
                    detail.ActualStatus,
                    detail.DurationMs,
                    detail.Reason);
            }
        }

        private async void BtnClearHistory_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Xoa toan bo lich su mau?",
                "Xac nhan",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            _sessions.Clear();
            await NapDanhSachAsync();
        }

        private static List<DemoSession> TaoDuLieuMau()
        {
            return new List<DemoSession>
            {
                new(
                    1,
                    DateTime.Now.AddHours(-2),
                    "an.nguyen",
                    "DESKTOP-01",
                    "Windows 11",
                    3,
                    2,
                    1,
                    67,
                    210,
                    "Website Demo",
                    new List<DemoSessionDetail>
                    {
                        new(1, "Dang nhap thanh cong", "PASS", 200, 180, "Response hop le"),
                        new(2, "Lay thong tin nguoi dung", "PASS", 200, 220, "Dung schema"),
                        new(3, "Tao don hang", "FAIL", 500, 230, "Mock loi server")
                    }),
                new(
                    2,
                    DateTime.Now.AddDays(-1),
                    "minh.le",
                    "LAPTOP-02",
                    "Windows 11",
                    2,
                    2,
                    0,
                    100,
                    150,
                    "Mobile Demo",
                    new List<DemoSessionDetail>
                    {
                        new(1, "Cap nhat avatar", "PASS", 200, 140, "Upload thanh cong"),
                        new(2, "Lay profile", "PASS", 200, 160, "Du lieu day du")
                    })
            };
        }

        private sealed record DemoSession(
            int Id,
            DateTime RunAt,
            string Runner,
            string Machine,
            string OperatingSystem,
            int TotalTests,
            int PassedTests,
            int FailedTests,
            int PassRate,
            int AverageDurationMs,
            string Note,
            List<DemoSessionDetail> Details);

        private sealed record DemoSessionDetail(
            int SequenceNo,
            string TestCaseName,
            string Result,
            int ActualStatus,
            int DurationMs,
            string Reason);
    }
}
