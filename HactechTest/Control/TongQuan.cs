using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using HactechTest.Models.Dashboard;
using HactechTest.Services.Dashboard;

namespace HactechTest.Control
{
    public partial class TongQuan : UserControl
    {
        private int _tongTest;
        private int _tongDat;
        private int _tongKhongDat;
        private int _tyLeDat;
        private bool _daKhoiTaoNguoiChay;
        private readonly TongQuanService _tongQuanService = new();

        public TongQuan()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                btnApDungLoc.Click += async (_, _) => await NapDuLieuAsync();
                pnlVeDonut.Paint += PnlVeDonut_Paint;
                pnlVeDonut.Resize += (_, _) => pnlVeDonut.Invalidate();
                pnlBieuDoDonut.Resize += (_, _) => CapNhatViTriChuThich();
            }
        }

        public async Task NapDuLieuAsync()
        {
            await NapNguoiChayAsync();

            try
            {
                var boLoc = new BoLocTongQuan(
                    dtpNgayLoc.Value.Date,
                    TongQuanService.ChuanHoaNguoiChay(cboNguoiChay.SelectedItem?.ToString()));
                var thongKe = await _tongQuanService.LayThongKeAsync(boLoc);
                GanSoLieu(thongKe.TongTest, thongKe.TongDat, thongKe.TongKhongDat);
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
            lblChuThichDat.Text = $"Đạt: {_tongDat:N0}";
            lblChuThichKhongDat.Text = $"Không đạt: {_tongKhongDat:N0}";
            CapNhatViTriChuThich();
            pnlVeDonut.Invalidate();
        }

        private async Task NapNguoiChayAsync()
        {
            if (_daKhoiTaoNguoiChay)
            {
                return;
            }

            cboNguoiChay.Items.Clear();
            try
            {
                foreach (var nguoiChay in await _tongQuanService.LayNguoiChayAsync())
                {
                    cboNguoiChay.Items.Add(nguoiChay);
                }
            }
            catch
            {
                cboNguoiChay.Items.Add(TongQuanService.TatCaNguoiChay);
            }

            cboNguoiChay.SelectedIndex = 0;
            _daKhoiTaoNguoiChay = true;
        }

        private void CapNhatViTriChuThich()
        {
            const int khoangCach = 28;
            var tongRong = lblChuThichDat.Width + khoangCach + lblChuThichKhongDat.Width;
            var x = Math.Max(16, (pnlBieuDoDonut.ClientSize.Width - tongRong) / 2);
            var y = Math.Max(0, pnlBieuDoDonut.ClientSize.Height - 88);

            lblChuThichDat.Location = new Point(x, y);
            lblChuThichKhongDat.Location = new Point(x + lblChuThichDat.Width + khoangCach, y);
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

