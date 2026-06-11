using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using HactechTest.Services.App;

namespace HactechTest.Control
{
    public partial class TongQuan : UserControl
    {
        private int _tongTest;
        private int _tongDat;
        private int _tongKhongDat;
        private int _tyLeDat;
        private bool _daKhoiTaoNguoiChay;

        public TongQuan()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                Load += TongQuan_Load;
                btnApDungLoc.Click += async (_, _) => await NapDuLieuAsync();
                btnDatLaiLoc.Click += BtnDatLaiLoc_Click;
                pnlVeDonut.Paint += PnlVeDonut_Paint;
                pnlVeDonut.Resize += (_, _) => pnlVeDonut.Invalidate();
            }
        }

        private async void TongQuan_Load(object? sender, EventArgs e)
        {
            await NapDuLieuAsync();
        }

        public async Task NapDuLieuAsync()
        {
            await NapNguoiChayAsync();

            if (!AppHost.IsInitialized || !AppHost.Instance.DatabaseSanSang)
            {
                GanSoLieu(0, 0, 0);
                return;
            }

            try
            {
                await using var conn = await AppHost.Instance.OpenConnectionAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = """
                    SELECT
                        ISNULL(SUM(tong_test), 0),
                        ISNULL(SUM(tong_dat), 0),
                        ISNULL(SUM(tong_khong_dat), 0)
                    FROM dbo.v_tong_quan
                    WHERE (@ngay IS NULL OR ngay_chay = @ngay)
                      AND (@nguoi_chay IS NULL OR nguoi_chay = @nguoi_chay);
                    """;

                var nguoiChay = cboNguoiChay.SelectedItem?.ToString();
                var locNguoiChay = !string.IsNullOrWhiteSpace(nguoiChay) &&
                    !string.Equals(nguoiChay, "(tat ca)", StringComparison.OrdinalIgnoreCase);

                cmd.Parameters.AddWithValue("@ngay", dtpNgayLoc.Checked ? (object)dtpNgayLoc.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@nguoi_chay", locNguoiChay ? (object)nguoiChay! : DBNull.Value);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    GanSoLieu(
                        Convert.ToInt32(reader.GetValue(0)),
                        Convert.ToInt32(reader.GetValue(1)),
                        Convert.ToInt32(reader.GetValue(2)));
                }
                else
                {
                    GanSoLieu(0, 0, 0);
                }
            }
            catch
            {
                GanSoLieu(0, 0, 0);
            }
        }

        private void GanSoLieu(int tong, int dat, int khongDat)
        {
            _tongTest = tong;
            _tongDat = dat;
            _tongKhongDat = khongDat;
            _tyLeDat = _tongTest == 0 ? 0 : (int)Math.Round(100d * _tongDat / _tongTest);
            lblSoTongTest.Text = _tongTest.ToString();
            lblSoDat.Text = _tongDat.ToString();
            lblSoKhongDat.Text = _tongKhongDat.ToString();
            lblSoTyLe.Text = _tyLeDat + "%";
            lblTyLeGiua.Text = _tyLeDat + "%";
            pnlVeDonut.Invalidate();
        }

        private async Task NapNguoiChayAsync()
        {
            if (_daKhoiTaoNguoiChay)
            {
                return;
            }

            cboNguoiChay.Items.Clear();
            cboNguoiChay.Items.Add("(tat ca)");

            if (AppHost.IsInitialized && AppHost.Instance.DatabaseSanSang)
            {
                try
                {
                    await using var conn = await AppHost.Instance.OpenConnectionAsync();
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = """
                        SELECT DISTINCT nguoi_chay
                        FROM dbo.v_tong_quan
                        WHERE nguoi_chay IS NOT NULL AND LTRIM(RTRIM(nguoi_chay)) <> N''
                        ORDER BY nguoi_chay;
                        """;
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        cboNguoiChay.Items.Add(reader.GetString(0));
                    }
                }
                catch
                {
                    // Giữ bộ lọc mặc định nếu chưa có bảng lịch sử hoặc chưa kết nối được DB.
                }
            }

            cboNguoiChay.SelectedIndex = 0;
            _daKhoiTaoNguoiChay = true;
        }

        private async void BtnDatLaiLoc_Click(object? sender, EventArgs e)
        {
            dtpNgayLoc.Checked = false;
            if (cboNguoiChay.Items.Count > 0)
            {
                cboNguoiChay.SelectedIndex = 0;
            }

            await NapDuLieuAsync();
        }

        private void PnlVeDonut_Paint(object? sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var width = pnlVeDonut.Width;
            var height = pnlVeDonut.Height;
            var size = Math.Min(width, height) - 16;
            if (size <= 0)
            {
                return;
            }

            var rect = new Rectangle((width - size) / 2, (height - size) / 2, size, size);
            var tong = _tongDat + _tongKhongDat;

            if (tong == 0)
            {
                using var pen = new Pen(Color.LightGray, 24);
                graphics.DrawEllipse(pen, rect);
                return;
            }

            var gocDat = 360f * _tongDat / tong;
            var gocKhongDat = 360f - gocDat;

            using (var brushDat = new SolidBrush(Color.FromArgb(40, 167, 69)))
            {
                graphics.FillPie(brushDat, rect, -90, gocDat);
            }

            using (var brushKhongDat = new SolidBrush(Color.FromArgb(220, 53, 69)))
            {
                graphics.FillPie(brushKhongDat, rect, -90 + gocDat, gocKhongDat);
            }

            var inner = new Rectangle(
                rect.X + size / 4,
                rect.Y + size / 4,
                size / 2,
                size / 2);

            graphics.FillEllipse(Brushes.White, inner);

            using var font = new Font("Segoe UI", 14, FontStyle.Bold);
            var text = _tyLeDat + "%";
            var textSize = graphics.MeasureString(text, font);
            graphics.DrawString(
                text,
                font,
                Brushes.Black,
                rect.X + (rect.Width - textSize.Width) / 2,
                rect.Y + (rect.Height - textSize.Height) / 2);
        }
    }
}

