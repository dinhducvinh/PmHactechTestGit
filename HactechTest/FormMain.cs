using HactechTest.Control;
using HactechTest.Services.App;
namespace HactechTest
{
    public partial class FormMain : Form
    {
        private static readonly Color DefaultButtonColor = Color.FromArgb(35, 49, 64);
        private static readonly Color ActiveButtonColor = Color.FromArgb(49, 65, 82);

        public bool DaDangXuat { get; private set; }

        public FormMain()
        {
            InitializeComponent();
            KhoiTaoDieuHuong();
            CapNhatThongTinDangNhap();
            Shown += FormMain_Shown;
            FormClosing += FormMain_FormClosing;
        }

        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            Shown -= FormMain_Shown;
            await MoTongQuanAsync();
            await tongQuan1.NapDuLieuAsync();
        }

        private void KhoiTaoDieuHuong()
        {
            btnTongQuan.Click += async (_, _) => await MoTongQuanAsync();
            btnChayTest.Click += async (_, _) => await MoChayTestAsync();
            btnBoSuuTap.Click += async (_, _) => await MoBoSuuTapAsync();
            btnLichSu.Click += async (_, _) => await MoLichSuAsync();
            btnQuanLyNhanSu.Click += async (_, _) => await MoQuanLyNhanSuAsync();
            btnCaiDatTaiKhoan.Click += BtnCaiDatTaiKhoan_Click;
            btnDangXuat.Click += BtnDangXuat_Click;

            boSuuTap1.OpenRunTestRequested += async (_, _) =>
            {
                await chayTest1.DatLaiBoLocAsync();
                await MoManHinhAsync(chayTest1, btnChayTest, "Chay test");
            };
        }

        private Task MoTongQuanAsync()
        {
            return MoManHinhAsync(tongQuan1, btnTongQuan, "Tong quan", tongQuan1.NapDuLieuAsync);
        }

        private Task MoChayTestAsync()
        {
            return MoManHinhAsync(
                chayTest1,
                btnChayTest,
                "Chay test",
                chayTest1.DatLaiBoLocAsync);
        }

        private Task MoBoSuuTapAsync()
        {
            return MoManHinhAsync(boSuuTap1, btnBoSuuTap, "Bo suu tap", boSuuTap1.NapCayAsync);
        }

        private Task MoLichSuAsync()
        {
            return MoManHinhAsync(lichSu1, btnLichSu, "Lich su", lichSu1.NapDanhSachAsync);
        }

        private Task MoQuanLyNhanSuAsync()
        {
            if (!AppHost.Instance.LaAdmin)
            {
                MessageBox.Show(
                    "Chỉ tài khoản admin được vào màn hình quản lý nhân sự.",
                    "HACTECH TEST",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return Task.CompletedTask;
            }

            return MoManHinhAsync(
                quanLyNhanSu1,
                btnQuanLyNhanSu,
                "Quan ly nhan su",
                quanLyNhanSu1.NapDanhSachAsync);
        }

        private async Task MoManHinhAsync(System.Windows.Forms.Control manHinh, Button nutDangChon, string tieuDe,
            Func<Task>? taiDuLieu = null)
        {
            if (taiDuLieu != null)
            {
                await taiDuLieu();
            }

            manHinh.Visible = true;
            manHinh.BringToFront();

            btnTongQuan.BackColor = DefaultButtonColor;
            btnChayTest.BackColor = DefaultButtonColor;
            btnBoSuuTap.BackColor = DefaultButtonColor;
            btnLichSu.BackColor = DefaultButtonColor;
            btnQuanLyNhanSu.BackColor = DefaultButtonColor;
            nutDangChon.BackColor = ActiveButtonColor;

            pnlVienActive.Height = nutDangChon.Height;
            pnlVienActive.Top = nutDangChon.Top;
            pnlVienActive.BringToFront();

            Text = $"HACTECH TEST - {tieuDe}";
        }

        private void CapNhatThongTinDangNhap()
        {
            var taiKhoan = AppHost.IsInitialized ? AppHost.Instance.TaiKhoanDangNhap : null;
            btnCaiDatTaiKhoan.Text = taiKhoan is null
                ? "⚙  Tài khoản"
                : $"⚙  {taiKhoan.TenDangNhap}";
            btnQuanLyNhanSu.Visible = taiKhoan?.LaAdmin == true;
        }

        private void BtnCaiDatTaiKhoan_Click(object? sender, EventArgs e)
        {
            var taiKhoan = AppHost.Instance.TaiKhoanDangNhap;
            if (taiKhoan is null)
            {
                return;
            }

            using var form = new FormDoiMatKhau(taiKhoan, AppHost.Instance.TaiKhoanPhanMem);
            form.ShowDialog(this);
        }

        private void BtnDangXuat_Click(object? sender, EventArgs e)
        {
            if (!XacNhanHanhDongThoat(
                    "Bạn muốn đăng xuất khỏi phần mềm?",
                    "Vui lòng kiểm tra kĩ lại phiên chạy đã lưu chưa, bạn chắc chắn muốn đăng xuất chứ?",
                    "Tiếp tục đăng xuất"))
            {
                return;
            }

            DaDangXuat = true;
            chayTest1.HuyNeuDangChay();
            AppHost.Instance.DangXuat();
            Close();
        }

        private void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (DaDangXuat)
            {
                return;
            }

            if (!XacNhanHanhDongThoat(
                    "Bạn có muốn đóng phần mềm không?",
                    "Vui lòng kiểm tra kĩ lại phiên chạy đã lưu chưa, bạn chắc chắn muốn đóng phần mềm chứ?",
                    "Tiếp tục đóng"))
            {
                e.Cancel = true;
                return;
            }

            chayTest1.HuyNeuDangChay();
        }

        private bool XacNhanHanhDongThoat(string cauHoiMacDinh, string cauHoiKhiCoKetQua, string tenNutTiepTuc)
        {
            if (!chayTest1.CoKetQuaPhienChay)
            {
                return MessageBox.Show(
                    cauHoiMacDinh,
                    "HACTECH TEST",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes;
            }

            return FormXacNhanThoat.XacNhan(this, cauHoiKhiCoKetQua, tenNutTiepTuc);
        }

        private void btnBoSuuTap_Click(object sender, EventArgs e)
        {

        }
    }
}

