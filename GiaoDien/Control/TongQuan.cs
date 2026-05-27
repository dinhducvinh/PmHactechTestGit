using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApiTest.Control
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

        public Task NapDuLieuAsync()
        {
            if (!_daKhoiTaoNguoiChay)
            {
                cboNguoiChay.Items.Clear();
                cboNguoiChay.Items.Add("(tat ca)");
                cboNguoiChay.Items.Add("an.nguyen");
                cboNguoiChay.Items.Add("minh.le");
                cboNguoiChay.Items.Add("demo.tester");
                cboNguoiChay.SelectedIndex = 0;
                _daKhoiTaoNguoiChay = true;
            }

            var tong = dtpNgayLoc.Checked ? 8 : 14;
            var nguoiChay = cboNguoiChay.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(nguoiChay) &&
                !string.Equals(nguoiChay, "(tat ca)", StringComparison.OrdinalIgnoreCase))
            {
                tong -= 3;
            }

            _tongTest = Math.Max(tong, 1);
            _tongKhongDat = Math.Max(1, _tongTest / 4);
            _tongDat = _tongTest - _tongKhongDat;
            _tyLeDat = (int)Math.Round(100d * _tongDat / _tongTest);

            lblSoTongTest.Text = _tongTest.ToString();
            lblSoDat.Text = _tongDat.ToString();
            lblSoKhongDat.Text = _tongKhongDat.ToString();
            lblSoTyLe.Text = _tyLeDat + "%";
            lblTyLeGiua.Text = _tyLeDat + "%";
            pnlVeDonut.Invalidate();

            return Task.CompletedTask;
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
